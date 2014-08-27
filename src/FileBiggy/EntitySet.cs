using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FileBiggy.Common;
using FileBiggy.Contracts;
using FileBiggy.Properties;

namespace FileBiggy
{
    [UsedImplicitly]
    public class EntitySet<T> : IEntitySet<T>, IAsyncEntitySet<T> where T : new()
    {
        private readonly StoreBase<T> _store;

        public EntitySet(StoreBase<T> store)
        {
            if (store == null)
            {
                throw new ArgumentNullException("store");
            }

            _store = store;
        }

        public IQueryable<T> AsQueryable()
        {
            return _store.AsQueryable();
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            return _store.All().GetEnumerator();
        }

        public virtual void Clear()
        {
            _store.Clear();
        }

        public virtual int Count()
        {
            return _store.AsQueryable().Count();
        }

        public virtual T Update(T item)
        {
            return _store.Update(item);
        }

        public virtual void Remove(T item)
        {
            _store.Remove(item);
        }

        public virtual void Remove(IEnumerable<T> items)
        {
            _store.Remove(items);
        }

        public virtual void Add(T item)
        {
            _store.Add(item);
        }

        public virtual void Add(List<T> items)
        {
            _store.Add(items);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Task ClearAsync()
        {
            return _store.ClearAsync();
        }

        public Task<T> UpdateAsync(T item)
        {
            return _store.UpdateAsync(item);
        }

        public Task RemoveAsync(T item)
        {
            return _store.RemoveAsync(item);
        }

        public Task RemoveAsync(IEnumerable<T> items)
        {
            return _store.RemoveAsync(items);
        }

        public Task AddAsync(T item)
        {
            return _store.AddAsync(item);
        }

        public Task AddAsync(List<T> items)
        {
            return _store.AddAsync(items);
        }
    }
}