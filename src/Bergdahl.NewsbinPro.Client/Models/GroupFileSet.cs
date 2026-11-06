using System;
using Bergdahl.NewsbinPro.Client.Internal;

namespace Bergdahl.NewsbinPro.Client;

/// <summary>
/// A file set in the loaded group (<c>LOADED LIST</c> / <c>LOADED FIND</c>).
/// </summary>
public class GroupFileSet
{
    /// <summary>The id used by other loaded commands.</summary>
    public int Id { get; }
    /// <summary>The subject.</summary>
    public string Subject { get; }
    /// <summary>Total size in bytes of the whole set, including par files.</summary>
    public long FileSize { get; }
    /// <summary>Post time (UTC).</summary>
    public DateTime TimeStamp { get; }
    /// <summary>The poster.</summary>
    public string Poster { get; }
    /// <summary>
    /// A file count reported by Newsbin. It isn't a plain count: a set Newsbin shows as <c>[1 files, 9 pars]</c> reports 0 here.
    /// </summary>
    public int FileCount { get; }
    /// <summary>Number of par files in the set.</summary>
    public int ParCount { get; }
    /// <summary>True if the file set is marked as old.</summary>
    public bool IsOld { get; }

    /// <summary>Parses a tab separated data line.</summary>
    public GroupFileSet(string data)
    {
        string[] fields = Parse.Fields(data, 8, nameof(GroupFileSet));
        Id = Parse.Int(fields[0]);
        Subject = fields[1];
        FileSize = Parse.Long(fields[2]);
        TimeStamp = Parse.UnixTime(fields[3]);
        Poster = fields[4];
        FileCount = Parse.Int(fields[5]);
        ParCount = Parse.Int(fields[6]);
        IsOld = fields[7] == "0";
    }

    /// <inheritdoc />
    public override string ToString() =>
        $"id={Id};subject={Subject};fileSize={FileSize};timeStamp={TimeStamp:o};poster={Poster};fileCount={FileCount};parCount={ParCount};isOld={IsOld}";
}
