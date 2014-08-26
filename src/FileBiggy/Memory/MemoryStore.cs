using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using FileBiggy.Contracts;

namespace FileBiggy.Memory
{
    public class MemoryStore<T> : StoreBase<T> where T : new()
    {
        private readonly List<T> _items;
        private readonly ReaderWriterLockSlim _lock;

        public MemoryStore(Dictionary<string, string> connectionString) 
            : base(connectionString)
        {
            _items = new List<T>();
            _lock = new ReaderWriterLockSlim();
        }

        public override void Add(T item)
        {
            try
            {
                _lock.EnterWriteLock();
                _items.Add(item);
            }
            finally
            {
                _lock.ExitWriteLock();    
            }
           
        }

        public override List<T> Load()
        {
            try
            {
                _lock.EnterReadLock();
                return _items.ToList();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public override void Add(List<T> items)
        {
            try
            {
                _lock.EnterWriteLock();
                _items.AddRange(items);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override void Clear()
        {
            try
            {
                _lock.EnterWriteLock();
                _items.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override void Remove(T item)
        {
            try
            {
                _lock.EnterWriteLock();
                _items.Remove(item);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override void Remove(IEnumerable<T> items)
        {
            try
            {
                _lock.EnterWriteLock();

                foreach (var item in items)
                {
                    _items.Remove(item);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override T Update(T item)
        {
            try
            {
                _lock.EnterWriteLock();

                var index = _items.IndexOf(item);

                if (index <= -1)
                {
                    return item;
                }

                _items.RemoveAt(index);
                _items.Insert(index, item);
                return item;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override IQueryable<T> AsQueryable()
        {
            try
            {
                _lock.EnterReadLock();
                // this should create a unique list instance, so that we dont get any
                // side effects when using the returned list and exit the readlock
                return _items.ToList().AsQueryable();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }
}