using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bergdahl.NewsbinPro.Client;

/// <summary>
/// Client for the Newsbin Pro remote control interface.
/// </summary>
/// <remarks>
/// <para>
/// Every command has a synchronous and an <c>…Async</c> version. A client runs one command at a time
/// over a single connection, so don't call it from several threads at once.
/// </para>
/// <para>
/// Cancelling an async call closes the connection, because the protocol has no way to abort a command
/// halfway. Call <see cref="ConnectAsync"/> again to reconnect.
/// </para>
/// </remarks>
public partial class NewsbinClient : IDisposable
{
    /// <summary>The default Newsbin remote control port.</summary>
    public const int DefaultPort = 118;

    private const int BufferSize = 64 * 1024;
    private const string LineTerminator = "\r\n";
    private static readonly string[] NoArguments = new string[0];

    private readonly string password;
    private readonly bool continueOnCapturedContext;

    private TcpClient? tcpClient;
    private NetworkStream? networkStream;
    private StreamReader? reader;
    private StreamWriter? writer;
    private volatile bool aborted;
    private bool disposed;

    /// <summary>Raised when the TCP connection has been opened, before authentication.</summary>
    public event EventHandler<ClientConnectedEventArgs>? ClientConnected;

    /// <summary>Raised when the connection has been closed.</summary>
    public event EventHandler<ClientDisconnectedEventArgs>? ClientDisconnected;

    /// <summary>Raised after each command line has been sent.</summary>
    public event EventHandler<ClientSentCommandEventArgs>? ClientSentCommand;

    /// <summary>Raised for each response line.</summary>
    public event EventHandler<ClientReceivedResponseEventArgs>? ClientReceivedResponse;

    /// <summary>Raised for each data line of a list response.</summary>
    public event EventHandler<ClientReceivedListItemEventArgs>? ClientReceivedListItem;

    /// <summary>Host name or IP address of the machine running Newsbin.</summary>
    public string ServerAddress { get; }

    /// <summary>The remote control port.</summary>
    public int Port { get; }

    /// <summary>True while the connection is open.</summary>
    public bool Connected => reader != null && (tcpClient?.Connected ?? false);

    /// <summary>The Newsbin version reported when connecting, if Newsbin required authentication.</summary>
    public string? NewsbinVersion { get; private set; }

    /// <summary>Creates a client. Call <see cref="Connect"/> or <see cref="ConnectAsync"/> to open the connection.</summary>
    /// <param name="serverAddress">Host name or IP address of the machine running Newsbin.</param>
    /// <param name="port">The remote control port, <see cref="DefaultPort"/> unless changed in Newsbin.</param>
    /// <param name="password">The password set under Options → Remote Control in Newsbin.</param>
    /// <param name="continueOnCapturedContext">
    /// Whether async calls resume on the captured synchronization context. Leave it <c>true</c> if you update a UI
    /// from the events; set it to <c>false</c> in services and libraries.
    /// </param>
    public NewsbinClient(string serverAddress, int port, string password, bool continueOnCapturedContext = true)
    {
        if (string.IsNullOrWhiteSpace(serverAddress))
            throw new ArgumentException("A server address is required.", nameof(serverAddress));
        if (port <= 0 || port > 65535)
            throw new ArgumentOutOfRangeException(nameof(port), port, "The port must be between 1 and 65535.");

        ServerAddress = serverAddress;
        Port = port;
        this.password = password ?? throw new ArgumentNullException(nameof(password));
        this.continueOnCapturedContext = continueOnCapturedContext;
    }

    #region Connection

    /// <summary>Opens the connection and authenticates.</summary>
    /// <returns>Always <c>true</c>; failures throw.</returns>
    /// <exception cref="CommunicationException">Authentication failed.</exception>
    /// <exception cref="SocketException">The connection couldn't be opened.</exception>
    public bool Connect()
    {
        PrepareConnect();
        var client = new TcpClient();
        try
        {
            client.Connect(ServerAddress, Port);
        }
        catch
        {
            client.Dispose();
            throw;
        }

        AttachConnection(client);
        try
        {
            Response response = ReadResponse();
            if (response.StatusCode == StatusCode.AuthNeedAuth)
                response = SendCommand(Command.Auth, GetAuthString(response));
            EnsureAuthenticated(response);
        }
        catch
        {
            DisposeConnection();
            throw;
        }
        return true;
    }

    /// <summary>Opens the connection and authenticates.</summary>
    /// <param name="cancellationToken">Cancels the connect.</param>
    /// <returns>Always <c>true</c>; failures throw.</returns>
    /// <exception cref="CommunicationException">Authentication failed.</exception>
    /// <exception cref="SocketException">The connection couldn't be opened.</exception>
    public async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
    {
        PrepareConnect();
        cancellationToken.ThrowIfCancellationRequested();
        var client = new TcpClient();
        try
        {
#if NET5_0_OR_GREATER
            await client.ConnectAsync(ServerAddress, Port, cancellationToken).ConfigureAwait(continueOnCapturedContext);
#else
            using (cancellationToken.Register(() => client.Dispose()))
            {
                try
                {
                    await client.ConnectAsync(ServerAddress, Port).ConfigureAwait(continueOnCapturedContext);
                }
                catch (Exception ex) when (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException("The connect was canceled.", ex, cancellationToken);
                }
            }
#endif
        }
        catch
        {
            client.Dispose();
            throw;
        }

        AttachConnection(client);
        try
        {
            Response response = await ReadResponseAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext);
            if (response.StatusCode == StatusCode.AuthNeedAuth)
            {
                response = await SendCommandAsync(Command.Auth, new[] { GetAuthString(response) }, cancellationToken)
                    .ConfigureAwait(continueOnCapturedContext);
            }
            EnsureAuthenticated(response);
        }
        catch
        {
            DisposeConnection();
            throw;
        }
        return true;
    }

    /// <summary>Sends <c>QUIT</c> and closes the connection. Does nothing if not connected.</summary>
    public void Disconnect()
    {
        if (reader == null)
            return;
        try
        {
            SendCommand(Command.Quit);
        }
        catch (Exception ex) when (ex is IOException || ex is CommunicationException || ex is ObjectDisposedException)
        {
            // Newsbin may close the socket before answering QUIT.
        }
        finally
        {
            DisposeConnection();
        }
    }

    /// <summary>Sends <c>QUIT</c> and closes the connection. Does nothing if not connected.</summary>
    /// <param name="cancellationToken">Cancels waiting for the reply; the connection is closed either way.</param>
    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        if (reader == null)
            return;
        try
        {
            await SendCommandAsync(Command.Quit, null, cancellationToken).ConfigureAwait(continueOnCapturedContext);
        }
        catch (Exception ex) when (ex is IOException || ex is CommunicationException || ex is ObjectDisposedException)
        {
            // Newsbin may close the socket before answering QUIT.
        }
        finally
        {
            DisposeConnection();
        }
    }

    /// <summary>Closes the connection without sending <c>QUIT</c>.</summary>
    public void Dispose()
    {
        if (disposed)
            return;
        disposed = true;
        DisposeConnection();
        GC.SuppressFinalize(this);
    }

    private void PrepareConnect()
    {
        if (disposed)
            throw new ObjectDisposedException(nameof(NewsbinClient));
        if (reader != null)
            throw new InvalidOperationException("The client is already connected.");
    }

    private void AttachConnection(TcpClient client)
    {
        aborted = false;
        tcpClient = client;
        networkStream = client.GetStream();
        var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        reader = new StreamReader(networkStream, encoding, detectEncodingFromByteOrderMarks: false, BufferSize, leaveOpen: true);
        writer = new StreamWriter(networkStream, encoding, BufferSize, leaveOpen: true) { NewLine = LineTerminator };
        ClientConnected?.Invoke(this, new ClientConnectedEventArgs());
    }

    private void DisposeConnection()
    {
        TcpClient? client = tcpClient;
        if (client == null)
            return;

        tcpClient = null;
        networkStream = null;
        reader?.Dispose();
        reader = null;
        // The socket may already be gone, so don't let a final flush throw.
        try
        {
            writer?.Dispose();
        }
        catch (Exception ex) when (ex is IOException || ex is ObjectDisposedException)
        {
        }
        writer = null;
        client.Dispose();
        ClientDisconnected?.Invoke(this, new ClientDisconnectedEventArgs());
    }

    private void AbortSocket()
    {
        aborted = true;
        tcpClient?.Dispose();
    }

    private string GetAuthString(Response greeting)
    {
        ParseGreeting(greeting.Message, out string? version, out string salt);
        NewsbinVersion = version;
        return AuthenticationHelper.GetAuthString(password, salt);
    }

    /// <summary>Splits a greeting like <c>Newsbin 6.90 - 0123456789ABCDEF</c> into version and the 16 character salt after the dash.</summary>
    internal static void ParseGreeting(string message, out string? version, out string salt)
    {
        version = null;
        salt = "";
        string[] parts = message.Split('-');
        if (parts.Length != 2)
            return;
        version = parts[0].Trim();
        if (parts[1].Length >= 17)
            salt = parts[1].Substring(1, 16);
    }

    private static void EnsureAuthenticated(Response response)
    {
        if (response.StatusCode != StatusCode.OK)
            throw new CommunicationException("Authentication failure", Command.Auth, response);
    }

    #endregion

    #region Low level commands

    /// <summary>Sends a command and reads the single response line.</summary>
    /// <param name="command">The command.</param>
    /// <param name="arguments">Arguments appended to the command, separated by spaces. Empty arguments are skipped.</param>
    /// <returns>The response. Only <see cref="StatusCode.ParseError"/> throws; check the status code for errors.</returns>
    public Response SendCommand(Command command, params string[] arguments)
    {
        WriteCommand(command, arguments);
        Response response = ReadResponse();
        if (response.StatusCode == StatusCode.ParseError)
            throw new CommunicationException("Unexpected response", command, response);
        return response;
    }

    /// <inheritdoc cref="SendCommand(Command, string[])"/>
    /// <param name="command">The command.</param>
    /// <param name="arguments">Arguments appended to the command, separated by spaces. Empty arguments are skipped.</param>
    /// <param name="cancellationToken">Cancels the call and closes the connection.</param>
    public async Task<Response> SendCommandAsync(Command command, string[]? arguments = null, CancellationToken cancellationToken = default)
    {
        await WriteCommandAsync(command, arguments, cancellationToken).ConfigureAwait(continueOnCapturedContext);
        Response response = await ReadResponseAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext);
        if (response.StatusCode == StatusCode.ParseError)
            throw new CommunicationException("Unexpected response", command, response);
        return response;
    }

    /// <summary>Sends a command without reading a response.</summary>
    /// <param name="command">The command.</param>
    /// <param name="arguments">Arguments appended to the command, separated by spaces. Empty arguments are skipped.</param>
    public void SendCommandWithoutResponse(Command command, params string[] arguments) =>
        WriteCommand(command, arguments);

    /// <summary>Sends a command without reading a response.</summary>
    /// <param name="command">The command.</param>
    /// <param name="arguments">Arguments appended to the command, separated by spaces. Empty arguments are skipped.</param>
    /// <param name="cancellationToken">Cancels the call and closes the connection.</param>
    public Task SendCommandWithoutResponseAsync(Command command, string[]? arguments = null, CancellationToken cancellationToken = default) =>
        WriteCommandAsync(command, arguments, cancellationToken);

    /// <summary>Sends a command that returns a list of data lines terminated by a single dot.</summary>
    /// <param name="command">The command.</param>
    /// <param name="arguments">Arguments appended to the command, separated by spaces. Empty arguments are skipped.</param>
    /// <exception cref="CommunicationException">Newsbin didn't answer <see cref="StatusCode.OK"/>.</exception>
    public ListResponse SendListCommand(Command command, params string[] arguments)
    {
        WriteCommand(command, arguments);
        var result = new ListResponse(ReadLine());
        OnReceivedResponse(result);
        EnsureListOk(command, result);

        string data = ReadLine();
        while (data != ".")
        {
            OnReceivedListItem(result, data);
            data = ReadLine();
        }
        return result;
    }

    /// <inheritdoc cref="SendListCommand(Command, string[])"/>
    /// <param name="command">The command.</param>
    /// <param name="arguments">Arguments appended to the command, separated by spaces. Empty arguments are skipped.</param>
    /// <param name="cancellationToken">Cancels the call and closes the connection.</param>
    public async Task<ListResponse> SendListCommandAsync(Command command, string[]? arguments = null, CancellationToken cancellationToken = default)
    {
        await WriteCommandAsync(command, arguments, cancellationToken).ConfigureAwait(continueOnCapturedContext);
        var result = new ListResponse(await ReadLineAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext));
        OnReceivedResponse(result);
        EnsureListOk(command, result);

        string data = await ReadLineAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext);
        while (data != ".")
        {
            OnReceivedListItem(result, data);
            data = await ReadLineAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext);
        }
        return result;
    }

    /// <summary>Sends a command whose response message starts with a number, and returns that number.</summary>
    /// <param name="command">The command.</param>
    /// <param name="arguments">Arguments appended to the command, separated by spaces. Empty arguments are skipped.</param>
    /// <exception cref="CommunicationException">Newsbin returned an error.</exception>
    public long SendLongCommand(Command command, params string[] arguments) =>
        ParseLong(command, SendCommand(command, arguments));

    /// <inheritdoc cref="SendLongCommand(Command, string[])"/>
    /// <param name="command">The command.</param>
    /// <param name="arguments">Arguments appended to the command, separated by spaces. Empty arguments are skipped.</param>
    /// <param name="cancellationToken">Cancels the call and closes the connection.</param>
    public async Task<long> SendLongCommandAsync(Command command, string[]? arguments = null, CancellationToken cancellationToken = default) =>
        ParseLong(command, await SendCommandAsync(command, arguments, cancellationToken).ConfigureAwait(continueOnCapturedContext));

    /// <summary>Reads one response line.</summary>
    public Response GetResponse() => ReadResponse();

    /// <summary>Reads one response line.</summary>
    /// <param name="cancellationToken">Cancels the call and closes the connection.</param>
    public Task<Response> GetResponseAsync(CancellationToken cancellationToken = default) => ReadResponseAsync(cancellationToken);

    #endregion

    #region Helpers used by the commands

    private void Execute(Command command, params string[] arguments) =>
        EnsureSuccess(command, SendCommand(command, arguments));

    private async Task ExecuteAsync(Command command, CancellationToken cancellationToken, params string[] arguments) =>
        EnsureSuccess(command, await SendCommandAsync(command, arguments, cancellationToken).ConfigureAwait(continueOnCapturedContext));

    private IReadOnlyList<T> List<T>(Command command, Func<string, T> parse, params string[] arguments) =>
        Map(SendListCommand(command, arguments), parse);

    private async Task<IReadOnlyList<T>> ListAsync<T>(Command command, Func<string, T> parse, CancellationToken cancellationToken, params string[] arguments) =>
        Map(await SendListCommandAsync(command, arguments, cancellationToken).ConfigureAwait(continueOnCapturedContext), parse);

    private static IReadOnlyList<T> Map<T>(ListResponse response, Func<string, T> parse)
    {
        var result = new List<T>(response.ListItems.Count);
        foreach (string item in response.ListItems)
            result.Add(parse(item));
        return result;
    }

    private static string Identity(string value) => value;

    private static string Arg(long value) => value.ToString(CultureInfo.InvariantCulture);

    private static string Flag(bool value) => value ? "1" : "0";

    private static string OnOff(bool value) => value ? "ON" : "OFF";

    private static void EnsureSuccess(Command command, Response response)
    {
        if (!response.IsSuccess)
            throw new CommunicationException(response.Message.Length > 0 ? response.Message : response.StatusCode.ToString(), command, response);
    }

    private static void EnsureListOk(Command command, ListResponse response)
    {
        if (response.StatusCode == StatusCode.ParseError)
            throw new CommunicationException("Unexpected response", command, response);
        if (response.StatusCode != StatusCode.OK)
            throw new CommunicationException(response.Message, command, response);
    }

    private static long ParseLong(Command command, Response response)
    {
        EnsureSuccess(command, response);
        string[] parts = response.Message.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0 || !long.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out long value))
            throw new CommunicationException("Expected a number in the response", command, response);
        return value;
    }

    #endregion

    #region Line and stream I/O

    internal static string BuildCommandLine(Command command, string[]? arguments)
    {
        if (!CommandStrings.Strings.TryGetValue(command, out string? text))
            throw new ArgumentOutOfRangeException(nameof(command), command, "Unknown command.");

        var builder = new StringBuilder(text);
        foreach (string argument in arguments ?? NoArguments)
        {
            if (string.IsNullOrEmpty(argument))
                continue;
            if (argument.IndexOf('\r') >= 0 || argument.IndexOf('\n') >= 0)
                throw new ArgumentException("Command arguments can't contain line breaks.", nameof(arguments));
            builder.Append(' ').Append(argument);
        }
        return builder.ToString();
    }

    private void WriteCommand(Command command, string[]? arguments)
    {
        string line = BuildCommandLine(command, arguments);
        StreamWriter w = Writer;
        w.Write(line);
        w.Write(LineTerminator);
        w.Flush();
        ClientSentCommand?.Invoke(this, new ClientSentCommandEventArgs(line));
    }

    private async Task WriteCommandAsync(Command command, string[]? arguments, CancellationToken cancellationToken)
    {
        string line = BuildCommandLine(command, arguments);
        StreamWriter w = Writer;
        await RunIoAsync(async () =>
        {
            await w.WriteAsync(line + LineTerminator).ConfigureAwait(continueOnCapturedContext);
            await w.FlushAsync().ConfigureAwait(continueOnCapturedContext);
            return true;
        }, cancellationToken).ConfigureAwait(continueOnCapturedContext);
        ClientSentCommand?.Invoke(this, new ClientSentCommandEventArgs(line));
    }

    private Response ReadResponse()
    {
        var response = new Response(ReadLine());
        OnReceivedResponse(response);
        return response;
    }

    private async Task<Response> ReadResponseAsync(CancellationToken cancellationToken)
    {
        var response = new Response(await ReadLineAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext));
        OnReceivedResponse(response);
        return response;
    }

    private string ReadLine()
    {
        string? line = Reader.ReadLine();
        return line ?? throw ConnectionClosed();
    }

    private async Task<string> ReadLineAsync(CancellationToken cancellationToken)
    {
        StreamReader r = Reader;
        string? line = await RunIoAsync(() => r.ReadLineAsync(), cancellationToken).ConfigureAwait(continueOnCapturedContext);
        return line ?? throw ConnectionClosed();
    }

    private void CopyExactly(Stream source, Stream destination, long count)
    {
        byte[] buffer = new byte[(int)Math.Min(BufferSize, Math.Max(count, 1))];
        long remaining = count;
        while (remaining > 0)
        {
            int read = source.Read(buffer, 0, (int)Math.Min(buffer.Length, remaining));
            if (read == 0)
                throw new EndOfStreamException($"The stream ended with {remaining} of {count} bytes left.");
            destination.Write(buffer, 0, read);
            remaining -= read;
        }
        destination.Flush();
    }

    private Task CopyExactlyAsync(Stream source, Stream destination, long count, CancellationToken cancellationToken) =>
        RunIoAsync(async () =>
        {
            byte[] buffer = new byte[(int)Math.Min(BufferSize, Math.Max(count, 1))];
            long remaining = count;
            while (remaining > 0)
            {
                int read = await source.ReadAsync(buffer, 0, (int)Math.Min(buffer.Length, remaining), cancellationToken)
                    .ConfigureAwait(continueOnCapturedContext);
                if (read == 0)
                    throw new EndOfStreamException($"The stream ended with {remaining} of {count} bytes left.");
                await destination.WriteAsync(buffer, 0, read, cancellationToken).ConfigureAwait(continueOnCapturedContext);
                remaining -= read;
            }
            await destination.FlushAsync(cancellationToken).ConfigureAwait(continueOnCapturedContext);
            return true;
        }, cancellationToken);

    /// <summary>
    /// Runs socket I/O so that cancellation works on every target framework: a cancelled token closes the socket,
    /// which unblocks pending reads, and the connection is then torn down.
    /// </summary>
    private async Task<T> RunIoAsync<T>(Func<Task<T>> io, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CancellationTokenRegistration registration = cancellationToken.CanBeCanceled
            ? cancellationToken.Register(AbortSocket)
            : default;
        try
        {
            T result = await io().ConfigureAwait(continueOnCapturedContext);
            if (aborted)
                throw new OperationCanceledException(cancellationToken);
            return result;
        }
        catch (Exception ex) when (aborted || cancellationToken.IsCancellationRequested)
        {
            DisposeConnection();
            if (ex is OperationCanceledException)
                throw;
            throw new OperationCanceledException("The operation was canceled and the connection to Newsbin was closed.", ex, cancellationToken);
        }
        finally
        {
            registration.Dispose();
        }
    }

    private void OnReceivedResponse(Response response) =>
        ClientReceivedResponse?.Invoke(this, new ClientReceivedResponseEventArgs(response));

    private void OnReceivedListItem(ListResponse response, string item)
    {
        ClientReceivedListItem?.Invoke(this, new ClientReceivedListItemEventArgs(item));
        response.AddListItem(item);
    }

    private StreamReader Reader => reader ?? throw NotConnected();

    private StreamWriter Writer => writer ?? throw NotConnected();

    private NetworkStream Network => networkStream ?? throw NotConnected();

    private Exception NotConnected() =>
        disposed ? new ObjectDisposedException(nameof(NewsbinClient)) : new InvalidOperationException("The client is not connected.");

    private static CommunicationException ConnectionClosed() =>
        new CommunicationException("Newsbin closed the connection.");

    #endregion
}
