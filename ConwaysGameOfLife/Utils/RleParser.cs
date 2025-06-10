using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConwaysGameOfLife.Utils;
public static class RleParser
{
    /// <summary>
    /// Parsuje RLE (Run Length Encoded) z input i zwraca byte[] o rozmiarze rows*cols,
    /// gdzie 1 = żywa komórka, 0 = martwa. Wzór w RLE umieszczany jest z przesunięciem
    /// xOffset, yOffset w docelowej siatce. Jeśli wzór wykracza poza granice, jest obcięty.
    /// </summary>
    public static byte[] Parse(int xOffset, int yOffset, string input, int rows, int cols)
    {
        var result = new byte[rows * cols];
        var lines = input.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        bool headerParsed = false;
        var bodySb = new StringBuilder();

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith("#")) continue;
            if (!headerParsed && line.StartsWith("x", StringComparison.OrdinalIgnoreCase))
            {
                headerParsed = true;
                continue;
            }
            bodySb.Append(line);
        }

        string body = bodySb.ToString();
        int row = 0, col = 0;
        int run = 0;

        for (int i = 0; i < body.Length; i++)
        {
            char c = body[i];
            if (char.IsDigit(c))
            {
                run = run * 10 + (c - '0');
            }
            else if (c == 'o' || c == 'b')
            {
                int count = (run == 0) ? 1 : run;
                run = 0;
                bool isAlive = c == 'o';

                for (int k = 0; k < count; k++, col++)
                {
                    int targetRow = row + yOffset;
                    int targetCol = col + xOffset;

                    if (targetRow >= 0 && targetRow < rows && targetCol >= 0 && targetCol < cols)
                    {
                        result[targetRow * cols + targetCol] = (byte)(isAlive ? 1 : 0);
                    }
                }
            }
            else if (c == '$')
            {
                int count = (run == 0) ? 1 : run;
                run = 0;
                row += count;
                col = 0;
            }
            else if (c == '!')
            {
                break;
            }

            if (row + yOffset >= rows)
                break;
        }

        return result;
    }

}
