using System;

namespace FileBiggy.Exceptions
{
    public class EntityTypeException : Exception
    {
        public string Type { get; private set; }

        public EntityTypeException(string message, string type)
            : base(message)
        {
            Type = type;
        }
    }
}