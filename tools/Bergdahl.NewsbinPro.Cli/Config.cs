using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bergdahl.NewsbinPro.Cli;

/// <summary>
/// Connection settings stored in <c>~/.config/newsbin/config.json</c> (<c>%APPDATA%\newsbin\config.json</c> on Windows),
/// or the file named by <c>NEWSBIN_CONFIG</c>.
/// </summary>
internal sealed class ConfigFile
{
    private static readonly JsonSerializerOptions Json = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public string? Host { get; set; }
    public int? Port { get; set; }
    public string? Password { get; set; }

    public static string FilePath
    {
        get
        {
            if (Environment.GetEnvironmentVariable("NEWSBIN_CONFIG") is { Length: > 0 } path)
                return path;
            if (OperatingSystem.IsWindows())
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "newsbin", "config.json");
            // ~/.config on macOS and Linux, the usual place for command line tools.
            string configHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME") is { Length: > 0 } xdg
                ? xdg
                : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config");
            return Path.Combine(configHome, "newsbin", "config.json");
        }
    }

    public static ConfigFile Load()
    {
        string path = FilePath;
        if (!File.Exists(path))
            return new ConfigFile();
        try
        {
            return JsonSerializer.Deserialize<ConfigFile>(File.ReadAllText(path), Json) ?? new ConfigFile();
        }
        catch (JsonException ex)
        {
            throw new UsageException($"Can't read {path}: {ex.Message}");
        }
    }

    public void Save()
    {
        string path = FilePath;
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(this, Json) + Environment.NewLine);
        // The file can hold the password, so keep it private.
        if (!OperatingSystem.IsWindows())
            File.SetUnixFileMode(path, UnixFileMode.UserRead | UnixFileMode.UserWrite);
    }

    /// <summary>The <c>config</c> command: show or change the settings file.</summary>
    public static Task Command(Session session, string[] args, CancellationToken ct)
    {
        ConfigFile config = Load();
        if (args.Length == 0)
        {
            Console.WriteLine($"Settings file: {FilePath}{(File.Exists(FilePath) ? "" : " (doesn't exist yet)")}");
            Output.Table(new[]
            {
                new[] { "host", config.Host ?? "(not set)" },
                new[] { "port", config.Port?.ToString() ?? $"(not set, default {Client.NewsbinClient.DefaultPort})" },
                new[] { "password", config.Password == null ? "(not set)" : "(set)" },
            });
            Console.WriteLine("Command line options and NEWSBIN_HOST/PORT/PASSWORD override these.");
            return Task.CompletedTask;
        }

        string key = args[0].ToLowerInvariant();
        if (key == "unset")
        {
            if (args.Length != 2)
                throw new UsageException("Give the setting to remove: host, port or password.");
            switch (args[1].ToLowerInvariant())
            {
                case "host": config.Host = null; break;
                case "port": config.Port = null; break;
                case "password": config.Password = null; break;
                default: throw new UsageException($"Unknown setting '{args[1]}'.");
            }
            config.Save();
            Console.WriteLine($"Removed {args[1].ToLowerInvariant()}.");
            return Task.CompletedTask;
        }

        switch (key)
        {
            case "host":
                config.Host = args.Length == 2 ? args[1] : throw new UsageException("Give a host name or IP address.");
                break;
            case "port":
                config.Port = args.Length == 2 && int.TryParse(args[1], out int port) && port > 0 && port < 65536
                    ? port
                    : throw new UsageException("Give a port between 1 and 65535.");
                break;
            case "password":
                // Prompting keeps the password out of the shell history.
                config.Password = args.Length == 2 ? args[1] : App.PromptPassword();
                break;
            default:
                throw new UsageException($"Unknown setting '{args[0]}'. Use host, port, password or unset.");
        }
        config.Save();
        Console.WriteLine($"Saved {key} to {FilePath}.");
        return Task.CompletedTask;
    }
}
