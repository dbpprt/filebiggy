using System;

namespace FileBiggy.Exceptions
{
    public class InvalidConnectionStringException : Exception
    {
        public string ConnectionString { get; private set; }

        public InvalidConnectionStringException(string message, string connectionString) : base(message)
        {
            ConnectionString = connectionString;
        }
        
    }
}
