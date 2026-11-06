using System;
using Bergdahl.NewsbinPro.Client.Internal;

namespace Bergdahl.NewsbinPro.Client;

/// <summary>The kind of entry in the download list.</summary>
public enum FileSetType
{
    /// <summary>A header download.</summary>
    Header,
    /// <summary>A file set being downloaded.</summary>
    Download,
    /// <summary>A file set being repaired with par files.</summary>
    Repair,
    /// <summary>A file set being unpacked.</summary>
    Unpack
}

/// <summary>
/// An entry in the download list (<c>DOWNLOADS LIST</c>).
/// </summary>
public class DownloadFileSet
{
    /// <summary>
    /// Newsbin's internal id. The download commands don't accept it; they take the zero-based position in the list.
    /// </summary>
    public int Id { get; }
    /// <summary>The kind of entry.</summary>
    public FileSetType FileSetType { get; }
    /// <summary>The subject, or the header download description for headers.</summary>
    public string Subject { get; }
    /// <summary>Size in bytes as listed for the entry (not set for headers). See <see cref="DownloadedSize"/> for the whole group.</summary>
    public long FileSize { get; }
    /// <summary>Post time (UTC, not set for headers).</summary>
    public DateTime TimeStamp { get; }
    /// <summary>The poster (not set for headers).</summary>
    public string Poster { get; } = "";
    /// <summary>Download folder (not set for headers).</summary>
    public string DownloadPath { get; } = "";
    /// <summary>
    /// Total bytes to download for downloads and headers, parsed from the progress text; 100 for repair and unpack.
    /// Newsbin groups a download with the rest of its set, including par files, so this can be larger than
    /// <see cref="FileSize"/>. Approximate, because Newsbin shows it rounded (for example <c>2.15 GB</c>).
    /// </summary>
    public long DownloadedSize { get; }
    /// <summary>Bytes done for downloads and headers (approximate, from the progress text); percent done for repair and unpack.</summary>
    public long DownloadedCurrentSize { get; }
    /// <summary>The raw status string.</summary>
    public string Status { get; }
    /// <summary>True if the entry is paused.</summary>
    public bool IsPaused { get; }
    /// <summary>File, par and part counts for the whole download group.</summary>
    public DownloadPartStatus PartStatus { get; } = new DownloadPartStatus();

    /// <summary>Parses a tab separated data line.</summary>
    public DownloadFileSet(string data)
    {
        string[] fields = Parse.Fields(data, 10, nameof(DownloadFileSet));
        Id = Parse.Int(fields[0]);
        Subject = fields[1];
        Status = fields[8];
        FileSetType = GetFileSetType(Subject, Status);
        IsPaused = fields[9] == "1";

        if (FileSetType == FileSetType.Header)
        {
            Subject = fields[4].Length == 0 ? "Header download" : fields[4];
            ParseProgress(fields[7], out long current, out long total);
            DownloadedCurrentSize = current;
            DownloadedSize = total;
            return;
        }

        FileSize = Parse.Long(fields[2]);
        TimeStamp = Parse.UnixTime(fields[3]);
        Poster = fields[4];
        DownloadPath = fields[5];

        if (FileSetType == FileSetType.Download)
        {
            ParseProgress(fields[7], out long current, out long total);
            DownloadedCurrentSize = current;
            DownloadedSize = total;
            PartStatus = new DownloadPartStatus(Status);
        }
        else
        {
            DownloadedSize = 100;
            DownloadedCurrentSize = Parse.Percent(Status);
        }
    }

    private static FileSetType GetFileSetType(string subject, string status)
    {
        if (subject.Length == 0)
            return FileSetType.Header;
        if (status.StartsWith("PAR", StringComparison.Ordinal))
            return FileSetType.Repair;
        if (status.StartsWith("UnRAR", StringComparison.Ordinal))
            return FileSetType.Unpack;
        return FileSetType.Download;
    }

    // Progress looks like "383.3 KB/5.92 GB".
    private static void ParseProgress(string value, out long current, out long total)
    {
        string[] parts = value.Split('/');
        current = parts.Length > 0 ? Parse.Size(parts[0]) : 0;
        total = parts.Length > 1 ? Parse.Size(parts[1]) : 0;
    }

    /// <inheritdoc />
    public override string ToString() =>
        $"id={Id};type={FileSetType};subject={Subject};fileSize={FileSize};done={DownloadedCurrentSize}/{DownloadedSize};paused={IsPaused};status={Status}";
}
