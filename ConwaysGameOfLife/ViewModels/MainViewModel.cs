using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ConwaysGameOfLife.ViewModels;

namespace ConwaysGameOfLife;

public enum STARTING_LAYOUT
{
    RANDOM,
    GLIDER_LINES,
    BLINKER_GRID,
    BLOCK_GRID,
    GLIDER_REACTOR,
    MANY_GLIDERS
}

public class MainViewModel : INotifyPropertyChanged
{
    // 44 80 10 4
    // 176 320 8 4
    public const int ROWS = 1000;
    public const int COLS = 2000;
    public const int CELL_SIZE = 1;
    public const int BORDER_SIZE = 4;

    public const int WIDTH = COLS * CELL_SIZE + BORDER_SIZE * 2;
    public const int HEIGHT = ROWS * CELL_SIZE + BORDER_SIZE * 2;

    private bool[] cells;

    private bool _isRunning = false;

    private double _simulationSpeed = 1;

    private bool[] cellsPad;    // rozmiar (ROWS+2)*(COLS+2)
    private bool[] newCells;    // rozmiar ROWS*COLS
    private bool[] newCellsPad; // rozmiar (ROWS+2)*(COLS+2)

    private int _generations;

    public int Generation
    {
        get => _generations;
        set
        {
            if (_generations != value)
            {
                _generations = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(GenerationText));
            }
        }
    }

    public string GenerationText => $"Generation: {_generations}";

    private int _fps;

    public int Fps
    {
        get => _fps;
        set
        {
            if (_fps != value)
            {
                _fps = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FpsText));
            }
        }
    }

    public string FpsText => $"FPS: {_fps}";

    private int _population;
    public int Population
    {
        get => _population;
        set
        {
            if (_population != value)
            {
                _population = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PopulationText));
            }
        }
    }

    public string PopulationText => $"Population: {_population}";

    public bool IsRunning
    {
        get => _isRunning;
        set
        {
            _isRunning = value;
            OnPropertyChanged(nameof(IsRunning));
        }
    }

    private WriteableBitmap _bitmap;
    public WriteableBitmap Bitmap
    {
        get => _bitmap;
        private set
        {
            _bitmap = value;
            OnPropertyChanged(nameof(Bitmap));
        }
    }

    public MainViewModel()
    {
        //cells = new bool[ROWS, COLS]; //empty
        cells = GetInitialPattern(STARTING_LAYOUT.MANY_GLIDERS);

        cellsPad = new bool[(ROWS + 2) * (COLS + 2)];
        newCells = new bool[ROWS * COLS];
        newCellsPad = new bool[(ROWS + 2) * (COLS + 2)];

        Bitmap = new WriteableBitmap(WIDTH, HEIGHT, 96, 96, PixelFormats.Bgra32, null);
        DrawGrid();
        StartGameLoop();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetIndex(int row, int col) => row * COLS + col;


    public void OnLeftClick(int x, int y)
    {
        x -= BORDER_SIZE;
        y -= BORDER_SIZE;

        int col = x / CELL_SIZE;
        int row = y / CELL_SIZE;

        if (row >= 0 && row < ROWS && col >= 0 && col < COLS)
        {
            int index = row * COLS + col;
            //cells[row, col] = true;
            cells[index] = true;
            DrawGrid();
        }
    }

    public void OnRightClick(int x, int y)
    {
        x -= BORDER_SIZE;
        y -= BORDER_SIZE;

        int col = x / CELL_SIZE;
        int row = y / CELL_SIZE;

        if (row >= 0 && row < ROWS && col >= 0 && col < COLS)
        {
            int index = row * COLS + col;
            //cells[row, col] = true;
            cells[index] = false;
            DrawGrid();
        }
    }

    private DispatcherTimer _timer = new DispatcherTimer();
    public void SimulationSpeed(double simulationSpeed)
    {
        _simulationSpeed = simulationSpeed;
        if (_timer != null)
        {
            _timer.Interval = TimeSpan.FromMilliseconds(_simulationSpeed);
        }
    }

    public void Reset()
    {
        //cells = new bool[WIDTH * HEIGHT];
        cells = GetInitialPattern(STARTING_LAYOUT.RANDOM);
        Generation = 0;
        DrawGrid();
    }

    private CancellationTokenSource _cts;

    public void StartGameLoop()
    {
        //_timer = new DispatcherTimer();
        //_timer.Interval = TimeSpan.FromMilliseconds(_simulationSpeed);
        //_timer.Tick += async (s, e) =>
        //{
        //    if (IsRunning)
        //    {
        //        await Task.Run(() =>
        //        {
        //            StepSimulation();
        //        });
        //        Generation += 1;
        //        DrawGrid();
        //    }
        //};
        //_timer.Start();

        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        int targetFrameTimeMs = (int)_simulationSpeed; // np. 16 ms dla ~60 FPS

        Task.Run(async () =>
        {
            var stopwatch = new Stopwatch();
            var update = 0;

            while (!token.IsCancellationRequested)
            {
                stopwatch.Restart();

                if (IsRunning)
                {
                    var timer = Stopwatch.StartNew(); // can be delted
                    StepSimulation();
                    Generation += 1;

                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        DrawGrid();
                    });

                    stopwatch.Stop(); // can be deleted
                    //Debug.WriteLine("Czas wykonania: " + stopwatch.ElapsedMilliseconds + " ms");

                    if (update == 5)
                    {
                        Fps = (int)(1000 / stopwatch.ElapsedMilliseconds);
                        int population = 0;
                        for (int i = 0; i < cells.Length; i++)
                        {
                            if (cells[i]) population++;
                        }

                        Population = population; 
                        update = 0;
                    }
                    update++;
                }

                stopwatch.Stop();

                int delay = Math.Max(0, targetFrameTimeMs - (int)stopwatch.ElapsedMilliseconds);
                await Task.Delay(delay);
            }
        }, token);
    }

    public void StopGameLoop()
    {
        _cts?.Cancel();
        _isRunning = false;
    }

    private void DrawGrid()
    {
        Clear();

        int stride = Bitmap.BackBufferStride;
        byte[] pixels = new byte[HEIGHT * stride];

        for (int row = 0; row < ROWS; row++)
        {
            for (int col = 0; col < COLS; col++)
            {
                if (cells[GetIndex(row, col)])
                {
                    int x = BORDER_SIZE + col * CELL_SIZE;
                    int y = BORDER_SIZE + row * CELL_SIZE;
                    DrawSquare(x, y, CELL_SIZE, pixels);
                }
            }
        }

        DrawBorder(pixels, BORDER_SIZE);

        Bitmap.WritePixels(
            new Int32Rect(0, 0, WIDTH, HEIGHT),
            pixels,
            stride,
            0
        );
    }

    private void Clear()
    {
        Bitmap.WritePixels(
            new Int32Rect(0, 0, WIDTH, HEIGHT),
            new byte[HEIGHT * Bitmap.BackBufferStride],
            Bitmap.BackBufferStride,
            0
        );
    }


    private void StepSimulation()
    {
        //Stopwatch stopwatch = Stopwatch.StartNew();

        //bool[] newCells = new bool[ROWS * COLS];

        //Parallel.For(0, ROWS, row =>
        //{
        //    for (int col = 0; col < COLS; col++)
        //    {
        //        int adjacent = CountAdjacent(row, col);
        //        bool isAlive = cells[GetIndex(row, col)];
        //        int index = GetIndex(row, col);

        //        if (isAlive && (adjacent == 2 || adjacent == 3))
        //        {
        //            newCells[index] = true;
        //        }
        //        else if (!isAlive && adjacent == 3)
        //        {
        //            newCells[index] = true;
        //        }
        //        else
        //        {
        //            newCells[index] = false;
        //        }
        //    }

        //});

        //cells = newCells;
        //stopwatch.Stop();
        //Debug.WriteLine("Czas wykonania: " + stopwatch.ElapsedMilliseconds + " ms");


        UpdatePadding();

        Array.Clear(newCells, 0, newCells.Length);

        int W = COLS + 2;
        Parallel.For(0, ROWS, row =>
        {
            int rowPad = row + 1;
            int basePad = rowPad * W + 1;
            int abovePad = basePad - W;
            int belowPad = basePad + W;

            for (int col = 0; col < COLS; col++)
            {
                //original
                int idx = row * COLS + col;

                int center = basePad + col;
                int count =
                    (cellsPad[abovePad + col - 1] ? 1 : 0) +
                    (cellsPad[abovePad + col] ? 1 : 0) +
                    (cellsPad[abovePad + col + 1] ? 1 : 0) +
                    (cellsPad[basePad + col - 1] ? 1 : 0) +
                    (cellsPad[basePad + col + 1] ? 1 : 0) +
                    (cellsPad[belowPad + col - 1] ? 1 : 0) +
                    (cellsPad[belowPad + col] ? 1 : 0) +
                    (cellsPad[belowPad + col + 1] ? 1 : 0);

                bool alive = cellsPad[center];
                newCells[idx] = (alive && (count == 2 || count == 3)) || (!alive && count == 3);
            }
        });


        cells = newCells;

        var tmp = newCells;
        newCells = cells;
        cells = tmp;
    }

    // OLD METHOD
    private static readonly int[] DX = { -1, -1, -1, 0, 0, 1, 1, 1 };
    private static readonly int[] DY = { -1, 0, 1, -1, 1, -1, 0, 1 };
    [Obsolete("Count Adjecent is depricated use UpdatePadding insted")]
    private int CountAdjacent(int row, int col)
    {
        int count = 0;

        for (int i = 0; i < DX.Length; i++)
        {
            int r = row + DX[i];
            int c = col + DY[i];

            if (r >= 0 && r < ROWS && c >= 0 && c < COLS && cells[GetIndex(r, c)])
                count++;
        }

        return count;
    }

    // NEW METHOD
    void UpdatePadding()
    {
        int W = COLS + 2;
        Array.Clear(cellsPad, 0, cellsPad.Length);
        for (int r = 0; r < ROWS; r++)
        {
            int srcBase = r * COLS;
            int dstBase = (r + 1) * W + 1;
            Array.Copy(cells, srcBase, cellsPad, dstBase, COLS);
        }
    }

    private void DrawSquare(int x, int y, int side, byte[] pixels)
    {
        for (int _y = y; _y < y + side; _y++)
        {
            for (int _x = x; _x < x + side; _x++)
            {
                int index = _y * Bitmap.BackBufferStride + _x * 4;
                if (index + 3 < pixels.Length)
                {
                    pixels[index + 0] = 250;     // Blue
                    pixels[index + 1] = 189;   // Green
                    pixels[index + 2] = 47;     // Red
                    pixels[index + 3] = 255;   // Alpha
                }
            }
        }
    }

    private void DrawBorder(byte[] pixels, int size)
    {
        int stride = Bitmap.BackBufferStride;
        int width = Bitmap.PixelWidth;
        int height = Bitmap.PixelHeight;

        // Góra i dół
        for (int x = 0; x < width; x++)
        {
            for (int w = 0; w < size; w++)
            {
                int top = w * stride + x * 4;
                int bottom = (height - 1 - w) * stride + x * 4;

                if (top + 3 < pixels.Length)
                {
                    pixels[top + 0] = 146; // Blue
                    pixels[top + 1] = 131;   // Green
                    pixels[top + 2] = 116;   // Red
                    pixels[top + 3] = 255; // Alpha
                }

                if (bottom + 3 < pixels.Length)
                {
                    pixels[bottom + 0] = 146; // Blue
                    pixels[bottom + 1] = 131;   // Green
                    pixels[bottom + 2] = 116;   // Red
                    pixels[bottom + 3] = 255; // Alpha
                }
            }
        }

        // Lewa i prawa
        for (int y = 0; y < height; y++)
        {
            for (int w = 0; w < size; w++)
            {
                int left = y * stride + w * 4;
                int right = y * stride + (width - 1 - w) * 4;

                if (left + 3 < pixels.Length)
                {
                    pixels[left + 0] = 146; // Blue
                    pixels[left + 1] = 131;   // Green
                    pixels[left + 2] = 116;   // Red
                    pixels[left + 3] = 255; // Alpha
                }

                if (right + 3 < pixels.Length)
                {
                    pixels[right + 0] = 146; // Blue
                    pixels[right + 1] = 131;   // Green
                    pixels[right + 2] = 116;   // Red
                    pixels[right + 3] = 255; // Alpha
                }
            }
        }
    }

    public static bool[] GetInitialPattern(STARTING_LAYOUT layout)
    {
        switch (layout)
        {
            case STARTING_LAYOUT.RANDOM:
                return LoadRandom();

            case STARTING_LAYOUT.GLIDER_LINES:
                return BoardPresets.PresetGliderLines();

            case STARTING_LAYOUT.BLINKER_GRID:
                return BoardPresets.PresetBlinkerGrid();

            case STARTING_LAYOUT.BLOCK_GRID:
                return BoardPresets.PresetBlockGrid();

            case STARTING_LAYOUT.GLIDER_REACTOR:
                return BoardPresets.PresetGliderReactor();
            case STARTING_LAYOUT.MANY_GLIDERS:
                return BoardPresets.GetManyGliderGuns(ROWS, COLS);

            default:
                return new bool[ROWS * COLS];
        }
    }

    private static bool[] LoadRandom(double chance = 0.15)
    {
        var rand = new Random();
        var cells = new bool[ROWS * COLS];

        for (int r = 0; r < ROWS; r++)
            for (int c = 0; c < COLS; c++)
                cells[r * COLS + c] = rand.NextDouble() < chance;

        return cells;
    }


    public event PropertyChangedEventHandler? PropertyChanged;
    //private void OnPropertyChanged(string name) =>
    //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
