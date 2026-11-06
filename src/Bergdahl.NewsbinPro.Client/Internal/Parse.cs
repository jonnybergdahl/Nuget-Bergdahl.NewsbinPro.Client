using System;
using System.Globalization;

namespace Bergdahl.NewsbinPro.Client.Internal;

internal static class Parse
{
    public static string[] Fields(string data, int minimumCount, string typeName)
    {
        if (data is null)
            throw new ArgumentNullException(nameof(data));
        string[] fields = data.Split('\t');
        if (fields.Length < minimumCount)
            throw new FormatException($"{typeName} needs at least {minimumCount} tab separated fields but got {fields.Length}: '{data}'");
        return fields;
    }

    public static int Int(string value) =>
        int.Parse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture);

    public static long Long(string value) =>
        long.Parse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture);

    public static int IntOrZero(string value) =>
        int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int result) ? result : 0;

    public static DateTime UnixTime(string value) =>
        DateTimeOffset.FromUnixTimeSeconds(Long(value)).UtcDateTime;

    public static string Invariant(long value) => value.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Parses sizes such as <c>383.3 KB</c> or <c>5.92 GB</c> into bytes. Newsbin uses decimal units: a 3,486,062,667 byte
    /// download is shown as <c>3.49 GB</c>. The result is approximate because of the rounding in the text.
    /// </summary>
    public static long Size(string value)
    {
        string[] parts = value.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return 0;
        double number = double.Parse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture);
        long multiplier = parts.Length < 2 ? 1 : parts[1].ToUpperInvariant() switch
        {
            "KB" => 1_000L,
            "MB" => 1_000_000L,
            "GB" => 1_000_000_000L,
            "TB" => 1_000_000_000_000L,
            _ => 1L
        };
        return (long)Math.Round(number * multiplier);
    }

    /// <summary>Returns the number in front of the first <c>%</c>, for example 45 for <c>PAR Repair: 45%</c>.</summary>
    public static long Percent(string value)
    {
        int end = value.IndexOf('%');
        if (end <= 0)
            return 0;
        int start = end;
        while (start > 0 && (char.IsDigit(value[start - 1]) || value[start - 1] == '.'))
            start--;
        if (start == end)
            return 0;
        return double.TryParse(value.Substring(start, end - start), NumberStyles.Float, CultureInfo.InvariantCulture, out double percent)
            ? (long)percent
            : 0;
    }

    /// <summary>Parses the <c>[102 files, 16 pars]</c> prefix into file and par counts.</summary>
    public static void FileAndParCount(string value, out int fileCount, out int parCount)
    {
        fileCount = 0;
        parCount = 0;
        int open = value.IndexOf('[');
        int close = value.IndexOf(']');
        if (open < 0 || close < open)
            return;
        string[] parts = value.Substring(open + 1, close - open - 1).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 1)
            fileCount = IntOrZero(parts[0]);
        if (parts.Length >= 3)
            parCount = IntOrZero(parts[2]);
    }
}
