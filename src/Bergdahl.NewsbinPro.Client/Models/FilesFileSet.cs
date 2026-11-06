using System;
using Bergdahl.NewsbinPro.Client.Internal;

namespace Bergdahl.NewsbinPro.Client;

/// <summary>
/// A downloaded file on the Files tab (<c>FILES LIST</c>).
/// </summary>
public class FilesFileSet
{
    /// <summary>The id used by <see cref="NewsbinClient.DownloadFromFiles(System.IO.Stream, int)"/>.</summary>
    public int Id { get; }
    /// <summary>The subject.</summary>
    public string Subject { get; }
    /// <summary>Size in bytes.</summary>
    public long FileSize { get; }
    /// <summary>Post time (UTC).</summary>
    public DateTime TimeStamp { get; }
    /// <summary>The poster.</summary>
    public string Poster { get; }
    /// <summary>The file name on disk.</summary>
    public string FileName { get; }

    /// <summary>Parses a tab separated data line.</summary>
    public FilesFileSet(string data)
    {
        string[] fields = Parse.Fields(data, 6, nameof(FilesFileSet));
        Id = Parse.Int(fields[0]);
        Subject = fields[1];
        FileSize = Parse.Long(fields[2]);
        TimeStamp = Parse.UnixTime(fields[3]);
        Poster = fields[4];
        FileName = fields[5];
    }

    /// <inheritdoc />
    public override string ToString() =>
        $"id={Id};subject={Subject};fileSize={FileSize};timeStamp={TimeStamp:o};poster={Poster};fileName={FileName}";
}
