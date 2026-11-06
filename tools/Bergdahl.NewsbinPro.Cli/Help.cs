namespace Bergdahl.NewsbinPro.Cli;

internal static class Help
{
    public static void PrintUsage()
    {
        Console.WriteLine("""
            newsbin: command line client for Newsbin Pro

            Usage:
              newsbin [options] <command> [arguments]   run one command and exit
              newsbin [options]                         interactive shell on one connection

            Options:
              -H, --host <host>          Newsbin host (or NEWSBIN_HOST, or 'config host')
              -p, --port <port>          remote control port, default 118 (or NEWSBIN_PORT, or 'config port')
              -P, --password <password>  remote control password (or NEWSBIN_PASSWORD, or 'config password';
                                         prompted if missing)
              -v, --verbose              log every line sent to and received from Newsbin
                  --sync                 use the synchronous API instead of the async one
              -h, --help                 show this help

            """);
        PrintCommands(Array.Empty<string>());
        Console.WriteLine("""

            In the shell, 'verbose [on|off]' and 'sync [on|off]' switch the options, and 'quit' exits.

            Settings are read from {0}.

            Examples:
              newsbin config host 192.168.1.10
              newsbin config password
              newsbin status
              newsbin search "some release" --download 1
              newsbin delete 0
              newsbin call SetFilterAge 48
            """.Replace("{0}", ConfigFile.FilePath));
    }

    public static void PrintCommands(string[] args)
    {
        if (args.Length > 0)
        {
            if (!Commands.TryGet(args[0], out CommandInfo? command))
            {
                Console.Error.WriteLine($"Unknown command '{args[0]}'.");
                return;
            }
            Console.WriteLine($"{command.Name} {command.Arguments}".TrimEnd());
            Console.WriteLine($"  {command.Description}");
            if (command.Aliases is { Length: > 0 })
                Console.WriteLine($"  Aliases: {string.Join(", ", command.Aliases)}");
            return;
        }

        Console.WriteLine("Commands:");
        var rows = Commands.All.Select(c => new[] { "  " + $"{c.Name} {c.Arguments}".TrimEnd(), c.Description }).ToList();
        int width = rows.Max(r => r[0].Length);
        if (width > 40)
            width = 40;
        foreach (string[] row in rows)
        {
            if (row[0].Length > width)
            {
                Console.WriteLine(row[0]);
                Console.WriteLine(new string(' ', width + 2) + row[1]);
            }
            else
            {
                Console.WriteLine(row[0].PadRight(width + 2) + row[1]);
            }
        }
    }
}
