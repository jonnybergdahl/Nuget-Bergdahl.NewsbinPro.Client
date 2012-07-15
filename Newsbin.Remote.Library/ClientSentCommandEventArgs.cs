using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newsbin.Remote
{
    public class ClientSentCommandEventArgs: EventArgs
    {
        protected string command;
 
        public string Command
        {
            get { return this.command; }
        }

        public ClientSentCommandEventArgs(string command)
        {
            this.command = command;
        }
    }
}
