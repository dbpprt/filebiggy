using System;
using System.Collections.Generic;
using System.Linq;

namespace FileBiggy.Contracts
{
    public interface IBiggy<T> : IEnumerable<T>
    {
        void Clear();
        int Count();
        T Update(T item);
        T Remove(T item);
        IList<T> Remove(List<T> items);
        T Add(T item);
        IList<T> Add(List<T> items);
        IQueryable<T> AsQueryable();
    }
}