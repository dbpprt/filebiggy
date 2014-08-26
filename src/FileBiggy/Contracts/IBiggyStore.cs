using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FileBiggy.Contracts
{
    public interface IBiggyStore<T>
    {
        List<T> All();
        void Clear();
        void Add(T item);
        void Add(List<T> items);
        T Update(T item);
        void Remove(T item);
        void Remove(IEnumerable<T> items);
        IQueryable<T> AsQueryable();
    }
}