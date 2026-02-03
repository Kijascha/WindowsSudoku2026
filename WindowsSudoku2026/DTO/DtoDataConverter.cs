using System.IO;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WindowsSudoku2026.Common.Enums;
using WindowsSudoku2026.Common.Models;
using WindowsSudoku2026.Common.Utils.Colors;

namespace WindowsSudoku2026.DTO;

public static class DtoDataConverter
{
    public static string? EncodeBitmapSource(BitmapSource? bitmap)
    {
        if (bitmap == null)
            return null;

        var encoder = new PngBitmapEncoder(); // PNG, verlustfrei, kompakt
        encoder.Frames.Add(BitmapFrame.Create(bitmap));

        using var ms = new MemoryStream();
        encoder.Save(ms);
        return Convert.ToBase64String(ms.ToArray());
    }
    public static BitmapSource? DecodeBitmapSource(string? base64)
    {
        if (string.IsNullOrEmpty(base64))
            return null;

        byte[] bytes;
        try
        {
            bytes = Convert.FromBase64String(base64);
        }
        catch
        {
            return null; // invalid base64
        }

        using var ms = new MemoryStream(bytes);
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.StreamSource = ms;
        bitmap.EndInit();
        bitmap.Freeze(); // optional, macht Bitmap thread-safe

        return bitmap;
    }
    public static byte[]? EncodeBitmapSourceToBytes(BitmapSource? bitmap)
    {
        if (bitmap == null)
            return null;

        var encoder = new PngBitmapEncoder(); // PNG bleibt ideal für Sudoku-Grids
        encoder.Frames.Add(BitmapFrame.Create(bitmap));

        using var ms = new MemoryStream();
        encoder.Save(ms);

        // Wir geben direkt das Roh-Array zurück, das SQLite als BLOB speichert
        return ms.ToArray();
    }

    public static BitmapSource? DecodeBitmapSourceFromBytes(byte[]? bytes)
    {
        // Prüfung auf null oder leeres Array (wichtig für DB-Einträge)
        if (bytes == null || bytes.Length == 0)
            return null;

        try
        {
            using var ms = new MemoryStream(bytes);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            // OnLoad sorgt dafür, dass der Stream sofort gelesen und geschlossen werden kann
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = ms;
            bitmap.EndInit();

            // Freeze ist essenziell für die UI-Performance und Thread-Sicherheit
            bitmap.Freeze();

            return bitmap;
        }
        catch
        {
            // Falls die Binärdaten korrupt sind
            return null;
        }
    }


    public static string EncodeSolution(IPuzzle puzzle)
    {
        var sb = new StringBuilder(IPuzzle.Size * (IPuzzle.Size + 1)); // grob voralloc

        for (int r = 0; r < IPuzzle.Size; r++)
        {
            for (int c = 0; c < IPuzzle.Size; c++)
            {
                sb.Append(puzzle.Solution[r, c]);
            }
            if (r < IPuzzle.Size - 1)
                sb.Append('_');
        }

        return sb.ToString();
    }
    public static bool TryDecodeSolution(IPuzzle puzzle, string encoded)
    {
        var rows = encoded.Split('_', StringSplitOptions.RemoveEmptyEntries);
        if (rows.Length != IPuzzle.Size)
            return false; // ungültige Row-Anzahl

        for (int r = 0; r < IPuzzle.Size; r++)
        {
            var row = rows[r];
            if (row.Length != IPuzzle.Size)
                return false; // ungültige Anzahl Cells in Row

            for (int c = 0; c < IPuzzle.Size; c++)
            {
                char ch = row[c];
                if (ch < '0' || ch > '9')
                    return false; // ungültiges Zeichen

                puzzle.Solution[r, c] = ch - '0';
            }
        }

        return true; // erfolgreich
    }
    public static string EncodeDigits(IPuzzle puzzle)
    {
        var sb = new StringBuilder(IPuzzle.Size * (IPuzzle.Size + 1)); // grob voralloc

        for (int r = 0; r < IPuzzle.Size; r++)
        {
            for (int c = 0; c < IPuzzle.Size; c++)
            {
                sb.Append(puzzle[r, c].Digit);
            }
            if (r < IPuzzle.Size - 1)
                sb.Append('_');
        }

        return sb.ToString();
    }
    public static bool TryDecodeDigits(IPuzzle puzzle, string encoded)
    {
        var rows = encoded.Split('_', StringSplitOptions.RemoveEmptyEntries);
        if (rows.Length != IPuzzle.Size)
            return false; // ungültige Row-Anzahl

        for (int r = 0; r < IPuzzle.Size; r++)
        {
            var row = rows[r];
            if (row.Length != IPuzzle.Size)
                return false; // ungültige Anzahl Cells in Row

            for (int c = 0; c < IPuzzle.Size; c++)
            {
                char ch = row[c];
                if (ch < '0' || ch > '9')
                    return false; // ungültiges Zeichen

                puzzle[r, c].Digit = ch - '0';
            }
        }

        return true; // erfolgreich
    }
    public static string EncodeBitmasks(IPuzzle puzzle, Func<Cell, Candidates> selector)
    {
        var sb = new StringBuilder(IPuzzle.Size * IPuzzle.Size * 10); // grob voralloc

        for (int r = 0; r < IPuzzle.Size; r++)
        {
            for (int c = 0; c < IPuzzle.Size; c++)
            {
                var bitmask = selector(puzzle[r, c]);
                sb.Append(bitmask.ToString());

                if (c < IPuzzle.Size - 1)
                    sb.Append('|');
            }
            if (r < IPuzzle.Size - 1)
                sb.Append('_');
        }
        return sb.ToString();
    }
    public static bool TryDecodeBitmasks(IPuzzle puzzle, string encoded, Action<Cell, Candidates> setBitmask)
    {
        var rows = encoded.Split('_');
        if (rows.Length != IPuzzle.Size) return false;

        for (int r = 0; r < IPuzzle.Size; r++)
        {
            var cells = rows[r].Split('|');
            if (cells.Length != IPuzzle.Size) return false;

            for (int c = 0; c < IPuzzle.Size; c++)
            {
                var bits = cells[c];
                if (bits.Length != 9 || bits.Any(ch => ch != '0' && ch != '1'))
                    return false;

                setBitmask(puzzle[r, c], Candidates.FromString(bits));
            }
        }
        return true;
    }
    public static string EncodeCellColors(IPuzzle puzzle)
    {
        var rows = new string[9];

        for (int r = 0; r < 9; r++)
        {
            var cells = new string[9];

            for (int c = 0; c < 9; c++)
            {
                var colors = puzzle[r, c].CellColors; // List<SudokuCellColor>

                cells[c] = colors.Count == 0
                    ? "0"
                    : string.Join('|', colors.Select(c => ((int)c).ToString()));
            }

            rows[r] = string.Join('_', cells);
        }

        return string.Join(';', rows);
    }

    public static bool TryDecodeCellColors(IPuzzle puzzle, string encoded)
    {
        bool success = true; // wird false, falls etwas ungültig ist

        var rows = encoded.Split(';', StringSplitOptions.RemoveEmptyEntries);
        if (rows.Length != IPuzzle.Size)
            return false;

        for (int r = 0; r < IPuzzle.Size; r++)
        {
            var cells = rows[r].Split('_', StringSplitOptions.RemoveEmptyEntries);
            if (cells.Length != IPuzzle.Size)
                return false;

            for (int c = 0; c < IPuzzle.Size; c++)
            {
                puzzle[r, c].CellColors.Clear();

                if (cells[c] == "0")
                    continue;

                var values = cells[c].Split('|', StringSplitOptions.RemoveEmptyEntries);
                foreach (var v in values)
                {
                    if (int.TryParse(v, out int intVal) && Enum.IsDefined(typeof(SudokuCellColor), intVal))
                    {
                        puzzle[r, c].CellColors.Add((SudokuCellColor)intVal);
                    }
                    else
                    {
                        // ungültiger Wert → setze success auf false, aber nicht abbrechen
                        success = false;
                    }
                }
            }
        }

        return success;
    }
    public static string EncodeIsGiven(IPuzzle puzzle)
    {
        var sb = new StringBuilder(IPuzzle.Size * (IPuzzle.Size + 1));

        for (int r = 0; r < IPuzzle.Size; r++)
        {
            for (int c = 0; c < IPuzzle.Size; c++)
            {
                sb.Append(puzzle[r, c].IsGiven ? '1' : '0');
            }

            if (r < IPuzzle.Size - 1)
                sb.Append('_');
        }

        return sb.ToString();
    }
    public static bool TryDecodeIsGiven(IPuzzle puzzle, string encoded)
    {
        var rows = encoded.Split('_', StringSplitOptions.RemoveEmptyEntries);

        if (rows.Length != IPuzzle.Size)
            return false;

        for (int r = 0; r < IPuzzle.Size; r++)
        {
            var row = rows[r];

            if (row.Length != IPuzzle.Size)
                return false;

            for (int c = 0; c < IPuzzle.Size; c++)
            {
                char ch = row[c];

                if (ch == '1')
                    puzzle[r, c].IsGiven = true;
                else if (ch == '0')
                    puzzle[r, c].IsGiven = false;
                else
                    return false;
            }
        }

        return true;
    }
    // Wandelt die Liste in den DB-String um
    public static string EncodePalette(IEnumerable<SudokuColor> colors)
    {
        if (colors == null) return string.Empty;
        // Wir nehmen exakt/maximal 9
        return string.Join("_", colors.Take(9).Select(c => $"{(int)c.Key}:{c.Value}"));
    }

    // Wandelt den DB-String zurück in eine Liste
    public static List<SudokuColor> DecodePalette(string serialized)
    {
        if (string.IsNullOrWhiteSpace(serialized)) return new List<SudokuColor>();

        return serialized.Split('_', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => part.Split(':'))
            .Where(split => split.Length == 2)
            .Select(split => new SudokuColor
            {
                Key = (SudokuCellColor)int.Parse(split[0]),
                Value = (Color)ColorConverter.ConvertFromString(split[1])
            })
            .ToList();
    }
}
