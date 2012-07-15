using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newsbin.Remote
{
    public class ClientReceivedResponseEventArgs: EventArgs
    {
        protected Response response;
        protected List<string> responseList;

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
        }
    }
}
