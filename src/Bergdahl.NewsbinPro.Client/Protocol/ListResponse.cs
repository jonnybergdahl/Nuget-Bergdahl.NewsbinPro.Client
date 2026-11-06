using System.Collections.Generic;

namespace Bergdahl.NewsbinPro.Client;

/// <summary>
/// A response followed by a list of data lines, terminated by a line containing a single dot.
/// </summary>
public class ListResponse : Response
{
    private readonly List<string> listItems = new List<string>();

    /// <summary>The data lines that followed the status line.</summary>
    public IReadOnlyList<string> ListItems => listItems;

    /// <summary>Parses the status line of a list response.</summary>
    /// <param name="response">The raw status line.</param>
    public ListResponse(string response)
        : base(response)
    {
    }

    internal void AddListItem(string item) => listItems.Add(item);
}
