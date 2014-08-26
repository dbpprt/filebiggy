using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FileBiggy.Contracts;

namespace FileBiggy
{
    public class BiggyList<T> : IBiggy<T> where T : new()
    {
        private readonly List<T> _items;
        private readonly IBiggyStore<T> _store;

        public BiggyList(IBiggyStore<T> store)
        {
            _store = store;
            _items = (_store != null) ? _store.Load() : new List<T>();
        }

        public IQueryable<T> AsQueryable()
        {
            return _store.AsQueryable();
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual void Clear()
        {
            if (_store != null)
            {
                _store.Clear();
            }
            _items.Clear();
        }

        public virtual int Count()
        {
            return _items.Count;
        }

        public virtual T Update(T item)
        {
            if (!_items.Contains(item))
            {
                // Figure out what to do here. Retreive Key From Store and evaluate?
                throw new InvalidOperationException(
                    @"The list does not contain a reference to the object passed as an argument. 
          Make sure you are passing a valid reference, or override Equals on the type being passed.");
            }
            T itemFromList = _items.ElementAt(_items.IndexOf(item));
            if (!ReferenceEquals(itemFromList, item))
            {
                //// The items are "equal" but do not refer to the same instance. 
                //// Somebody overrode Equals on the type passed as an argument. Replace:
                int index = _items.IndexOf(item);
                _items.RemoveAt(index);
                _items.Insert(index, item);
            }
            // From here forward, the item passed in refers to the item in the list. 
            if (_store != null)
            {
                _store.Update(item);
            }
            return item;
        }

        public virtual T Remove(T item)
        {
            _items.Remove(item);
            if (_store != null)
            {
                _store.Remove(item);
            }
            return item;
        }

        public IList<T> Remove(List<T> items)
        {
            foreach (T item in items)
            {
                _items.Remove(item);
            }
            if (_store != null)
            {
                _store.Remove(items);
            }
            return items;
        }

        public virtual T Add(T item)
        {
            if (_store != null)
            {
                _store.Add(item);
            }
            _items.Add(item);
            return item;
        }

        public virtual IList<T> Add(List<T> items)
        {
            if (_store != null)
            {
                _store.Add(items);
            }
            _items.AddRange(items);
            return items;
        }

    }
}