using System;
using System.Linq;
using System.Runtime.Serialization;

namespace Newsbin.Remote
{
    [Serializable]
    public class CommunicationException: Exception
    {
        protected Command command = Command.None;
        protected Response response;

        public CommunicationException()
        { 
        }

        public CommunicationException(string message)
            : base(message)
        { 
        }

        public CommunicationException(string message, Command command, Response response)
            : base(message)
        {
            this.command = command;
            this.response = response;
        }

        public CommunicationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected CommunicationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override string ToString()
        {
            return String.Format("{0} [{1}]-[{2]]", 
                                 this.Message, 
                                 this.command, 
                                 this.response);
        }
    }
}
