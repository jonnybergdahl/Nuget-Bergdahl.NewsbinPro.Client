using System;
using Bergdahl.NewsbinPro.Client.Internal;

namespace Bergdahl.NewsbinPro.Client;

/// <summary>
/// Part counts of a download, parsed from a status like <c>[68 files, 0 pars] D:19 DL:49 Inc:54 P:0 R:0</c>.
/// </summary>
public class DownloadPartStatus
{
    /// <summary>Number of files.</summary>
    public int FileCount { get; }
    /// <summary>Number of par files.</summary>
    public int ParCount { get; }
    /// <summary>Number of downloaded files (<c>D:</c>).</summary>
    public int Downloaded { get; }
    /// <summary>Number of files downloading (<c>DL:</c>).</summary>
    public int Downloading { get; }
    /// <summary>Number of incomplete files (<c>Inc:</c>, or <c>In:</c> in older versions).</summary>
    public int Incomplete { get; }
    /// <summary>Number of paused files (<c>P:</c>).</summary>
    public int Paused { get; }
    /// <summary>Number of retries (<c>R:</c>).</summary>
    public int Retries { get; }

    /// <summary>Creates an empty status.</summary>
    public DownloadPartStatus()
    {
    }

    /// <summary>Parses a status string. Unknown or malformed parts are left at zero.</summary>
    public DownloadPartStatus(string data)
    {
        if (data is null)
            throw new ArgumentNullException(nameof(data));
        int close = data.IndexOf(']');
        if (close == -1)
            return;

        Parse.FileAndParCount(data, out int fileCount, out int parCount);
        FileCount = fileCount;
        ParCount = parCount;

        foreach (string part in data.Substring(close + 1).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
        {
            string[] keyValue = part.Split(':');
            if (keyValue.Length != 2)
                continue;
            int value = Parse.IntOrZero(keyValue[1]);
            switch (keyValue[0])
            {
                case "D":
                    Downloaded = value;
                    break;
                case "DL":
                    Downloading = value;
                    break;
                case "In":
                case "Inc":
                    Incomplete = value;
                    break;
                case "P":
                    Paused = value;
                    break;
                case "R":
                    Retries = value;
                    break;
            }
        }
    }

    /// <inheritdoc />
    public override string ToString() =>
        $"fileCount:{FileCount};parCount:{ParCount};downloaded:{Downloaded};downloading:{Downloading};incomplete:{Incomplete};paused:{Paused};retries:{Retries}";
}
