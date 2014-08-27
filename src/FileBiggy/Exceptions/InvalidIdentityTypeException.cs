using System;

namespace FileBiggy.Exceptions
{
    public class InvalidIdentityTypeException : Exception
    {
        public Type ExpectedType { get; set; }

        public InvalidIdentityTypeException(string message, Type expectedType)
            : base(message)
        {
            ExpectedType = expectedType;
        }
    }
}