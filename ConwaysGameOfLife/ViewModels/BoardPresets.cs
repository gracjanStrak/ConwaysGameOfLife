using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConwaysGameOfLife.ViewModels;
public class BoardPresets
{
    private const int ROWS = 400;
    private const int COLS = 800;

    public static bool[] PresetRandomNoise(double chance = 0.15)
    {
        var rand = new Random();
        var cells = new bool[400 * 800];
        for (int r = 0; r < 400; r++)
            for (int c = 0; c < 800; c++)
                cells[r * 800 + c] = rand.NextDouble() < chance;
        return cells;
    }

    public static bool[] PresetGliderLines()
    {
        var cells = new bool[400 * 800];

        int[,] glider = new int[,]
        {
        {1,0,0},
        {0,1,1},
        {1,1,0}
        };

        for (int row = 0; row < 400; row += 10)
        {
            for (int col = 0; col < 800; col += 20)
            {
                for (int y = 0; y < 3; y++)
                    for (int x = 0; x < 3; x++)
                        if (glider[y, x] == 1)
                            cells[(row + y) * 800 + (col + x)] = true;
            }
        }

        return cells;
    }

    public static bool[] PresetBlinkerGrid()
    {
        var cells = new bool[400 * 800];

        for (int row = 5; row < 400; row += 10)
        {
            for (int col = 5; col < 800; col += 10)
            {
                cells[row * 800 + col] = true;
                cells[(row + 1) * 800 + col] = true;
                cells[(row + 2) * 800 + col] = true;
            }
        }

        return cells;
    }

    public static bool[] PresetBlockGrid()
    {
        var cells = new bool[400 * 800];

        for (int row = 0; row < 400; row += 6)
        {
            for (int col = 0; col < 800; col += 6)
            {
                cells[(row + 0) * 800 + (col + 0)] = true;
                cells[(row + 0) * 800 + (col + 1)] = true;
                cells[(row + 1) * 800 + (col + 0)] = true;
                cells[(row + 1) * 800 + (col + 1)] = true;
            }
        }

        return cells;
    }

    public static bool[] PresetGliderReactor()
    {
        var cells = new bool[ROWS * COLS];
        // Gosper Glider Gun offsets
        int[,] gun = new int[,]
        {
            {0,24},{1,22},{1,24},{2,12},{2,13},{2,20},{2,21},{2,34},{2,35},
            {3,11},{3,15},{3,20},{3,21},{3,34},{3,35},
            {4,0},{4,1},{4,10},{4,16},{4,20},{4,21},
            {5,0},{5,1},{5,10},{5,14},{5,16},{5,17},{5,22},{5,24},
            {6,10},{6,16},{6,24},{7,11},{7,15},{8,12},{8,13}
        };
        void PlaceGun(int baseRow, int baseCol, bool mirror)
        {
            for (int i = 0; i < gun.GetLength(0); i++)
            {
                int r = baseRow + gun[i, 0];
                int c = baseCol + (mirror ? (COLS - gun[i, 1] - 1) : gun[i, 1]);
                if (r >= 0 && r < ROWS && c >= 0 && c < COLS)
                    cells[r * COLS + c] = true;
            }
        }
        // Place two guns facing each other
        PlaceGun(150, 5, false);
        PlaceGun(150, 5, true);
        // Place shields (blocks) behind guns
        for (int dr = 2; dr < 6; dr++)
            for (int dc = 0; dc < 2; dc++)
            {
                int leftR = 150 + dr;
                int leftC = 36 + dc;
                int rightR = 150 + dr;
                int rightC = COLS - 37 - dc;
                cells[leftR * COLS + leftC] = true;
                cells[rightR * COLS + rightC] = true;
            }
        return cells;
    }

    public static bool[] GetManyGliderGuns(int rows, int cols)
    {
        var cells = new bool[rows * cols];

        var gun = new List<(int x, int y)>
       {
        (24, 0),
        (22, 1), (24, 1),
        (12, 2), (13, 2), (20, 2), (21, 2), (34, 2), (35, 2),
        (11, 3), (15, 3), (20, 3), (21, 3), (34, 3), (35, 3),
        (0, 4), (1, 4), (10, 4), (16, 4), (20, 4), (21, 4),
        (0, 5), (1, 5), (10, 5), (14, 5), (16, 5), (17, 5), (22, 5), (24, 5),
        (10, 6), (16, 6), (24, 6),
        (11, 7), (15, 7),
        (12, 8), (13, 8)
    };

        int gunWidth = 36;
        int gunHeight = 11;
        int spacing = 100; // Odstęp poziomy między działkami

        int topOffsetY = 20;
        int bottomOffsetY = rows - gunHeight - 20;

        // Dodajemy Glider Guny co `spacing` kolumn
        for (int i = 0; i + gunWidth + 10 < cols; i += spacing)
        {
            // Górny rząd (strzelają w dół)
            foreach (var (x, y) in gun)
            {
                int index = (topOffsetY + y) * cols + (i + x);
                cells[index] = true;
            }

            // Dolny rząd (strzelają w górę – lustrzane odbicie w pionie)
            foreach (var (x, y) in gun)
            {
                int mirroredY = gunHeight - y;
                int index = (bottomOffsetY + mirroredY) * cols + (i + x);
                cells[index] = true;
            }
        }

        return cells;
    }
}
