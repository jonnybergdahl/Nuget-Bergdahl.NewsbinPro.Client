using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Newsbin.Remote
{
    public class ClientSentCommandEventArgs: EventArgs
    {
        protected string command;
		protected int threadId;
 
		public int ThreadId
		{
			get { return this.threadId; }
		}

		public string Command
		{
            get { return this.command; }
        }

        public ClientSentCommandEventArgs(string command)
        {
            this.command = command;
			this.threadId = Thread.CurrentThread.ManagedThreadId;
        }
    }
}
