using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Newsbin.Remote
{
    public class ClientReceivedListItemEventArgs: EventArgs
    {
        protected string listItem;
		protected int threadId;

		public int ThreadId
		{
			get { return this.threadId; }
		}

        public string ListItem
        {
            get { return this.listItem; }
        }

        public ClientReceivedListItemEventArgs(string listItem)
        {
            this.listItem = listItem;
			this.threadId = Thread.CurrentThread.ManagedThreadId;
        }
    }
}
