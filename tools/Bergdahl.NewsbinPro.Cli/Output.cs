namespace Bergdahl.NewsbinPro.Cli;

internal static class Output
{
    /// <summary>Formats bytes with decimal units, the same way Newsbin shows sizes.</summary>
    public static string Bytes(long bytes)
    {
        string[] units = { "B", "KB", "MB", "GB", "TB" };
        double value = bytes;
        int unit = 0;
        while (Math.Abs(value) >= 1000 && unit < units.Length - 1)
        {
            value /= 1000;
            unit++;
        }
        return unit == 0 ? $"{bytes} B" : $"{value:0.##} {units[unit]}";
    }

    /// <summary>Formats a Newsbin speed, which is in bits per second.</summary>
    public static string Speed(long bitsPerSecond) =>
        $"{Bytes(bitsPerSecond / 8)}/s ({bitsPerSecond / 1_000_000.0:0.#} Mbit/s)";

    /// <summary>Prints rows as aligned columns. The last column isn't padded, so long subjects can wrap.</summary>
    public static void Table(IReadOnlyList<string[]> rows, bool header = false)
    {
        if (rows.Count == 0)
            return;
        int columns = rows.Max(r => r.Length);
        int[] widths = new int[columns];
        foreach (string[] row in rows)
            for (int i = 0; i < row.Length - 1; i++)
                widths[i] = Math.Max(widths[i], row[i].Length);

        for (int r = 0; r < rows.Count; r++)
        {
            string[] row = rows[r];
            var line = new System.Text.StringBuilder();
            for (int i = 0; i < row.Length; i++)
            {
                if (i > 0)
                    line.Append("  ");
                line.Append(i < row.Length - 1 ? row[i].PadRight(widths[i]) : row[i]);
            }
            Console.WriteLine(line.ToString().TrimEnd());
            if (header && r == 0)
                Console.WriteLine(new string('-', Math.Min(line.Length, Math.Max(ConsoleWidth() - 1, 20))));
        }
    }

    // IDE run consoles aren't terminals, and the width can be unavailable or zero there.
    private static int ConsoleWidth()
    {
        if (Console.IsOutputRedirected)
            return 80;
        try
        {
            return Console.WindowWidth > 0 ? Console.WindowWidth : 80;
        }
        catch (IOException)
        {
            return 80;
        }
    }
}
