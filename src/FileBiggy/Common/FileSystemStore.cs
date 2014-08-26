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
        private Dictionary<object, T> _items;
        private readonly ReaderWriterLockSlim _lock;

        protected string DatabaseDirectory { get; private set; }

        protected abstract string DatabaseFilePath { get; }

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

                List<T> result;
                using (var stream = File.OpenRead(DatabaseFilePath))
                {
                    result = Load(stream);
                }

                _items = result.ToDictionary(
                    GetKey,
                    value => value
                    );
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

        private object GetKey(T item)
        {
            var identity = item.GetKeyFromEntity();
            return identity ?? Guid.NewGuid();
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

        public override void Clear()
        {
            try
            {
                _lock.EnterWriteLock();

                _items = new Dictionary<object, T>();
                using (var stream = new FileStream(DatabaseFilePath, FileMode.Create))
                {
                    FlushToDisk(stream, new List<T>());
                }
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
                using (var stream = new FileStream(DatabaseFilePath, FileMode.Append))
                {
                    Append(stream, new List<T> { item });
                }
                _items.Add(GetKey(item), item);
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
                using (var stream = new FileStream(DatabaseFilePath, FileMode.Append))
                {
                    Append(stream, items);
                }

                // this is not nice.. you want to add 10 items, it inserts 8 and the
                // ninth gets a duplicate key exception
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

        public override T Update(T item)
        {
            try
            {
                _lock.EnterWriteLock();

                _items.Remove(GetKey(item));
                _items.Add(GetKey(item), item);

                // this is really ulgy.. updating 1 item causes the entire database file to be rewritten 
                using (var stream = new FileStream(DatabaseFilePath, FileMode.Create))
                {
                    FlushToDisk(stream, _items.Select(tuple => tuple.Value).ToList());
                }

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
                _items.Remove(GetKey(item));

                // this is really ulgy.. updating 1 item causes the entire database file to be rewritten 
                using (var stream = new FileStream(DatabaseFilePath, FileMode.Create))
                {
                    FlushToDisk(stream, _items.Select(tuple => tuple.Value).ToList());
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public override void Remove(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                _items.Remove(GetKey(item));
            }

            using (var stream = new FileStream(DatabaseFilePath, FileMode.Create))
            {
                FlushToDisk(stream, _items.Select(tuple => tuple.Value).ToList());
            }
        }

        public override IQueryable<T> AsQueryable()
        {
            try
            {
                _lock.EnterReadLock();
                return _items.Select(tuple => tuple.Value).ToList().AsQueryable();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public abstract List<T> Load(Stream stream);

        public abstract void FlushToDisk(Stream stream, List<T> items);

        public abstract void Append(Stream stream, IEnumerable<T> items);
    }
}