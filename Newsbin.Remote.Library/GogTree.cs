using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newsbin.Remote
{
    public class GogTree
    {
        protected readonly Dictionary<string, List<String>> groupOfGroups;

        public IEnumerable<string> GroupOfGroupNames
        {
            get { return this.groupOfGroups.Keys; }
        }

        public IEnumerable<string> GetGroupOfGroupChildren(string groupOfGroupName)
        {
            return this.groupOfGroups[groupOfGroupName];
        }

        internal void Add(string groupOfGroupName, string groupName)
        {
            if (!this.groupOfGroups.Keys.Contains(groupOfGroupName))
                this.groupOfGroups.Add(groupOfGroupName, new List<string>());
            this.groupOfGroups[groupOfGroupName].Add(groupName);
        }

        public GogTree()
        {
            groupOfGroups = new Dictionary<string, List<string>>();
        }
    }
}
