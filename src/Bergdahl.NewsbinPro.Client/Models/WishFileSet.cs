using System;
using Bergdahl.NewsbinPro.Client.Internal;

namespace Bergdahl.NewsbinPro.Client;

/// <summary>
/// An entry in the wish list (<c>WISH LIST</c>).
/// </summary>
public class WishFileSet
{
    /// <summary>The id used by <see cref="NewsbinClient.DownloadFromWish(int)"/>.</summary>
    public int Id { get; }
    /// <summary>The subject.</summary>
    public string Subject { get; }
    /// <summary>Total size in bytes.</summary>
    public long FileSize { get; }
    /// <summary>Post time (UTC).</summary>
    public DateTime TimeStamp { get; }
    /// <summary>The poster.</summary>
    public string Poster { get; }
    /// <summary>Download folder.</summary>
    public string DownloadPath { get; }
    /// <summary>The raw status string, starting with <c>[n files, m pars]</c>.</summary>
    public string StatusString { get; }
    /// <summary>Number of files.</summary>
    public int FileCount { get; }
    /// <summary>Number of par files.</summary>
    public int ParCount { get; }

    /// <summary>Parses a tab separated data line.</summary>
    public WishFileSet(string data)
    {
        string[] fields = Parse.Fields(data, 9, nameof(WishFileSet));
        Id = Parse.Int(fields[0]);
        Subject = fields[1];
        FileSize = Parse.Long(fields[2]);
        TimeStamp = Parse.UnixTime(fields[3]);
        Poster = fields[4];
        DownloadPath = fields[5];
        StatusString = fields[8];
        Parse.FileAndParCount(StatusString, out int fileCount, out int parCount);
        FileCount = fileCount;
        ParCount = parCount;
    }

    /// <inheritdoc />
    public override string ToString() =>
        $"id={Id};subject={Subject};fileSize={FileSize};timeStamp={TimeStamp:o};poster={Poster};downloadPath={DownloadPath};fileCount={FileCount};parCount={ParCount}";
}
