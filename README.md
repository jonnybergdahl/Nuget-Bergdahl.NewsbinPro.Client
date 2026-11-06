# Bergdahl.NewsbinPro.Client

A .NET client for the [Newsbin Pro](https://www.newsbin.com/) remote control interface. Search Usenet, load groups,
queue and manage downloads, browse the wish list, download files and upload NZB files from your own application.

- Targets .NET Framework 4.8, .NET 8 and .NET 10
- Every command comes in a synchronous and an `…Async` version, and the async ones take a `CancellationToken`
- Nullable annotated, no dependencies outside the framework
- Events for every command sent and every response received, useful for logging and debugging

The library started out in 2011 as *Newsbin Remote Control* on [SourceForge](https://sourceforge.net/projects/nbremote/),
written together with nbdexter. Version 4 is a port to modern .NET, published on NuGet.
A Python version is available as [newsbinpro-client](https://pypi.org/project/newsbinpro-client/).

## Installation

```bash
dotnet add package Bergdahl.NewsbinPro.Client
```

## Setting up Newsbin

Enable the remote control interface in Newsbin under **Options → Remote Control** and set a password.
The default port is **118**.

## Usage

```c#
using Bergdahl.NewsbinPro.Client;

using var client = new NewsbinClient("192.168.1.10", NewsbinClient.DefaultPort, "password");
await client.ConnectAsync();
Console.WriteLine($"Connected to {client.NewsbinVersion}");

NewsbinStatus status = await client.GetStatusAsync();
Console.WriteLine($"Speed: {status.Speed / 8} bytes/s, queued: {status.QueuedFileSize} bytes");

long count = await client.GetDownloadsCountAsync();
if (count > 0)
{
    foreach (DownloadFileSet download in await client.GetDownloadsListAsync(1, (int)count))
        Console.WriteLine($"{download.Id}: {download.Subject} {download.DownloadedCurrentSize}/{download.DownloadedSize}");
}

await client.SetPausedStateAsync(true);
await client.UploadNzbAsync(@"C:\Temp\something.nzb");

await client.DisconnectAsync();
```

The synchronous API works the same way without the `Async` suffix:

```c#
using var client = new NewsbinClient("192.168.1.10", 118, "password");
client.Connect();
bool paused = client.GetPausedState();
```

## Command line client

[tools/Bergdahl.NewsbinPro.Cli](tools/Bergdahl.NewsbinPro.Cli) is a small command line client built on the library.
It replaces the WinForms test client from earlier versions and can run every call the library supports.

```bash
alias newsbin='dotnet run --project tools/Bergdahl.NewsbinPro.Cli --'

newsbin config host 192.168.1.10            # saved to ~/.config/newsbin/config.json
newsbin config password                     # prompts, so it stays out of the shell history

newsbin status                              # speed, free space, queue, paused, limiter
newsbin downloads                           # the queue, with the positions delete/move take
newsbin search "some release" --download 1  # search and queue the first result
newsbin upload something.nzb
newsbin call SetFilterAge 48                # any NewsbinClient method; 'newsbin methods' lists them
newsbin                                     # interactive shell on one connection
```

- Connection settings come from `--host/--port/--password`, then `NEWSBIN_HOST/PORT/PASSWORD`, then the settings
  file (`%APPDATA%\newsbin\config.json` on Windows, or the file named by `NEWSBIN_CONFIG`). The file is only readable
  by you, since it can hold the password.
- The `.run` folder has Rider run configurations for the shell, `status` and `downloads`. They read the settings
  file, so no secrets are stored in the repo.
- In the shell, `search` and then `get <number>` work on the same connection, which search results need.
- `-v` logs every protocol line (the password hash is masked), and `--sync` runs everything through the synchronous
  API instead of the async one, so both can be tested.
- Ctrl+C cancels a running command. Exit codes: 0 OK, 1 usage, 2 Newsbin error, 3 connection error, 130 canceled.

### Things to know

- **List indexes start at 1.** `GetDownloadsList(1, 10)` returns the first ten entries; both ends are included, and
  a range starting at 0 or past the end throws (`ErrorRangeExceeded`). `DownloadFromLoaded` uses the same numbering.
- **Download commands count from 0.** `DeleteDownload(0)` deletes the first entry in the download list. The
  `DownloadFileSet.Id` value isn't accepted. Pause, resume, move and assemble are assumed to work the same way.
- **Search results belong to the connection.** After `SearchInternet` or `SearchLocal`, read the results with
  `GetLoadedCount` and `ListLoaded`, and queue them with `DownloadFromLoaded(position)`, on the same connected client.
  A new connection starts with an empty loaded list.
- **Speeds are in bits per second.** `NewsbinStatus.Speed` and `SpeedSamples` are bits/s (Newsbin sends the
  samples as kbit/s; the library converts them). Divide by 8 for bytes per second.
- **`DownloadFromFiles` is untested.** Newsbin Pro 6.91 answered `FILES DOWNLOAD` with 405 for every argument tried
  (position, id, range), so this call may not work against current Newsbin versions.
- **One command at a time.** A client uses a single connection and isn't thread safe. Don't run calls on it in parallel.
- **Errors throw.** If Newsbin answers with an error status (400 and up), the call throws a `CommunicationException`
  with the `Command` and the `Response`. The low level `SendCommand` only throws for unparseable responses, so you
  can check the status code yourself.
- **Cancelling closes the connection.** The protocol can't abort a command halfway, so a cancelled async call closes
  the connection. Call `ConnectAsync` again to reconnect.
- **Synchronization context.** Async calls resume on the captured context by default, so events can update a UI.
  Pass `continueOnCapturedContext: false` to the constructor in services and libraries.

## API overview

| Area | Methods |
|---|---|
| Connection | `Connect`, `Disconnect`, `Connected`, `NewsbinVersion` |
| Status | `GetStatus`, `Get/SetBandwidthLimiterState`, `Get/SetPausedState` |
| Groups and servers | `ListGroups`, `ListGogs`, `ListChildren`, `ListChildrenDetails`, `ListGogsWithChildren`, `ListServers` |
| Filters | `ListFilters`, `SetFilterMinSize/MaxSize/Age`, `SetTextFilter`, `AddGroupFilter`, `HideOld`, `EnableFilters`, `ClearFilters`, `SetFilterProfile`, `MarkOld`, `MarkAllOld` |
| Search | `SearchLocal`, `SearchInternet` |
| Loaded group | `LoadGroup`, `ClearGroup`, `DeleteFromGroup`, `GetLoadedCount`, `ListLoaded`, `SortLoaded`, `ClearLoaded`, `FindLoaded`, `DownloadFromLoaded`, `GetNzbFromLoaded` |
| Downloads | `GetDownloadsCount`, `GetDownloadsList`, `Lock/UnlockDownloads`, `SortDownloadsList`, `ClearDownloadList`, `FindInDownloadList`, `AssembleDownload`, `DeleteDownload`, `Pause/ResumeDownload`, `DownloadMoveUp/Down/Top/Bottom` |
| Files | `GetFilesCount`, `GetFilesList`, `DownloadFromFiles` (to a file or a `Stream`) |
| Wish list | `GetWishCount`, `GetWishList`, `DownloadFromWish` |
| NZB | `UploadNzb` (from a file or a `Stream`) |
| Low level | `SendCommand`, `SendCommandWithoutResponse`, `SendListCommand`, `SendLongCommand`, `GetResponse` |
| Events | `ClientConnected`, `ClientDisconnected`, `ClientSentCommand`, `ClientReceivedResponse`, `ClientReceivedListItem` |

All methods except `Connected` and `NewsbinVersion` also have an `…Async` version.

## Building

```bash
dotnet build
dotnet test
```

Building needs the .NET 10 SDK. The .NET Framework 4.8 target builds on any OS; its tests only run on Windows.

## Changes

### 4.0.0 (2026)

Port to modern .NET, published on NuGet as `Bergdahl.NewsbinPro.Client`.

- SDK-style project targeting `net48`, `net8.0` and `net10.0`, with nullable annotations and XML docs.
- Namespace changed from `Newsbin.Remote` to `Bergdahl.NewsbinPro.Client`.
- All async methods take an optional `CancellationToken`.
- `NewsbinClient` implements `IDisposable` and can reconnect after `Disconnect`.
- Events use `EventHandler<T>`. `ClientRecievedListItem` is renamed to `ClientReceivedListItem`.
- Commands throw `CommunicationException` on error status codes instead of ignoring them.
- List methods return `IReadOnlyList<T>`. `DownloadFromFiles` returns the `FilesDownloadItem`.
- The low level `Send…Command` methods take `params string[]` (sync) or `string[]` (async) arguments.
- Commands always end with CRLF, the same as the original library sent on Windows.
- Fixes:
  - `ListGogsWithChildren` sent the wrong commands and didn't list each group's children.
  - Download progress like `383.3 KB/5.92 GB` failed to parse.
  - Sizes in download progress use decimal units (1 GB = 1,000,000,000 bytes) as Newsbin does, not 1024-based.
  - The incomplete part count (`Inc:` in the status) was always 0.
  - All numbers are parsed with the invariant culture.
  - `UploadNzbAsync(fileName)` closed the file before the upload finished.
  - `UploadNzb` could stop early on partial stream reads, and `DownloadFromFiles` counted bytes wrong on short reads.
  - The async upload and download methods made blocking reads at the end.
  - `CommunicationException.ToString()` threw a `FormatException`.
  - A dropped connection now throws `CommunicationException` instead of `NullReferenceException`.
  - Command arguments containing line breaks are rejected.

### 3.0.0.0 (2014-11-01)

- Added async support.

### 2.0.0.0.b2 (2012-07-17)

- Changed `Response.Status` to an enum.
- Implemented the NZB upload and Files download commands.
- Fixed a UTF-8 BOM sent to Newsbin in the first packet.

### 2.0.0.0.b1 (2012-07-16)

- New client library and test client.

### 1.0.0.1 (2011-09-12)

- Added tool tips and program name and version in the title bar.
- Fixed a bug with Upload Files.

### 1.0.0.0 (2011-09-11)

- First check-in.

## License

MIT, see [LICENSE](LICENSE).

Newsbin® is a trademark of CMCE, Inc. This project is not affiliated with Newsbin.
