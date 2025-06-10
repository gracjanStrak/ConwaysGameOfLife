
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ConwaysGameOfLife.Models.enums;
using ConwaysGameOfLife.Utils;

namespace ConwaysGameOfLife.Models;

public class GameOfLife : INotifyPropertyChanged
{
    public const int ROWS = 2060;
    public const int COLS = 2060;
    public const int CELL_SIZE = 1;
    public const int BORDER_SIZE = 4;

    public const int WIDTH = COLS * CELL_SIZE + BORDER_SIZE * 2;
    public const int HEIGHT = ROWS * CELL_SIZE + BORDER_SIZE * 2;

    private byte[] cells;

    private bool _isRunning = false;

    private double _simulationSpeed = 0;

    private byte[] cellsPad;    
    private byte[] newCells;    
    private byte[] newCellsPad;

    private CancellationTokenSource _cts;

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


    string input = "#N Light speed oscillator 3\r\n#O Josh Ball\r\n#C A period 5 extensible oscillator.\r\n#C www.conwaylife.com/wiki/Light_Speed_Oscillator_3\r\nx = 73, y = 73, rule = b3/s23\r\n61bo11b$59b3o11b$56bobo3b2o9b$54b3ob4o2bob2o5b$53bo7bob2obo6b$52bob5ob\r\no2bo2bo2bo3b$14b2o36b2o3bob2o2bobob3o3b$14b2o34b2o3bo8bobo6b$49bobob2o\r\nbo8bo2b2o3b$14b4o3b2o26bo2bobobo5bo3b3obo2b$14bo3bo2bo28bobo2b2o4bobo\r\n6bo2b$17bobobo29bo2b2o6bo5b2ob2o$17b3obobo22b2o6b3o9b2obobob$19bob2obo\r\n21bobo5b2o10bo2bobob$6b2ob2o7bo5bo23bo2b2o14bob2o2b$6b2obo7b2ob5ob2o\r\n17bo2b5o13b2o5b$9bo16b2o17b3o4bo7bob3o2bob2o2b$9bob2o2bo4bob3o5bo2bo2b\r\no2bo2bo5bo2bo7b4o2bobobo3b$10bobob2o3bo5b23obo9b3ob2o2bobo3b$11b3o4bo\r\n6bo25bo12bobobo4b$15bobo4bo3bo2b3o2b3o2b3o2b5o2b2o3b3o3b2o2b2o5b$9b5ob\r\no6bo3b2o3b2o3b2o3b2o4bo4b2obob2o2bo2b2o7b$9bo3bobobo2b2o2b3o2b3o2b3o2b\r\n3o2b5o8bo4bo2bo7b$12bo2bobo6bo29bo2bo5b2o8b$13b3obo4b2ob23o2bobo2bob3o\r\n13b$18b2o2bobo2bo2bo2bo2bo2bo2bo2bo2bob3obobo3bo12b$15b2obob3obo23bobo\r\nbobobo2b2o12b$15b2obo2bo2b2o21b2obobobob2o15b$18bo5bo23bobobobo18b$18b\r\nobobobo23bo5bo18b$17b2obobob2o21b2o2bo2b2o17b$18bob3obo23bob3obo18b$\r\n18bo2bo2bo23bobobobo18b$17b2o5b2o21b2obobob2o17b$18bobobobo23bo5bo18b$\r\n18bobobobo23bo2bo2bo18b$17b2ob3ob2o21b2ob3ob2o17b$18bo2bo2bo23bobobobo\r\n18b$18bo5bo23bobobobo18b$17b2obobob2o21b2o5b2o17b$18bobobobo23bo2bo2bo\r\n18b$18bob3obo23bob3obo18b$17b2o2bo2b2o21b2obobob2o17b$18bo5bo23bobobob\r\no18b$18bobobobo23bo5bo18b$15b2obobobob2o21b2o2bo2bob2o15b$12b2o2bobobo\r\nbobo23bob3obob2o15b$12bo3bobob3obo2bo2bo2bo2bo2bo2bo2bo2bobo2b2o18b$\r\n13b3obo2bobo2b23ob2o4bob3o13b$8b2o5bo2bo29bo6bobo2bo12b$7bo2bo4bo8b5o\r\n2b3o2b3o2b3o2b3o2b2o2bobobo3bo9b$7b2o2bo2b2obob2o4bo4b2o3b2o3b2o3b2o3b\r\no6bob5o9b$5b2o2b2o3b3o3b2o2b5o2b3o2b3o2b3o2bo3bo4bobo15b$4bobobo12bo\r\n25bo6bo4b3o11b$3bobo2b2ob3o9bob23o5bo3b2obobo10b$3bobobo2b4o7bo2bo5bo\r\n2bo2bo2bo2bo5b3obo4bo2b2obo9b$2b2obo2b3obo7bo4b3o17b2o16bo9b$5b2o13b5o\r\n2bo17b2ob5ob2o7bob2o6b$2b2obo14b2o2bo23bo5bo7b2ob2o6b$bobo2bo10b2o5bob\r\no21bob2obo19b$bobob2o9b3o6b2o22bobob3o17b$2ob2o5bo6b2o2bo29bobobo17b$\r\n2bo6bobo4b2o2bobo28bo2bo3bo14b$2bob3o3bo5bobobo2bo26b2o3b4o14b$3b2o2bo\r\n8bob2obobo49b$6bobo8bo3b2o34b2o14b$3b3obobo2b2obo3b2o36b2o14b$3bo2bo2b\r\no2bob5obo52b$6bob2obo7bo53b$5b2obo2b4ob3o54b$9b2o3bobo56b$11b3o59b$11b\r\no!";



    public GameOfLife()
    {
        cells = RleParser.Parse(0, 0, RlePatterns.Turing, ROWS, COLS);
        cellsPad = new byte[(ROWS + 2) * (COLS + 2)];
        newCells = new byte[ROWS * COLS];
        newCellsPad = new byte[(ROWS + 2) * (COLS + 2)];

        Bitmap = new WriteableBitmap(WIDTH, HEIGHT, 96, 96, PixelFormats.Bgra32, null);

        DrawGrid();
        StartGameLoop();
    }


    public void StartGameLoop()
    {
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        var update = 0;
        int targetFrameTimeMs = 0; // np. 16 ms dla ~60 FPS

        Task.Run(async () =>
        {
            var stopwatch = new Stopwatch();

            update = 0;
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
                            if (cells[i] == 1) population++;
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
        int stride = Bitmap.BackBufferStride;
        byte[] pixels = new byte[HEIGHT * stride];

        for (int row = 0; row < ROWS; row++)
        {
            for (int col = 0; col < COLS; col++)
            {
                if (cells[GetIndex(row, col)] == 1)
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

    public void Clear()
    {
        cells = new byte[HEIGHT * Bitmap.BackBufferStride];
        DrawGrid();
    }


    private void StepSimulation()
    {
        UpdatePadding();
        Array.Clear(newCells, 0, newCells.Length);

        int W = COLS + 2;
        Parallel.For(0, ROWS, row =>
        {
            int rowPad = row + 1;
            int basePad = rowPad * W + 1;
            int abovePad = basePad - W;
            int belowPad = basePad + W;

            int idxAbove = abovePad;
            int idxBase = basePad;
            int idxBelow = belowPad;
            int dst = row * COLS;

            for (int col = 0; col < COLS; col++)
            {
                // sumowanie sąsiadów jako int:
                int count =
                    cellsPad[idxAbove - 1] + cellsPad[idxAbove] + cellsPad[idxAbove + 1] +
                    cellsPad[idxBase - 1] /* + cellsPad[idxBase] */ + cellsPad[idxBase + 1] +
                    cellsPad[idxBelow - 1] + cellsPad[idxBelow] + cellsPad[idxBelow + 1];

                // stan bieżący:
                byte alive = cellsPad[idxBase];
                newCells[dst] = (byte)(((alive == 1) && (count == 2 || count == 3) || (alive == 0 && count == 3)) ? 1 : 0);

                idxAbove++;
                idxBase++;
                idxBelow++;
                dst++;
            }
        });

        // swap buforów:
        var tmp = cells;
        cells = newCells;
        newCells = tmp;
    }


    private void UpdatePadding()
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

    private static byte[] LoadRandom(double chance = 0.15)
    {
        var rand = new Random();
        var cells = new byte[ROWS * COLS];

        for (int r = 0; r < ROWS; r++)
            for (int c = 0; c < COLS; c++)

                if (rand.NextDouble() < chance)
                {
                    cells[r * COLS + c] = 1;
                }
                else
                {
                    cells[r * COLS + c] = 0;
                }



        return cells;
    }

    public static byte[] GetInitialPattern(STARTING_LAYOUT layout)
    {
        switch (layout)
        {
            case STARTING_LAYOUT.RANDOM:
                return LoadRandom();

            default:
                return new byte[ROWS * COLS];
        }
    }

    public void SetPattern(STARTING_LAYOUT layout)
    {
        switch (layout)
        {
            case STARTING_LAYOUT.RANDOM: { cells = LoadRandom(); break; }
            case STARTING_LAYOUT.TURING: { cells = RleParser.Parse(0, 0, RlePatterns.Turing, ROWS, COLS); break; }
            case STARTING_LAYOUT.CORDER: { cells = RleParser.Parse(500, 500, RlePatterns.Corder, ROWS, COLS); break; }
            case STARTING_LAYOUT.OMATON: { cells = RleParser.Parse(0, 0, RlePatterns.Omaton, ROWS, COLS); break; }
            case STARTING_LAYOUT.GUNS: { cells = RleParser.Parse(500, 500, RlePatterns.Gun, ROWS, COLS); break; }
            case STARTING_LAYOUT.SPIRAL: { cells = RleParser.Parse(100, 100, RlePatterns.Spiral, ROWS, COLS); break; }
            case STARTING_LAYOUT.CALCULATOR: { cells = RleParser.Parse(0, 1300, RlePatterns.Calculator, ROWS, COLS); break; }
        }

        DrawGrid();
    }

    public void SetCustomPattern(int x,int y, string rle)
    {
        cells = RleParser.Parse(x, y, rle, ROWS, COLS);
        DrawGrid();
    }

    public void HandleLeftClick(int x, int y)
    {
        x -= BORDER_SIZE;
        y -= BORDER_SIZE;

        int col = x / CELL_SIZE;
        int row = y / CELL_SIZE;

        if (row >= 0 && row < ROWS && col >= 0 && col < COLS)
        {
            int index = row * COLS + col;
            //cells[row, col] = true;
            cells[index] = 1;
            DrawGrid();
        }
    }

    public void HandleRightClick(int x, int y)
    {
        x -= BORDER_SIZE;
        y -= BORDER_SIZE;

        int col = x / CELL_SIZE;
        int row = y / CELL_SIZE;

        if (row >= 0 && row < ROWS && col >= 0 && col < COLS)
        {
            int index = row * COLS + col;
            //cells[row, col] = true;
            cells[index] = 0;
            DrawGrid();
        }
    }

    public void SimulationSpeed(double simulationSpeed)
    {
        throw new NotImplementedException();
    }

    public void Reset()
    {
        cells = RleParser.Parse(0, 0, RlePatterns.Omaton, ROWS, COLS);
        DrawGrid();
    }


    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetIndex(int row, int col) => row * COLS + col;
}
