using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bergdahl.NewsbinPro.Client.Tests;

/// <summary>
/// A scripted Newsbin stand-in on the loopback interface. It accepts one connection, sends a greeting,
/// and hands every received line to a handler that writes the reply.
/// </summary>
internal sealed class FakeNewsbinServer : IDisposable
{
    public const string Password = "secret";
    public const string Salt = "0123456789ABCDEF";
    // Format captured from Newsbin 6.91RC4.
    public const string Greeting = "480 Newsbin Server 6.91RC4 - " + Salt + " Need to authenticate";

    private readonly TcpListener listener;
    private readonly Func<string, Session, Task> handler;
    private readonly string greeting;
    private readonly CancellationTokenSource stop = new CancellationTokenSource();

    public int Port { get; }
    public ConcurrentQueue<string> ReceivedLines { get; } = new ConcurrentQueue<string>();
    public Task Completion { get; }

    public FakeNewsbinServer(Func<string, Session, Task> handler, string greeting = Greeting)
    {
        this.handler = handler;
        this.greeting = greeting;
        listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        Port = ((IPEndPoint)listener.LocalEndpoint).Port;
        Completion = Task.Run(RunAsync);
    }

    public NewsbinClient CreateClient() => new NewsbinClient("127.0.0.1", Port, Password, continueOnCapturedContext: false);

    /// <summary>A handler that authenticates and then answers QUIT; other lines go to <paramref name="next"/>.</summary>
    public static Func<string, Session, Task> WithAuth(Func<string, Session, Task> next) => async (line, session) =>
    {
        if (line.StartsWith("AUTH ", StringComparison.Ordinal))
        {
            string expected = "AUTH " + AuthenticationHelper.GetAuthString(Password, Salt);
            await session.SendLineAsync(line == expected ? "200 Newsbin Server 6.91RC4 " : "480 Bad password");
        }
        else if (line == "QUIT")
        {
            await session.SendLineAsync("200 Bye");
            session.Close();
        }
        else
        {
            await next(line, session);
        }
    };

    private async Task RunAsync()
    {
        using TcpClient client = await listener.AcceptTcpClientAsync();
        using NetworkStream stream = client.GetStream();
        var session = new Session(stream);
        await session.SendLineAsync(greeting);
        try
        {
            while (!stop.IsCancellationRequested && !session.Closed)
            {
                string? line = await session.ReadLineAsync();
                if (line == null)
                    return;
                ReceivedLines.Enqueue(line);
                await handler(line, session);
            }
        }
        catch (IOException)
        {
            // The client closed the connection.
        }
    }

    public void Dispose()
    {
        stop.Cancel();
        listener.Stop();
    }

    internal sealed class Session
    {
        private readonly NetworkStream stream;

        public bool Closed { get; private set; }

        /// <summary>False if any received line wasn't terminated by CRLF.</summary>
        public bool AllLinesUsedCrLf { get; private set; } = true;

        public Session(NetworkStream stream)
        {
            this.stream = stream;
        }

        public Task SendLineAsync(string line) => SendBytesAsync(Encoding.UTF8.GetBytes(line + "\r\n"));

        public async Task SendLinesAsync(params string[] lines)
        {
            foreach (string line in lines)
                await SendLineAsync(line);
        }

        public async Task SendBytesAsync(byte[] data)
        {
            await stream.WriteAsync(data, 0, data.Length);
            await stream.FlushAsync();
        }

        public async Task<byte[]> ReadBytesAsync(int count)
        {
            byte[] buffer = new byte[count];
            int offset = 0;
            while (offset < count)
            {
                int read = await stream.ReadAsync(buffer, offset, count - offset);
                if (read == 0)
                    throw new EndOfStreamException();
                offset += read;
            }
            return buffer;
        }

        // Reads byte by byte so binary data after a line is left in the stream.
        public async Task<string?> ReadLineAsync()
        {
            var bytes = new MemoryStream();
            byte[] one = new byte[1];
            while (true)
            {
                int read = await stream.ReadAsync(one, 0, 1);
                if (read == 0)
                    return null;
                if (one[0] == '\n')
                    break;
                bytes.WriteByte(one[0]);
            }
            byte[] data = bytes.ToArray();
            if (data.Length == 0 || data[data.Length - 1] != '\r')
                AllLinesUsedCrLf = false;
            else
                Array.Resize(ref data, data.Length - 1);
            return Encoding.UTF8.GetString(data);
        }

        public void Close()
        {
            Closed = true;
            stream.Close();
        }
    }
}
