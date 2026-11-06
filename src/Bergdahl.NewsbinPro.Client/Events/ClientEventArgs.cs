using System;
using System.Collections.Generic;

namespace Bergdahl.NewsbinPro.Client;

/// <summary>Raised when the TCP connection to Newsbin has been opened.</summary>
public class ClientConnectedEventArgs : EventArgs
{
}

/// <summary>Raised when the connection to Newsbin has been closed.</summary>
public class ClientDisconnectedEventArgs : EventArgs
{
}

/// <summary>Base class for events that record the thread they were raised on.</summary>
public abstract class ClientThreadEventArgs : EventArgs
{
    /// <summary>The managed thread id the event was raised on.</summary>
    public int ThreadId { get; } = Environment.CurrentManagedThreadId;
}

/// <summary>Raised after a command line has been sent to Newsbin.</summary>
public class ClientSentCommandEventArgs : ClientThreadEventArgs
{
    /// <summary>The command line that was sent.</summary>
    public string Command { get; }

    /// <summary>Creates the event arguments.</summary>
    public ClientSentCommandEventArgs(string command)
    {
        Command = command;
    }
}

/// <summary>Raised when a response line has been received from Newsbin.</summary>
public class ClientReceivedResponseEventArgs : ClientThreadEventArgs
{
    private static readonly string[] NoLines = new string[0];

    /// <summary>The response.</summary>
    public Response Response { get; }

    /// <summary>Any data lines belonging to the response.</summary>
    public IReadOnlyList<string> ResponseList { get; }

    /// <summary>Creates the event arguments.</summary>
    public ClientReceivedResponseEventArgs(Response response)
        : this(response, NoLines)
    {
    }

    /// <summary>Creates the event arguments with data lines.</summary>
    public ClientReceivedResponseEventArgs(Response response, IEnumerable<string> responseList)
    {
        Response = response;
        ResponseList = new List<string>(responseList);
    }
}

/// <summary>Raised for each data line of a list response.</summary>
public class ClientReceivedListItemEventArgs : ClientThreadEventArgs
{
    /// <summary>The data line.</summary>
    public string ListItem { get; }

    /// <summary>Creates the event arguments.</summary>
    public ClientReceivedListItemEventArgs(string listItem)
    {
        ListItem = listItem;
    }
}
