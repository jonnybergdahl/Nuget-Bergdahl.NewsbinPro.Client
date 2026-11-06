using System;

namespace Bergdahl.NewsbinPro.Client;

/// <summary>
/// Thrown when Newsbin returns an error, an unexpected response, or closes the connection.
/// </summary>
public class CommunicationException : Exception
{
    /// <summary>The command that failed, or <see cref="Client.Command.None"/>.</summary>
    public Command Command { get; } = Command.None;

    /// <summary>The response that caused the failure, if any.</summary>
    public Response? Response { get; }

    /// <summary>Creates an exception without a message.</summary>
    public CommunicationException()
    {
    }

    /// <summary>Creates an exception with a message.</summary>
    public CommunicationException(string message)
        : base(message)
    {
    }

    /// <summary>Creates an exception for a failed command.</summary>
    public CommunicationException(string message, Command command, Response? response)
        : base(message)
    {
        Command = command;
        Response = response;
    }

    /// <summary>Creates an exception wrapping another exception.</summary>
    public CommunicationException(string message, Exception inner)
        : base(message, inner)
    {
    }

    /// <inheritdoc />
    public override string ToString() =>
        $"{base.ToString()}{Environment.NewLine}Command: {Command}, Response: {Response}";
}
