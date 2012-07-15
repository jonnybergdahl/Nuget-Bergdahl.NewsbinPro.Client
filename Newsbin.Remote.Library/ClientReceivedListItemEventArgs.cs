using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newsbin.Remote
{
    public class ClientReceivedListItemEventArgs: EventArgs
    {
        protected string listItem;

        public string ListItem
        {
            get { return this.listItem; }
        }

        public ClientReceivedListItemEventArgs(string listItem)
        {
            this.listItem = listItem;
        }
    }
}
