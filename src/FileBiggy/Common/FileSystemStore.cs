using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FileBiggy.Contracts;
using FileBiggy.Exceptions;
using Nito.AsyncEx;

namespace FileBiggy.Common
{
    public abstract class FileSystemStore<T> : StoreBase<T> where T : new()
    {
        protected Dictionary<object, T> Items { get; private set; }
        private readonly AsyncReaderWriterLock _lock;

        protected string DatabaseDirectory { get; private set; }

        protected FileSystemStore(Dictionary<string, string> connectionString)
            : base(connectionString)
        {
            _lock = new AsyncReaderWriterLock();

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
            using (_lock.WriterLock())
            {
                Items = Initialize();
            }
        }

        public override T Find(object id)
        {
            using (_lock.ReaderLock())
            {
                T result;

                if (Items.TryGetValue(id, out result))
                {
                    return result;
                }

                throw new ArgumentException("id");
            }
        }

        public override List<T> All()
        {
            using (_lock.ReaderLock())
            {
                return Items.Select(tuple => tuple.Value).ToList();
            }
        }

        public override void Clear()
        {
            using (_lock.WriterLock())
            {
                Items = new Dictionary<object, T>();
                ClearFileSystemItems();
            }
        }

        public override void Add(T item)
        {
            using (_lock.WriterLock())
            {
                Items.Add(GetKey(item), item);
                AddFileSystemItem(item);
            }
        }

        public override void Add(List<T> items)
        {
            using (_lock.WriterLock())
            {
                // this is not nice.. you want to add 10 items, it inserts 8 and the
                // ninth gets a duplicate key exception
                foreach (var item in items)
                {
                    Items.Add(GetKey(item), item);
                }

                AddFileSystemItems(items);
            }
        }

        public override T Update(T item)
        {
            using (_lock.WriterLock())
            {
                Items.Remove(GetKey(item));
                Items.Add(GetKey(item), item);

                UpdateFileSystemItem(item);

                return item;
            }
        }

        public override void Remove(T item)
        {
            using (_lock.WriterLock())
            {
                Items.Remove(GetKey(item));
                RemoveFileSystemItem(item);
            }
        }

        public override void Remove(IEnumerable<T> items)
        {
            using (_lock.WriterLock())
            {
                var enumerable = items as T[] ?? items.ToArray();

                foreach (var item in enumerable)
                {
                    Items.Remove(GetKey(item));
                }

                RemoveFileSystemItems(enumerable);
            }
        }

        public override IQueryable<T> AsQueryable()
        {
            using (_lock.ReaderLock())
            {
                return Items.Select(tuple => tuple.Value).ToList().AsQueryable();
            }
        }

        public override async Task AddAsync(List<T> items)
        {
            using (await _lock.WriterLockAsync())
            {
                // this is not nice.. you want to add 10 items, it inserts 8 and the
                // ninth gets a duplicate key exception
                foreach (var item in items)
                {
                    Items.Add(GetKey(item), item);
                }

                await AddFileSystemItemsAsync(items);
            }
        }

        public override async Task AddAsync(T item)
        {
            using (await _lock.WriterLockAsync())
            {
                Items.Add(GetKey(item), item);

                await AddFileSystemItemAsync(item);
            }
        }

        public override Task<List<T>> AllAsync()
        {
            return Task.FromResult(All());
        }

        public override async Task ClearAsync()
        {
            using (await _lock.WriterLockAsync())
            {
                Items = new Dictionary<object, T>();
                await ClearFileSystemItemsAsync();
            }
        }

        public override async Task RemoveAsync(IEnumerable<T> items)
        {
            using (await _lock.WriterLockAsync())
            {
                var enumerable = items as T[] ?? items.ToArray();

                foreach (var item in enumerable)
                {
                    Items.Remove(GetKey(item));
                }

                await RemoveFileSystemItemsAsync(enumerable);
            }
        }

        public override async Task RemoveAsync(T item)
        {
            using (await _lock.WriterLockAsync())
            {
                Items.Remove(GetKey(item));
                await RemoveFileSystemItemAsync(item);
            }
        }

        public override async Task<T> UpdateAsync(T item)
        {
            using (await _lock.WriterLockAsync())
            {
                Items.Remove(GetKey(item));
                Items.Add(GetKey(item), item);

                await UpdateFileSystemItemAsync(item);

                return item;
            }
        }

        protected abstract void RemoveFileSystemItems(IEnumerable<T> items);

        protected abstract void AddFileSystemItem(T item);

        protected abstract void AddFileSystemItems(List<T> item);

        protected abstract void ClearFileSystemItems();

        protected abstract void RemoveFileSystemItem(T item);

        protected abstract void UpdateFileSystemItem(T item);

        protected abstract Dictionary<object, T> Initialize();


        // Async implementation
        protected abstract Task RemoveFileSystemItemsAsync(IEnumerable<T> items);

        protected abstract Task AddFileSystemItemAsync(T item);

        protected abstract Task AddFileSystemItemsAsync(List<T> item);

        protected abstract Task ClearFileSystemItemsAsync();

        protected abstract Task RemoveFileSystemItemAsync(T item);

        protected abstract Task UpdateFileSystemItemAsync(T item);

        protected abstract Task<Dictionary<object, T>> InitializeAsync();
    }
}