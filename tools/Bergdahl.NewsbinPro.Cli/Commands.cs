using System.Diagnostics.CodeAnalysis;
using Bergdahl.NewsbinPro.Client;

namespace Bergdahl.NewsbinPro.Cli;

internal sealed record CommandInfo(
    string Name,
    string Arguments,
    string Description,
    Func<Session, string[], CancellationToken, Task> Run,
    bool NeedsConnection = true,
    string[]? Aliases = null);

internal static class Commands
{
    public static readonly CommandInfo[] All =
    {
        new("status", "", "Speed, free space, queue size, paused and bandwidth limiter state.", Status),
        new("pause", "", "Pause downloading.", (s, a, ct) => SetPaused(s, true, ct)),
        new("resume", "", "Resume downloading.", (s, a, ct) => SetPaused(s, false, ct)),
        new("limiter", "[on|off]", "Show or set the bandwidth limiter.", Limiter),

        new("downloads", "", "List the download queue. Positions start at 0, as delete and move expect.", Downloads, Aliases: new[] { "dl", "queue" }),
        new("delete", "<position>...", "Remove entries from the download queue (positions from 'downloads').", Delete),
        new("move", "<position> up|down|top|bottom", "Move an entry in the download queue.", Move),

        new("search", "<text> [--local] [--min-age N] [--max-age N] [--limit N] [--download N,...]",
            "Search the internet (or the local database with --local) and list the results.", Search),
        new("get", "<number>...", "Download results from the last search in this shell (numbers from 'search').", Get),

        new("files", "[count]", "List the newest files on the Files tab (default 20).", Files),
        new("wish", "", "List the wish list.", Wish),
        new("upload", "<file.nzb>...", "Upload NZB files.", Upload),

        new("servers", "", "List the news servers.", (s, a, ct) => PrintList(s, c => c.ListServers(), (c, t) => c.ListServersAsync(t), ct)),
        new("groups", "", "List the groups.", (s, a, ct) => PrintList(s, c => c.ListGroups(), (c, t) => c.ListGroupsAsync(t), ct)),
        new("gogs", "[name]", "List groups of groups with their groups, or the groups in one.", Gogs),
        new("filters", "", "List the filter profiles.", (s, a, ct) => PrintList(s, c => c.ListFilters(), (c, t) => c.ListFiltersAsync(t), ct)),

        new("call", "<Method> [arguments]", "Call any NewsbinClient method, for example 'call SetFilterAge 48'.", MethodCaller.CallAsync),
        new("config", "[host|port|password <value>] [unset <setting>]",
            "Show or change the saved connection settings.", ConfigFile.Command, NeedsConnection: false),
        new("methods", "[filter]", "List the methods 'call' can run.", MethodCaller.ListAsync, NeedsConnection: false),
        new("help", "[command]", "Show help.", (s, a, ct) => { Help.PrintCommands(a); return Task.CompletedTask; }, NeedsConnection: false, Aliases: new[] { "?" }),
    };

    public static bool TryGet(string name, [NotNullWhen(true)] out CommandInfo? command)
    {
        command = All.FirstOrDefault(c =>
            string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase) ||
            (c.Aliases?.Any(a => string.Equals(a, name, StringComparison.OrdinalIgnoreCase)) ?? false));
        return command != null;
    }

    private static async Task Status(Session s, string[] args, CancellationToken ct)
    {
        NewsbinStatus status = await s.Call(c => c.GetStatus(), (c, t) => c.GetStatusAsync(t), ct);
        bool paused = await s.Call(c => c.GetPausedState(), (c, t) => c.GetPausedStateAsync(t), ct);
        bool limiter = await s.Call(c => c.GetBandwidthLimiterState(), (c, t) => c.GetBandwidthLimiterStateAsync(t), ct);

        Output.Table(new[]
        {
            new[] { "Newsbin", $"{s.Client.NewsbinVersion ?? "?"} at {s.Client.ServerAddress}:{s.Client.Port}" },
            new[] { "Speed", Output.Speed(status.Speed) },
            new[] { "Queued", Output.Bytes(status.QueuedFileSize) },
            new[] { "Download folder free", Output.Bytes(status.DownloadFolderFreeSpace) },
            new[] { "Data folder free", Output.Bytes(status.DataFolderFreeSpace) },
            new[] { "Paused", paused ? "yes" : "no" },
            new[] { "Bandwidth limiter", limiter ? "on" : "off" },
        });
    }

    private static async Task SetPaused(Session s, bool pause, CancellationToken ct)
    {
        bool paused = await s.Call(c => c.SetPausedState(pause), (c, t) => c.SetPausedStateAsync(pause, t), ct);
        Console.WriteLine(paused ? "Downloading is paused." : "Downloading is running.");
    }

    private static async Task Limiter(Session s, string[] args, CancellationToken ct)
    {
        bool on;
        if (args.Length == 0)
        {
            on = await s.Call(c => c.GetBandwidthLimiterState(), (c, t) => c.GetBandwidthLimiterStateAsync(t), ct);
        }
        else
        {
            bool value = Args.Bool(args[0]);
            on = await s.Call(c => c.SetBandwidthLimiterState(value), (c, t) => c.SetBandwidthLimiterStateAsync(value, t), ct);
        }
        Console.WriteLine($"Bandwidth limiter is {(on ? "on" : "off")}.");
    }

    private static async Task Downloads(Session s, string[] args, CancellationToken ct)
    {
        long count = await s.Call(c => c.GetDownloadsCount(), (c, t) => c.GetDownloadsCountAsync(t), ct);
        if (count == 0)
        {
            Console.WriteLine("The download queue is empty.");
            return;
        }
        int n = (int)count;
        IReadOnlyList<DownloadFileSet> sets = await s.Call(c => c.GetDownloadsList(1, n), (c, t) => c.GetDownloadsListAsync(1, n, t), ct);

        var rows = new List<string[]> { new[] { "Pos", "Done", "Progress", "Parts", "State", "Subject" } };
        for (int i = 0; i < sets.Count; i++)
        {
            DownloadFileSet d = sets[i];
            string percent = d.DownloadedSize > 0 ? $"{100.0 * d.DownloadedCurrentSize / d.DownloadedSize:0}%" : "";
            string progress = d.FileSetType is FileSetType.Repair or FileSetType.Unpack
                ? $"{d.FileSetType} {d.DownloadedCurrentSize}%"
                : $"{Output.Bytes(d.DownloadedCurrentSize)} / {Output.Bytes(d.DownloadedSize)}";
            DownloadPartStatus p = d.PartStatus;
            string parts = $"{p.FileCount}f {p.ParCount}p  D:{p.Downloaded} DL:{p.Downloading} Inc:{p.Incomplete}";
            rows.Add(new[] { i.ToString(), percent, progress, parts, d.IsPaused ? "paused" : "", d.Subject });
        }
        Output.Table(rows, header: true);
    }

    private static async Task Delete(Session s, string[] args, CancellationToken ct)
    {
        if (args.Length == 0)
            throw new UsageException("Give one or more positions from 'downloads'.");
        // Delete from the highest position down so the remaining positions don't shift.
        foreach (int position in args.Select(a => Args.Int(a, "position")).Distinct().OrderByDescending(p => p))
        {
            await s.Call(c => c.DeleteDownload(position), (c, t) => c.DeleteDownloadAsync(position, t), ct);
            Console.WriteLine($"Deleted position {position}.");
        }
    }

    private static async Task Move(Session s, string[] args, CancellationToken ct)
    {
        if (args.Length != 2)
            throw new UsageException("Give a position and a direction.");
        int position = Args.Int(args[0], "position");
        switch (args[1].ToLowerInvariant())
        {
            case "up":
                await s.Call(c => c.DownloadMoveUp(position), (c, t) => c.DownloadMoveUpAsync(position, t), ct);
                break;
            case "down":
                await s.Call(c => c.DownloadMoveDown(position), (c, t) => c.DownloadMoveDownAsync(position, t), ct);
                break;
            case "top":
                await s.Call(c => c.DownloadMoveTop(position), (c, t) => c.DownloadMoveTopAsync(position, t), ct);
                break;
            case "bottom":
                await s.Call(c => c.DownloadMoveBottom(position), (c, t) => c.DownloadMoveBottomAsync(position, t), ct);
                break;
            default:
                throw new UsageException($"Unknown direction '{args[1]}'.");
        }
        Console.WriteLine($"Moved position {position} {args[1].ToLowerInvariant()}.");
    }

    private static async Task Search(Session s, string[] args, CancellationToken ct)
    {
        var words = new List<string>();
        bool local = false;
        int minAge = 0, maxAge = 1000, limit = 20;
        int[] download = Array.Empty<int>();
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--local":
                    local = true;
                    break;
                case "--min-age":
                    minAge = Args.Int(Args.Next(args, ref i), "min age");
                    break;
                case "--max-age":
                    maxAge = Args.Int(Args.Next(args, ref i), "max age");
                    break;
                case "--limit":
                    limit = Args.Int(Args.Next(args, ref i), "limit");
                    break;
                case "--download":
                    download = Args.Next(args, ref i).Split(',').Select(n => Args.Int(n, "result number")).ToArray();
                    break;
                default:
                    words.Add(args[i]);
                    break;
            }
        }
        if (words.Count == 0)
            throw new UsageException("Give a search text.");
        string text = string.Join(" ", words);

        if (local)
            await s.Call(c => c.SearchLocal(minAge, maxAge, text), (c, t) => c.SearchLocalAsync(minAge, maxAge, text, t), ct);
        else
            await s.Call(c => c.SearchInternet(minAge, maxAge, text), (c, t) => c.SearchInternetAsync(minAge, maxAge, text, t), ct);

        long count = await s.Call(c => c.GetLoadedCount(), (c, t) => c.GetLoadedCountAsync(t), ct);
        s.SearchConnection = s.ConnectionNumber;
        s.SearchResultCount = count;
        Console.WriteLine($"{count} results.");
        if (count > 0 && limit > 0)
        {
            int last = (int)Math.Min(count, limit);
            IReadOnlyList<GroupFileSet> results = await s.Call(c => c.ListLoaded(1, last), (c, t) => c.ListLoadedAsync(1, last, t), ct);
            var rows = new List<string[]> { new[] { "#", "Size", "Posted", "Pars", "Subject" } };
            rows.AddRange(results.Select(r => new[]
            {
                r.Id.ToString(), Output.Bytes(r.FileSize), r.TimeStamp.ToLocalTime().ToString("yyyy-MM-dd"), r.ParCount.ToString(), r.Subject
            }));
            Output.Table(rows, header: true);
            if (count > last)
                Console.WriteLine($"Showing {last} of {count}. Use --limit to see more.");
        }

        if (download.Length > 0)
            await Get(s, download.Select(n => n.ToString()).ToArray(), ct);
    }

    private static async Task Get(Session s, string[] args, CancellationToken ct)
    {
        if (args.Length == 0)
            throw new UsageException("Give one or more result numbers from 'search'.");
        if (s.SearchConnection != s.ConnectionNumber)
            throw new UsageException("Search results belong to the connection that ran the search. Run 'search' again first.");
        foreach (int number in args.Select(a => Args.Int(a, "result number")))
        {
            if (number < 1 || number > s.SearchResultCount)
                throw new UsageException($"Result {number} is out of range (1-{s.SearchResultCount}).");
            await s.Call(c => c.DownloadFromLoaded(number), (c, t) => c.DownloadFromLoadedAsync(number, t), ct);
            Console.WriteLine($"Queued result {number}.");
        }
    }

    private static async Task Files(Session s, string[] args, CancellationToken ct)
    {
        int limit = args.Length > 0 ? Args.Int(args[0], "count") : 20;
        long count = await s.Call(c => c.GetFilesCount(), (c, t) => c.GetFilesCountAsync(t), ct);
        if (count == 0)
        {
            Console.WriteLine("No files.");
            return;
        }
        int last = (int)Math.Min(count, Math.Max(limit, 1));
        IReadOnlyList<FilesFileSet> files = await s.Call(c => c.GetFilesList(1, last), (c, t) => c.GetFilesListAsync(1, last, t), ct);
        var rows = new List<string[]> { new[] { "#", "Size", "Downloaded", "File" } };
        rows.AddRange(files.Select((f, i) => new[]
        {
            (i + 1).ToString(), Output.Bytes(f.FileSize), f.TimeStamp.ToLocalTime().ToString("yyyy-MM-dd HH:mm"), f.FileName
        }));
        Output.Table(rows, header: true);
        if (count > last)
            Console.WriteLine($"Showing {last} of {count}.");
    }

    private static async Task Wish(Session s, string[] args, CancellationToken ct)
    {
        long count = await s.Call(c => c.GetWishCount(), (c, t) => c.GetWishCountAsync(t), ct);
        if (count == 0)
        {
            Console.WriteLine("The wish list is empty.");
            return;
        }
        int n = (int)count;
        IReadOnlyList<WishFileSet> wishes = await s.Call(c => c.GetWishList(1, n), (c, t) => c.GetWishListAsync(1, n, t), ct);
        var rows = new List<string[]> { new[] { "#", "Size", "Files", "Pars", "Subject" } };
        rows.AddRange(wishes.Select((w, i) => new[]
        {
            (i + 1).ToString(), Output.Bytes(w.FileSize), w.FileCount.ToString(), w.ParCount.ToString(), w.Subject
        }));
        Output.Table(rows, header: true);
    }

    private static async Task Upload(Session s, string[] args, CancellationToken ct)
    {
        if (args.Length == 0)
            throw new UsageException("Give one or more NZB files.");
        foreach (string file in args)
        {
            if (!File.Exists(file))
                throw new UsageException($"File not found: {file}");
            await s.Call(c => c.UploadNzb(file), (c, t) => c.UploadNzbAsync(file, t), ct);
            Console.WriteLine($"Uploaded {Path.GetFileName(file)}.");
        }
    }

    private static async Task Gogs(Session s, string[] args, CancellationToken ct)
    {
        if (args.Length > 0)
        {
            string name = string.Join(" ", args);
            await PrintList(s, c => c.ListChildren(name), (c, t) => c.ListChildrenAsync(name, t), ct);
            return;
        }
        GogTree tree = await s.Call(c => c.ListGogsWithChildren(), (c, t) => c.ListGogsWithChildrenAsync(t), ct);
        foreach (string gog in tree.GroupOfGroupNames)
        {
            Console.WriteLine(gog);
            foreach (string group in tree.GetGroupOfGroupChildren(gog))
                Console.WriteLine($"  {group}");
        }
    }

    private static async Task PrintList(Session s, Func<NewsbinClient, IReadOnlyList<string>> sync,
        Func<NewsbinClient, CancellationToken, Task<IReadOnlyList<string>>> async, CancellationToken ct)
    {
        foreach (string item in await s.Call(sync, async, ct))
            Console.WriteLine(item);
    }
}

internal static class Args
{
    public static int Int(string value, string name) =>
        int.TryParse(value, out int result) ? result : throw new UsageException($"Invalid {name} '{value}'.");

    public static bool Bool(string value) => value.ToLowerInvariant() switch
    {
        "on" or "true" or "yes" or "1" => true,
        "off" or "false" or "no" or "0" => false,
        _ => throw new UsageException($"Expected on or off, got '{value}'.")
    };

    public static string Next(string[] args, ref int i) =>
        ++i < args.Length ? args[i] : throw new UsageException($"{args[i - 1]} needs a value.");
}
