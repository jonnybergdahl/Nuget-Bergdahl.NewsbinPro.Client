using System;
using System.Linq;

namespace Newsbin.Remote
{
    public class Response
    {
        protected StatusCode statusCode = StatusCode.ParseError;
        protected string message = "";

        public StatusCode StatusCode
        {
            get { return this.statusCode; }
        }

        public string Message
        {
            get { return this.message; }
        }

        protected Response()
        { }

        public Response(string response)
        {
            if (response.Length > 3)
            {
                int status;
                if (int.TryParse(response.Substring(0, 3), out status))
                    this.statusCode = (StatusCode)status;
                this.message = response.Substring(3);
            }
            else
            {
                this.message = response;
            }
        }

        public override string ToString()
        {
            return String.Format("{0} {1}", this.statusCode, this.message); 
        }
    }
}
