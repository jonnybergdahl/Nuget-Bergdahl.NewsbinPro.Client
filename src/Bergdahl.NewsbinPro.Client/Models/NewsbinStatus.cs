using System.Collections.Generic;
using Bergdahl.NewsbinPro.Client.Internal;

namespace Bergdahl.NewsbinPro.Client;

/// <summary>
/// The result of the <c>STATUS</c> command.
/// </summary>
public class NewsbinStatus
{
    private readonly List<long> speedSamples = new List<long>();

    /// <summary>
    /// Current download speed in bits per second, averaged over the last few seconds. Divide by 8 for bytes per second.
    /// </summary>
    public int Speed { get; private set; }

    /// <summary>Free space in the download folder, in bytes.</summary>
    public long DownloadFolderFreeSpace { get; private set; }

    /// <summary>Free space in the data folder, in bytes.</summary>
    public long DataFolderFreeSpace { get; private set; }

    /// <summary>Total size of queued files, in bytes.</summary>
    public long QueuedFileSize { get; private set; }

    /// <summary>Recent speed samples in bits per second, newest first, about one every 2 seconds (Newsbin sends kbit/s).</summary>
    public IReadOnlyList<long> SpeedSamples => speedSamples;

    internal void Add(string data)
    {
        string[] fields = data.Split('\t');
        if (fields.Length < 2)
            return;
        switch (fields[0])
        {
            case "Speed":
                Speed = Parse.Int(fields[1]);
                break;
            case "DownloadFolder":
                DownloadFolderFreeSpace = Parse.Long(fields[1]);
                break;
            case "DataFolder":
                DataFolderFreeSpace = Parse.Long(fields[1]);
                break;
            case "Queued":
                QueuedFileSize = Parse.Long(fields[1]);
                break;
            case "Speed Samples":
                for (int i = 1; i < fields.Length; i++)
                {
                    if (fields[i].Length > 0)
                        speedSamples.Add(Parse.Long(fields[i]) * 1000);
                }
                break;
        }
    }

    internal static NewsbinStatus FromLines(IEnumerable<string> lines)
    {
        var status = new NewsbinStatus();
        foreach (string line in lines)
            status.Add(line);
        return status;
    }
}
