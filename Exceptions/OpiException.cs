using System;

namespace UTG.Exceptions
{
    public class OpiException : Exception
    {
        public int ResponseStatusCode {get; set;}
        public OpiException()
        {
        }
        public OpiException(string message,int httpStatusCode) : base(message)
        {
            ResponseStatusCode = httpStatusCode;
        }

        public OpiException(string message) : base(message)
        {
           
        }
    }
}
