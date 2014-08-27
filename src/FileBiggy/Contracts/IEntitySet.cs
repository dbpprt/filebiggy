using System.Collections.Generic;
using System.Linq;

namespace FileBiggy.Contracts
{
    public interface IEntitySet<T> : IEnumerable<T>, IAsyncEntitySet<T>
    {
        void Clear();
        int Count();
        T Update(T item);
        void Remove(T item);
        void Remove(IEnumerable<T> items);
        void Add(T item);
        void Add(IEnumerable<T> items);
        IQueryable<T> AsQueryable();
    }
}