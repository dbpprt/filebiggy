using System;

namespace FileBiggy.Exceptions
{
    public class IdentityAttributeMismatchException : Exception
    {
        public IdentityAttributeMismatchException(string message)
            : base(message)
        { }
        
    }
}
