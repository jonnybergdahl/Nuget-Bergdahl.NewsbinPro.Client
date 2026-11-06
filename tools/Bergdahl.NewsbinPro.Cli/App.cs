using System.Net.Sockets;
using Bergdahl.NewsbinPro.Client;

namespace Bergdahl.NewsbinPro.Cli;

internal sealed class Options
{
    public string? Host { get; set; }
    public int Port { get; set; } = NewsbinClient.DefaultPort;
    public string? Password { get; set; }
    public bool Verbose { get; set; }
    public bool Sync { get; set; }
    public bool Help { get; set; }
    public List<string> Command { get; } = new();
}

internal static class ExitCodes
{
    public const int Ok = 0;
    public const int Usage = 1;
    public const int NewsbinError = 2;
    public const int ConnectionError = 3;
    public const int Canceled = 130;
}

internal static class App
{
    public static async Task<int> RunAsync(string[] args)
    {
        Options options;
        try
        {
            options = ParseOptions(args);
        }
        catch (UsageException ex)
        {
            Console.Error.WriteLine(ex.Message);
            Console.Error.WriteLine("Run 'newsbin --help' for usage.");
            return ExitCodes.Usage;
        }

        if (options.Help)
        {
            Help.PrintUsage();
            return ExitCodes.Ok;
        }
        bool offline = options.Command.Count > 0
            && Commands.TryGet(options.Command[0], out CommandInfo? first)
            && !first.NeedsConnection;
        if (offline)
        {
            // help and methods don't talk to Newsbin.
            options.Host ??= "localhost";
        }
        else
        {
            if (string.IsNullOrWhiteSpace(options.Host))
            {
                Console.Error.WriteLine("No host given. Use --host, set NEWSBIN_HOST, or run 'newsbin config host <host>'.");
                return ExitCodes.Usage;
            }
            options.Password ??= PromptPassword();
        }

        using var session = new Session(options);
        Console.CancelKeyPress += session.OnCancelKeyPress;

        if (options.Command.Count > 0)
            return await session.ExecuteAsync(options.Command.ToArray(), connectFirst: true);

        return await session.RunShellAsync();
    }

    private static Options ParseOptions(string[] args)
    {
        // Settings file first, then environment variables, then command line options.
        ConfigFile config = ConfigFile.Load();
        var options = new Options
        {
            Host = Environment.GetEnvironmentVariable("NEWSBIN_HOST") is { Length: > 0 } host ? host : config.Host,
            Password = Environment.GetEnvironmentVariable("NEWSBIN_PASSWORD") ?? config.Password,
        };
        if (int.TryParse(Environment.GetEnvironmentVariable("NEWSBIN_PORT"), out int envPort))
            options.Port = envPort;
        else if (config.Port is int configPort)
            options.Port = configPort;
        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            if (options.Command.Count > 0)
            {
                // Everything after the command name belongs to the command.
                options.Command.Add(arg);
                continue;
            }
            switch (arg)
            {
                case "-H":
                case "--host":
                    options.Host = Value(args, ref i, arg);
                    break;
                case "-p":
                case "--port":
                    string port = Value(args, ref i, arg);
                    options.Port = int.TryParse(port, out int value) && value > 0 && value < 65536
                        ? value
                        : throw new UsageException($"Invalid port '{port}'.");
                    break;
                case "-P":
                case "--password":
                    options.Password = Value(args, ref i, arg);
                    break;
                case "-v":
                case "--verbose":
                    options.Verbose = true;
                    break;
                case "--sync":
                    options.Sync = true;
                    break;
                case "-h":
                case "--help":
                case "-?":
                    options.Help = true;
                    break;
                default:
                    if (arg.StartsWith('-'))
                        throw new UsageException($"Unknown option '{arg}'.");
                    options.Command.Add(arg);
                    break;
            }
        }
        return options;
    }

    private static string Value(string[] args, ref int i, string name) =>
        ++i < args.Length ? args[i] : throw new UsageException($"{name} needs a value.");

    public static string PromptPassword()
    {
        if (Console.IsInputRedirected)
            return Console.ReadLine() ?? "";

        Console.Error.Write("Password: ");
        var password = new System.Text.StringBuilder();
        while (true)
        {
            ConsoleKeyInfo key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter)
                break;
            if (key.Key == ConsoleKey.Backspace)
            {
                if (password.Length > 0)
                    password.Length--;
                continue;
            }
            if (!char.IsControl(key.KeyChar))
                password.Append(key.KeyChar);
        }
        Console.Error.WriteLine();
        return password.ToString();
    }
}

internal sealed class UsageException : Exception
{
    public UsageException(string message) : base(message)
    {
    }
}

/// <summary>One client connection plus the state the commands share.</summary>
internal sealed class Session : IDisposable
{
    private readonly object gate = new();
    private CancellationTokenSource? running;
    private int connectionNumber;

    public Options Options { get; }
    public NewsbinClient Client { get; }

    /// <summary>The connection the last search ran on; search results only exist on that connection.</summary>
    public int SearchConnection { get; set; } = -1;
    public long SearchResultCount { get; set; }
    public int ConnectionNumber => connectionNumber;

    public Session(Options options)
    {
        Options = options;
        Client = new NewsbinClient(options.Host!, options.Port, options.Password ?? "", continueOnCapturedContext: false);
        Client.ClientSentCommand += (_, e) => Log($"-> ({e.ThreadId}) {Mask(e.Command)}");
        Client.ClientReceivedResponse += (_, e) => Log($"<- ({e.ThreadId}) {(int)e.Response.StatusCode} {e.Response.Message}");
        Client.ClientReceivedListItem += (_, e) => Log($" . ({e.ThreadId}) {e.ListItem.Replace('\t', '|')}");
        Client.ClientConnected += (_, _) => Log("connected");
        Client.ClientDisconnected += (_, _) => Log("disconnected");
    }

    public void Dispose() => Client.Dispose();

    private static string Mask(string command) =>
        command.StartsWith("AUTH ", StringComparison.Ordinal) ? "AUTH ********" : command;

    private void Log(string line)
    {
        if (!Options.Verbose)
            return;
        ConsoleColor color = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Error.WriteLine(line);
        Console.ForegroundColor = color;
    }

    public void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        lock (gate)
        {
            if (running == null)
                return; // nothing running: let Ctrl+C end the program
            e.Cancel = true;
            running.Cancel();
        }
    }

    public async Task EnsureConnectedAsync(CancellationToken cancellationToken)
    {
        if (Client.Connected)
            return;
        await Call(c => c.Connect(), (c, ct) => c.ConnectAsync(ct), cancellationToken);
        connectionNumber++;
        if (Options.Verbose || connectionNumber > 1)
            Console.Error.WriteLine($"Connected to {Client.NewsbinVersion ?? "Newsbin"} at {Client.ServerAddress}:{Client.Port}");
    }

    /// <summary>Runs a call through the sync or async API depending on --sync.</summary>
    public Task<T> Call<T>(Func<NewsbinClient, T> sync, Func<NewsbinClient, CancellationToken, Task<T>> async, CancellationToken cancellationToken) =>
        Options.Sync ? Task.Run(() => sync(Client), cancellationToken) : async(Client, cancellationToken);

    /// <inheritdoc cref="Call{T}"/>
    public Task Call(Action<NewsbinClient> sync, Func<NewsbinClient, CancellationToken, Task> async, CancellationToken cancellationToken) =>
        Options.Sync ? Task.Run(() => sync(Client), cancellationToken) : async(Client, cancellationToken);

    public async Task<int> ExecuteAsync(string[] words, bool connectFirst)
    {
        if (words.Length == 0)
            return ExitCodes.Ok;

        if (!Commands.TryGet(words[0], out CommandInfo? command))
        {
            Console.Error.WriteLine($"Unknown command '{words[0]}'. Run 'help' for the list.");
            return ExitCodes.Usage;
        }

        using var cts = new CancellationTokenSource();
        lock (gate)
            running = cts;
        try
        {
            if (command.NeedsConnection && (connectFirst || !Client.Connected))
                await EnsureConnectedAsync(cts.Token);
            await command.Run(this, words.Skip(1).ToArray(), cts.Token);
            return ExitCodes.Ok;
        }
        catch (UsageException ex)
        {
            Console.Error.WriteLine(ex.Message);
            Console.Error.WriteLine($"Usage: {command.Name} {command.Arguments}");
            return ExitCodes.Usage;
        }
        catch (CommunicationException ex)
        {
            Console.Error.WriteLine(ex.Response != null
                ? $"Newsbin: {ex.Message} ({(int)ex.Response.StatusCode} {ex.Response.StatusCode})"
                : $"Newsbin: {ex.Message}");
            return ExitCodes.NewsbinError;
        }
        catch (OperationCanceledException)
        {
            Console.Error.WriteLine("Canceled. The connection was closed; the next command reconnects.");
            return ExitCodes.Canceled;
        }
        catch (Exception ex) when (ex is SocketException || ex is IOException)
        {
            Console.Error.WriteLine($"Connection to {Options.Host}:{Options.Port} failed: {ex.Message}");
            return ExitCodes.ConnectionError;
        }
        finally
        {
            lock (gate)
                running = null;
        }
    }

    public async Task<int> RunShellAsync()
    {
        int result = await ExecuteAsync(new[] { "status" }, connectFirst: true);
        if (result == ExitCodes.ConnectionError || (result == ExitCodes.NewsbinError && !Client.Connected))
            return result;

        Console.WriteLine("Type 'help' for commands, 'quit' to exit. Ctrl+C cancels a running command.");
        while (true)
        {
            Console.Write(Options.Sync ? "newsbin (sync)> " : "newsbin> ");
            string? line = Console.ReadLine();
            if (line == null)
                break;
            string[] words;
            try
            {
                words = Tokenizer.Split(line);
            }
            catch (UsageException ex)
            {
                Console.Error.WriteLine(ex.Message);
                continue;
            }
            if (words.Length == 0)
                continue;

            switch (words[0].ToLowerInvariant())
            {
                case "quit":
                case "exit":
                    await Call(c => c.Disconnect(), (c, ct) => c.DisconnectAsync(ct), CancellationToken.None);
                    return ExitCodes.Ok;
                case "verbose":
                    Options.Verbose = Toggle(words, Options.Verbose);
                    Console.WriteLine($"Protocol log {(Options.Verbose ? "on" : "off")}.");
                    continue;
                case "sync":
                    Options.Sync = Toggle(words, Options.Sync);
                    Console.WriteLine($"Using the {(Options.Sync ? "sync" : "async")} API.");
                    continue;
            }
            await ExecuteAsync(words, connectFirst: false);
        }

        await Call(c => c.Disconnect(), (c, ct) => c.DisconnectAsync(ct), CancellationToken.None);
        return ExitCodes.Ok;
    }

    private static bool Toggle(string[] words, bool current) =>
        words.Length < 2 ? !current : words[1].ToLowerInvariant() is "on" or "1" or "true" or "yes";
}

internal static class Tokenizer
{
    /// <summary>Splits a line on whitespace, keeping "quoted text" together.</summary>
    public static string[] Split(string line)
    {
        var words = new List<string>();
        var current = new System.Text.StringBuilder();
        bool inQuotes = false;
        bool hasWord = false;
        foreach (char c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
                hasWord = true;
            }
            else if (char.IsWhiteSpace(c) && !inQuotes)
            {
                if (hasWord)
                    words.Add(current.ToString());
                current.Clear();
                hasWord = false;
            }
            else
            {
                current.Append(c);
                hasWord = true;
            }
        }
        if (inQuotes)
            throw new UsageException("Missing closing quote.");
        if (hasWord)
            words.Add(current.ToString());
        return words.ToArray();
    }
}
