using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FileBiggy.Common;

namespace FileBiggy.Memory
{
    /// <summary>
    /// This is just an idea to implement key-value store at store base
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MemoryStore<T> : StoreBase<T> where T : new()
    {
        //private readonly List<T> _items;
        private readonly ReaderWriterLockSlim _lock;
        private readonly Dictionary<object, T> _items;

        public MemoryStore(Dictionary<string, string> connectionString)
            : base(connectionString)
        {
            _items = new Dictionary<object, T>();
            _lock = new ReaderWriterLockSlim();
        }

        public override T Find(object id)
        {
            try
            {
                _lock.EnterReadLock();
                T result;

                if (_items.TryGetValue(id, out result))
                {
                    return result;
                }

                throw new ArgumentException("id");
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public override void Add(T item)
        {
            try
            {
                _lock.EnterWriteLock();
                _items.Add(GetKey(item), item);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override List<T> All()
        {
            try
            {
                _lock.EnterReadLock();
                return _items.Select(tuple => tuple.Value).ToList();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public override void Add(IEnumerable<T> items)
        {
            try
            {
                _lock.EnterWriteLock();

                foreach (var item in items)
                {
                    _items.Add(GetKey(item), item);
                }
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
                _items.Remove(GetKey(item));
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
                    _items.Remove(GetKey(item));
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override Task<List<T>> AllAsync()
        {
            return Task.FromResult(All());
        }

        public override Task ClearAsync()
        {
            Clear();
            return Task.FromResult(0);
        }

        public override Task AddAsync(T item)
        {
            Add(item);
            return Task.FromResult(0);
        }

        public override Task AddAsync(IEnumerable<T> items)
        {
            Add(items);
            return Task.FromResult(0);
        }

        public override Task<T> UpdateAsync(T item)
        {
            return Task.FromResult(Update(item));
        }

        public override Task RemoveAsync(T item)
        {
            Remove(item);
            return Task.FromResult(0);
        }

        public override Task RemoveAsync(IEnumerable<T> items)
        {
            Remove(items);
            return Task.FromResult(0);
        }

        public override T Update(T item)
        {
            try
            {
                _lock.EnterWriteLock();

                _items.Remove(GetKey(item));
                _items.Add(GetKey(item), item);

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
                return _items.Select(tuple => tuple.Value).ToList().AsQueryable();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }
}