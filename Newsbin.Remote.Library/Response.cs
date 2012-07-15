using System;
using System.Linq;

namespace Newsbin.Remote
{
    public class Response
    {
        protected string statusCode = StatusCodes.ParseError;
        protected string message = "";

        public string StatusCode
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
                this.statusCode = response.Substring(0, 3);
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
