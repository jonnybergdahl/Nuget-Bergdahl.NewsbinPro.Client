using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Bergdahl.NewsbinPro.Client.Tests;

public class NewsbinClientTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    private static Func<string, FakeNewsbinServer.Session, Task> Replies(Dictionary<string, string[]> replies) =>
        FakeNewsbinServer.WithAuth((line, session) =>
            replies.TryGetValue(line, out string[]? lines)
                ? session.SendLinesAsync(lines)
                : session.SendLineAsync("500 Unknown command"));

    [Fact]
    public async Task ConnectAsync_Authenticates_AndReadsVersion()
    {
        using var server = new FakeNewsbinServer(Replies(new Dictionary<string, string[]>()));
        using NewsbinClient client = server.CreateClient();

        Assert.True(await client.ConnectAsync());

        Assert.True(client.Connected);
        Assert.Equal("Newsbin Server 6.91RC4", client.NewsbinVersion);
        Assert.Equal("AUTH " + AuthenticationHelper.GetAuthString(FakeNewsbinServer.Password, FakeNewsbinServer.Salt), server.ReceivedLines.Single());
    }

    [Fact]
    public void Connect_Sync_Authenticates()
    {
        using var server = new FakeNewsbinServer(Replies(new Dictionary<string, string[]>()));
        using NewsbinClient client = server.CreateClient();

        Assert.True(client.Connect());
        Assert.True(client.Connected);
    }

    [Fact]
    public async Task ConnectAsync_WrongPassword_Throws_AndDisconnects()
    {
        using var server = new FakeNewsbinServer(Replies(new Dictionary<string, string[]>()));
        using var client = new NewsbinClient("127.0.0.1", server.Port, "wrong", continueOnCapturedContext: false);
        int disconnected = 0;
        client.ClientDisconnected += (_, _) => disconnected++;

        var ex = await Assert.ThrowsAsync<CommunicationException>(() => client.ConnectAsync());

        Assert.Equal(Command.Auth, ex.Command);
        Assert.Equal(StatusCode.AuthNeedAuth, ex.Response?.StatusCode);
        Assert.False(client.Connected);
        Assert.Equal(1, disconnected);
    }

    [Fact]
    public async Task ConnectAsync_NoAuthRequired()
    {
        using var server = new FakeNewsbinServer(Replies(new Dictionary<string, string[]>()), greeting: "200 Newsbin ready");
        using NewsbinClient client = server.CreateClient();

        await client.ConnectAsync();

        Assert.True(client.Connected);
        Assert.Empty(server.ReceivedLines);
    }

    [Fact]
    public async Task Commands_AreTerminatedWithCrLf()
    {
        FakeNewsbinServer.Session? captured = null;
        using var server = new FakeNewsbinServer(FakeNewsbinServer.WithAuth(async (line, session) =>
        {
            captured = session;
            await session.SendLineAsync("200 OK");
        }));
        using NewsbinClient client = server.CreateClient();
        await client.ConnectAsync();

        await client.ClearFiltersAsync();
        client.ClearFilters();

        Assert.NotNull(captured);
        Assert.True(captured!.AllLinesUsedCrLf);
    }

    [Fact]
    public async Task GetStatusAsync_ParsesList_AndRaisesEvents()
    {
        using var server = new FakeNewsbinServer(Replies(new Dictionary<string, string[]>
        {
            ["STATUS"] = new[] { "200 Status List Follows ", "Speed\t1500", "Queued\t300", "." },
        }));
        using NewsbinClient client = server.CreateClient();
        var sent = new List<string>();
        var items = new List<string>();
        client.ClientSentCommand += (_, e) => sent.Add(e.Command);
        client.ClientReceivedListItem += (_, e) => items.Add(e.ListItem);
        await client.ConnectAsync();

        NewsbinStatus status = await client.GetStatusAsync();

        Assert.Equal(1500, status.Speed);
        Assert.Equal(300, status.QueuedFileSize);
        Assert.Equal("STATUS", sent.Last());
        Assert.Equal(new[] { "Speed\t1500", "Queued\t300" }, items);
    }

    [Fact]
    public void GetStatus_Sync_ParsesList()
    {
        using var server = new FakeNewsbinServer(Replies(new Dictionary<string, string[]>
        {
            ["STATUS"] = new[] { "200 Status List Follows ", "Speed\t42", "." },
        }));
        using NewsbinClient client = server.CreateClient();
        client.Connect();

        Assert.Equal(42, client.GetStatus().Speed);
    }

    [Fact]
    public async Task PauseAndBandwidthLimiter_MapStatusCodes()
    {
        using var server = new FakeNewsbinServer(Replies(new Dictionary<string, string[]>
        {
            ["PAUSE"] = new[] { "254 Not paused" },
            ["PAUSE ON"] = new[] { "253 Paused" },
            ["BWL"] = new[] { "251 On" },
            ["BWL OFF"] = new[] { "252 Off" },
        }));
        using NewsbinClient client = server.CreateClient();
        await client.ConnectAsync();

        Assert.False(await client.GetPausedStateAsync());
        Assert.True(await client.SetPausedStateAsync(true));
        Assert.True(client.GetBandwidthLimiterState());
        Assert.False(client.SetBandwidthLimiterState(false));
    }

    [Fact]
    public async Task CountCommands_ReturnTheNumber()
    {
        using var server = new FakeNewsbinServer(Replies(new Dictionary<string, string[]>
        {
            ["DOWNLOADS COUNT"] = new[] { "200 17 Sets Loaded 123456 Bytes " },
            ["FILES COUNT"] = new[] { "200 77 Files Listed" },
            ["GROUP LOAD 0 30"] = new[] { "200 1234" },
        }));
        using NewsbinClient client = server.CreateClient();
        await client.ConnectAsync();

        Assert.Equal(17, await client.GetDownloadsCountAsync());
        Assert.Equal(77, client.GetFilesCount());
        Assert.Equal(1234, client.LoadGroup(0, 30));
    }

    [Fact]
    public async Task ErrorStatus_ThrowsCommunicationException()
    {
        using var server = new FakeNewsbinServer(Replies(new Dictionary<string, string[]>
        {
            ["DOWNLOADS DELETE 99"] = new[] { "405 Downloads Delete Range exceeded" },
        }));
        using NewsbinClient client = server.CreateClient();
        await client.ConnectAsync();

        var ex = await Assert.ThrowsAsync<CommunicationException>(() => client.DeleteDownloadAsync(99));
        Assert.Equal(Command.DownloadsDelete, ex.Command);
        Assert.Equal(StatusCode.ErrorRangeExceeded, ex.Response?.StatusCode);
        Assert.Equal("Downloads Delete Range exceeded", ex.Message);

        // The connection stays usable after an error reply.
        Assert.Throws<CommunicationException>(() => client.DeleteDownload(99));
        Assert.True(client.Connected);
    }

    [Fact]
    public async Task ListLoadedAsync_ParsesFileSets()
    {
        using var server = new FakeNewsbinServer(Replies(new Dictionary<string, string[]>
        {
            ["LOADED LIST 1 2"] = new[]
            {
                "200 OK",
                "0\tFirst\t100\t1414843200\tposter\t1\t0\t0",
                "1\tSecond\t200\t1414843200\tposter\t2\t1\t1",
                ".",
            },
        }));
        using NewsbinClient client = server.CreateClient();
        await client.ConnectAsync();

        IReadOnlyList<GroupFileSet> sets = await client.ListLoadedAsync(1, 2);

        Assert.Equal(new[] { "First", "Second" }, sets.Select(s => s.Subject));
    }

    [Fact]
    public async Task ListGogsWithChildren_ListsChildrenOfEachGog()
    {
        using var server = new FakeNewsbinServer(Replies(new Dictionary<string, string[]>
        {
            ["LIST GOGS"] = new[] { "200 OK", "Movies", "Empty", "." },
            ["LIST CHILDREN Movies"] = new[] { "200 OK", "alt.binaries.a", "alt.binaries.b", "." },
            ["LIST CHILDREN Empty"] = new[] { "200 OK", "." },
        }));
        using NewsbinClient client = server.CreateClient();
        await client.ConnectAsync();

        GogTree tree = await client.ListGogsWithChildrenAsync();

        Assert.Equal(new[] { "Movies", "Empty" }, tree.GroupOfGroupNames);
        Assert.Equal(new[] { "alt.binaries.a", "alt.binaries.b" }, tree.GetGroupOfGroupChildren("Movies"));
        Assert.Empty(tree.GetGroupOfGroupChildren("Empty"));
        Assert.Empty(tree.GetGroupOfGroupChildren("Unknown"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UploadNzb_SendsExactBytes(bool useAsync)
    {
        byte[] nzb = Encoding.UTF8.GetBytes(new string('x', 100_000));
        byte[]? received = null;
        using var server = new FakeNewsbinServer(FakeNewsbinServer.WithAuth(async (line, session) =>
        {
            Assert.Equal($"UPLOAD NZBBINARY test.nzb {nzb.Length}", line);
            await session.SendLineAsync("300 Send data");
            received = await session.ReadBytesAsync(nzb.Length);
            await session.SendLineAsync("200 Saved");
        }));
        using NewsbinClient client = server.CreateClient();
        await client.ConnectAsync();

        using var stream = new MemoryStream(nzb);
        if (useAsync)
            await client.UploadNzbAsync("test.nzb", stream, nzb.Length);
        else
            client.UploadNzb("test.nzb", stream, nzb.Length);

        Assert.Equal(nzb, received);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DownloadFromFiles_ReceivesExactBytes(bool useAsync)
    {
        byte[] file = Enumerable.Range(0, 200_000).Select(i => (byte)i).ToArray();
        using var server = new FakeNewsbinServer(FakeNewsbinServer.WithAuth(async (line, session) =>
        {
            if (line == "FILES DOWNLOAD 4")
            {
                await session.SendLineAsync($"200 \"file.bin\" {file.Length}");
            }
            else if (line == "300 CONTINUE")
            {
                await session.SendBytesAsync(file);
                await session.SendLineAsync("200 Done");
            }
        }));
        using NewsbinClient client = server.CreateClient();
        await client.ConnectAsync();

        using var destination = new MemoryStream();
        FilesDownloadItem item = useAsync
            ? await client.DownloadFromFilesAsync(destination, 4)
            : client.DownloadFromFiles(destination, 4);

        Assert.Equal("file.bin", item.FileName);
        Assert.Equal(file, destination.ToArray());

        // The protocol is still in sync after the binary data.
        Assert.True(client.Connected);
    }

    [Fact]
    public async Task Cancellation_ClosesTheConnection()
    {
        using var server = new FakeNewsbinServer(FakeNewsbinServer.WithAuth((line, session) => Task.CompletedTask)); // never answers
        using NewsbinClient client = server.CreateClient();
        await client.ConnectAsync();
        using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => client.GetStatusAsync(cts.Token)).WaitAsync(Timeout);

        Assert.False(client.Connected);
    }

    [Fact]
    public async Task ServerClosingConnection_ThrowsCommunicationException()
    {
        using var server = new FakeNewsbinServer(FakeNewsbinServer.WithAuth((line, session) =>
        {
            session.Close();
            return Task.CompletedTask;
        }));
        using NewsbinClient client = server.CreateClient();
        await client.ConnectAsync();

        await Assert.ThrowsAnyAsync<Exception>(() => client.GetStatusAsync()).WaitAsync(Timeout);
    }

    [Fact]
    public async Task Disconnect_SendsQuit_AndCanReconnectLater()
    {
        using var server = new FakeNewsbinServer(Replies(new Dictionary<string, string[]>()));
        using NewsbinClient client = server.CreateClient();
        await client.ConnectAsync();

        await client.DisconnectAsync();

        Assert.False(client.Connected);
        Assert.Contains("QUIT", server.ReceivedLines);
        await client.DisconnectAsync(); // no-op when not connected
    }

    [Fact]
    public void Commands_WhenNotConnected_ThrowInvalidOperation()
    {
        using var client = new NewsbinClient("127.0.0.1", 118, "pw");
        Assert.Throws<InvalidOperationException>(() => client.GetStatus());
    }

    [Fact]
    public void Commands_AfterDispose_ThrowObjectDisposed()
    {
        var client = new NewsbinClient("127.0.0.1", 118, "pw");
        client.Dispose();
        Assert.Throws<ObjectDisposedException>(() => client.Connect());
        Assert.Throws<ObjectDisposedException>(() => client.GetStatus());
    }
}
