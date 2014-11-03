using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Newsbin.Remote
{
    public class ClientReceivedResponseEventArgs: EventArgs
    {
        protected Response response;
        protected List<string> responseList;
		protected int threadId;

		public int ThreadId
		{
			get { return this.threadId; }
		}

        public Response Response
        {
            get { return this.response; }
        }

        public IEnumerable<string> ResponseList
        {
            get { return this.responseList; }
        }

        public ClientReceivedResponseEventArgs(Response response)
        {
            this.response = response;
            this.responseList = new List<string>();
        }

        public ClientReceivedResponseEventArgs(Response response, IEnumerable<string> responseList)
        {
            this.response = response;
            this.responseList = new List<string>();
            this.responseList.AddRange(responseList);
			this.threadId = Thread.CurrentThread.ManagedThreadId;
        }
    }
}
