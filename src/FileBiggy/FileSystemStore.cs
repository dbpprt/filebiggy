using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileBiggy.Common;
using FileBiggy.Contracts;
using FileBiggy.Exceptions;

namespace FileBiggy
{
    public abstract class FileSystemStore<T> : StoreBase<T> where T : new()
    {
        private const string DbPathKey = "dbpath";
        private List<T> _items;

        protected string DatabaseDirectory { get; private set; }

        protected abstract string DatabaseFilePath { get; }

        protected FileSystemStore(Dictionary<string, string> connectionString)
            : base(connectionString)
        {
            string dbPath;
            if (!connectionString.TryGetValue(DbPathKey, out dbPath))
            {
                throw new InvalidConnectionStringException("The connection string must provide a valid dbPath", "");
            }

            DatabaseDirectory = DirectoryUtilities.EnsureExists(dbPath);
        }

        public override List<T> Load()
        {
            List<T> result;

            using (var stream = File.OpenRead(DatabaseFilePath))
            {
                result = Load(stream);
            }

            _items = result.ToList();
            return result;
        }

        public override void Clear()
        {
            _items = new List<T>();
            using (var stream = new FileStream(DatabaseFilePath, FileMode.Create))
            {
                FlushToDisk(stream, _items);
            }
        }

        public override void Add(T item)
        {
            using (var stream = new FileStream(DatabaseFilePath, FileMode.Append))
            {
                Append(stream, new List<T> { item });
            }
            _items.Add(item);
        }

        public override void Add(List<T> items)
        {
            using (var stream = new FileStream(DatabaseFilePath, FileMode.Append))
            {
                Append(stream, items);
            }
            _items.AddRange(items);
        }

        // IUPDATEABLEBIGGYSTORE IMPLEMENTATION:

        public override T Update(T item)
        {
            if (!_items.Contains(item))
            {
                // Figure out what to do here. Retreive Key From Store and evaluate? Throw for now:
                throw new InvalidOperationException(
                    @"The list does not contain a reference to the object passed as an argument. 
          Make sure you are passing a valid reference, or override Equals on the type being passed.");
            }
            T itemFromList = _items.ElementAt(_items.IndexOf(item));
            if (!ReferenceEquals(itemFromList, item))
            {
                // The items are "equal" but do not refer to the same instance. 
                // Somebody overrode Equals on the type passed as an argument. Replace:
                var index = _items.IndexOf(item);
                _items[index] = item;
            }
            // Otherwise, the item passed is reference-equal. item now refers to it. Process as normal
            using (var stream = new FileStream(DatabaseFilePath, FileMode.Create))
            {
                FlushToDisk(stream, _items);
            }
            // The item in the list now refers to the item passed in, including updated data:
            return item;
        }

        public override void Remove(T item)
        {
            _items.Remove(item);
            using (var stream = new FileStream(DatabaseFilePath, FileMode.Create))
            {
                FlushToDisk(stream, _items);
            }
        }

        public override void Remove(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                _items.Remove(item);
            }
            using (var stream = new FileStream(DatabaseFilePath, FileMode.Create))
            {
                FlushToDisk(stream, _items);
            }
        }

        public override IQueryable<T> AsQueryable()
        {
            return _items.AsQueryable();
        }

        public abstract List<T> Load(Stream stream);

        public abstract void FlushToDisk(Stream stream, List<T> items);

        public abstract void Append(Stream stream, IEnumerable<T> items);
    }
}