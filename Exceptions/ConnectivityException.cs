using System;

namespace UTG.Exceptions
{
    public class ConnectivityException : Exception
    {
        public int ResponseStatusCode {get; set;}
        public ConnectivityException()
        {
        }
        public ConnectivityException(string message,int httpStatusCode) : base(message)
        {
            ResponseStatusCode = httpStatusCode;
        }

        public ConnectivityException(string message) : base(message)
        {
           
        }
    }
}
