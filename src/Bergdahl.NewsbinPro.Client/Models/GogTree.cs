using System.Collections.Generic;

namespace Bergdahl.NewsbinPro.Client;

/// <summary>
/// Groups of groups (GOGs) and the groups they contain.
/// </summary>
public class GogTree
{
    private static readonly string[] Empty = new string[0];
    private readonly Dictionary<string, List<string>> groupOfGroups = new Dictionary<string, List<string>>();

    /// <summary>The names of all groups of groups.</summary>
    public IEnumerable<string> GroupOfGroupNames => groupOfGroups.Keys;

    /// <summary>Returns the groups in a group of groups, or an empty list if the name is unknown.</summary>
    public IReadOnlyList<string> GetGroupOfGroupChildren(string groupOfGroupName) =>
        groupOfGroups.TryGetValue(groupOfGroupName, out List<string>? children) ? children : Empty;

    internal void AddGroupOfGroups(string groupOfGroupName)
    {
        if (!groupOfGroups.ContainsKey(groupOfGroupName))
            groupOfGroups.Add(groupOfGroupName, new List<string>());
    }

    internal void Add(string groupOfGroupName, string groupName)
    {
        AddGroupOfGroups(groupOfGroupName);
        groupOfGroups[groupOfGroupName].Add(groupName);
    }
}
