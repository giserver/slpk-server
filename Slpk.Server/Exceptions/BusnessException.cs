using System;

namespace Slpk.Server.Exceptions
{
    public class BusnessException : Exception
    {
        public int StatusCode { get; }

        public BusnessException(int statusCode, string message) : base(message)
        {
            StatusCode = statusCode;
        }

        public BusnessException(int statusCode, string message, Exception? innerException) : base(message, innerException)
        {
            StatusCode = statusCode;
        }
    }
}
