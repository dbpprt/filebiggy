using System;

namespace FileBiggy.Contracts
{
    public interface IBiggyContext
    {
        IEntitySet<T> Set<T>();
        object Set(Type entityType);
    }
}
