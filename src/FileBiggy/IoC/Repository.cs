using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileBiggy.Contracts;

namespace FileBiggy.IoC
{
    public class Repository<T> : IEntitySet<T> where T : class, new()
    {
        private readonly IBiggyContext _context;
        private readonly IEntitySet<T> _underlayingSet;

        public Repository(
            IBiggyContext context)
        {
            _context = context;
            _underlayingSet = _context.Set<T>();
        }

        protected virtual void BeforeAdd(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
        }

        protected virtual void BeforeUpdate(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
        }

        protected virtual void BeforeDelete(T item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _underlayingSet.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _underlayingSet).GetEnumerator();
        }

        public Task ClearAsync()
        {
            return _underlayingSet.ClearAsync();
        }

        public Task<T> UpdateAsync(T item)
        {
            BeforeUpdate(item);
            return _underlayingSet.UpdateAsync(item);
        }

        public Task RemoveAsync(T item)
        {
            BeforeDelete(item);
            return _underlayingSet.RemoveAsync(item);
        }

        public Task RemoveAsync(IEnumerable<T> items)
        {
            var enumerable = items as T[] ?? items.ToArray();

            foreach (var item in enumerable)
            {
                BeforeDelete(item);
            }
            return _underlayingSet.RemoveAsync(enumerable);
        }

        public Task AddAsync(T item)
        {
            BeforeAdd(item);
            return _underlayingSet.AddAsync(item);
        }

        public Task AddAsync(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                BeforeAdd(item);
            }
            return _underlayingSet.AddAsync(items);
        }

        public void Clear()
        {
            _underlayingSet.Clear();
        }

        public int Count()
        {
            return _underlayingSet.Count();
        }

        public T Update(T item)
        {
            BeforeUpdate(item);
            return _underlayingSet.Update(item);
        }

        public void Remove(T item)
        {
            BeforeDelete(item);
            _underlayingSet.Remove(item);
        }

        public void Remove(IEnumerable<T> items)
        {
            var enumerable = items as T[] ?? items.ToArray();

            foreach (var item in enumerable)
            {
                BeforeDelete(item);
            }
            _underlayingSet.Remove(enumerable);
        }

        public void Add(T item)
        {
            BeforeAdd(item);
            _underlayingSet.Add(item);
        }

        public void Add(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                BeforeAdd(item);
            }

            _underlayingSet.Add(items);
        }

        public IQueryable<T> AsQueryable()
        {
            return _underlayingSet.AsQueryable();
        }
    }
}