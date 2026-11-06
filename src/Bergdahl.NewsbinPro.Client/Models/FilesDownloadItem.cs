using System;
using Bergdahl.NewsbinPro.Client.Internal;

namespace Bergdahl.NewsbinPro.Client;

/// <summary>
/// File name and size announced by Newsbin before a file download, from a message like <c>"name.ext" 12345</c>.
/// </summary>
public class FilesDownloadItem
{
    /// <summary>The file name.</summary>
    public string FileName { get; set; }

    /// <summary>The file size in bytes.</summary>
    public long FileSize { get; set; }

    /// <summary>Parses the message of the <c>FILES DOWNLOAD</c> response.</summary>
    public FilesDownloadItem(string value)
    {
        if (value is null)
            throw new ArgumentNullException(nameof(value));
        string[] parts = value.Split('"');
        if (parts.Length < 3)
            throw new FormatException($"Expected a quoted file name followed by a size: '{value}'");
        FileName = parts[1];
        FileSize = Parse.Long(parts[2]);
    }
}
