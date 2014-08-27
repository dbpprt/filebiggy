using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using FileBiggy.Exceptions;

namespace FileBiggy.Common
{
    public abstract class FileSystemStore<T> : StoreBase<T> where T : new()
    {
        protected Dictionary<object, T> Items { get; private set; }
        private readonly ReaderWriterLockSlim _lock;

        protected string DatabaseDirectory { get; private set; }

        protected FileSystemStore(Dictionary<string, string> connectionString)
            : base(connectionString)
        {
            _lock = new ReaderWriterLockSlim();

            string dbPath;
            if (!connectionString.TryGetValue(ConnectionStringConstants.Path, out dbPath))
            {
                throw new InvalidConnectionStringException("The connection string must provide a valid dbPath", "");
            }

            DatabaseDirectory = DirectoryUtilities.EnsureExists(dbPath);

            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            try
            {
                _lock.EnterWriteLock();

                Items = Initialize();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override T Find(object id)
        {
            try
            {
                _lock.EnterReadLock();
                T result;

                if (Items.TryGetValue(id, out result))
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

        public override List<T> All()
        {
            try
            {
                _lock.EnterReadLock();
                return Items.Select(tuple => tuple.Value).ToList();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public override void Clear()
        {
            try
            {
                _lock.EnterWriteLock();

                Items = new Dictionary<object, T>();
                ClearFileSystemItems();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override void Add(T item)
        {
            try
            {
                _lock.EnterWriteLock();
                Items.Add(GetKey(item), item);
                AddFileSystemItem(item);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override void Add(List<T> items)
        {
            try
            {
                _lock.EnterWriteLock();
                
                // this is not nice.. you want to add 10 items, it inserts 8 and the
                // ninth gets a duplicate key exception
                foreach (var item in items)
                {
                    Items.Add(GetKey(item), item);
                }

                AddFileSystemItems(items);
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

                Items.Remove(GetKey(item));
                Items.Add(GetKey(item), item);

                UpdateFileSystemItem(item);

                return item;
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
                Items.Remove(GetKey(item));

                RemoveFileSystemItem(item);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override void Remove(IEnumerable<T> items)
        {
            var enumerable = items as T[] ?? items.ToArray();

            foreach (var item in enumerable)
            {
                Items.Remove(GetKey(item));
            }

            RemoveFileSystemItems(enumerable);
        }

        public override IQueryable<T> AsQueryable()
        {
            try
            {
                _lock.EnterReadLock();
                return Items.Select(tuple => tuple.Value).ToList().AsQueryable();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        protected abstract void RemoveFileSystemItems(IEnumerable<T> items);

        protected abstract void AddFileSystemItem(T item);

        protected abstract void AddFileSystemItems(List<T> item);

        protected abstract void ClearFileSystemItems();

        protected abstract void RemoveFileSystemItem(T item);

        protected abstract void UpdateFileSystemItem(T item);

        protected abstract Dictionary<object, T> Initialize();
    }
}