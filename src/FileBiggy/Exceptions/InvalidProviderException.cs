using System;

namespace FileBiggy.Exceptions
{
    public class InvalidProviderException : Exception
    {
        public string Provider { get; private set; }

        public InvalidProviderException(string message, string provider)
            : base(message)
        {
            Provider = provider;
        }
    }
}