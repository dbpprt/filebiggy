using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FileBiggy.Contracts;
using FileBiggy.Properties;

namespace FileBiggy
{
    [UsedImplicitly]
    public class EntitySet<T> : IEntitySet<T> where T : new()
    {
        private readonly IBiggyStore<T> _store;

        public EntitySet(IBiggyStore<T> store)
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
    }
}