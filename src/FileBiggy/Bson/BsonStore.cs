using System;
using System.Collections.Generic;
using System.IO;
using FileBiggy.Common;
using FileBiggy.Exceptions;
using FileBiggy.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace FileBiggy.Bson
{
    [UsedImplicitly]
    public class BsonStore<T> : FileSystemStore<T> where T : new()
    {
        private JsonSerializer _serializer;
        private const string FileExtension = ".bson";

        public BsonStore(Dictionary<string, string> connectionString)
            : base(connectionString)
        {
            // BsonStore only accepts guid as identity 
            // because we want to name files with the key
            var identity = typeof (T).GetKeyFromEntityType();
            if (identity.PropertyType != typeof (Guid))
            {
                throw new InvalidIdentityTypeException("BsonStore requires a Guid identity", typeof (Guid));
            }

            _serializer = new JsonSerializer();
        }

        private void Serialize(Stream stream, T entity)
        {
            using (var writer = new BsonWriter(stream))
            {
                _serializer.Serialize(writer, entity);
            }
        }

        private T Deserialize(Stream stream)
        {
            using (var reader = new BsonReader(stream))
            {
                _serializer = new JsonSerializer();

                return _serializer.Deserialize<T>(reader);
            }
        }

        private string FilePath(T entity)
        {
            return Path.Combine(DatabaseDirectory, ((Guid) GetKey(entity)).ToString() + FileExtension);
        }

        protected override void RemoveFileSystemItems(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                RemoveFileSystemItem(item);
            }
        }

        protected override void AddFileSystemItem(T item)
        {
            UpdateFileSystemItem(item);
        }

        protected override void AddFileSystemItems(List<T> items)
        {
            foreach (var item in items)
            {
                AddFileSystemItem(item);
            }
        }

        protected override void ClearFileSystemItems()
        {
            var files = Directory.EnumerateFiles(DatabaseDirectory, string.Format("*{0}", FileExtension));

            foreach (var file in files)
            {
                File.Delete(file);
            }
        }

        protected override void RemoveFileSystemItem(T item)
        {
            var file = FilePath(item);
            File.Delete(file);
        }

        protected override void UpdateFileSystemItem(T item)
        {
            var file = FilePath(item);

            using (var fs = File.Open(file, FileMode.Create))
            {
                Serialize(fs, item);
            }
        }

        protected override Dictionary<object, T> Initialize()
        {
            var files = Directory.EnumerateFiles(DatabaseDirectory, "*.bson");
            var result = new Dictionary<object, T>();

            foreach (var file in files)
            {
                using (var fs = File.OpenRead(file))
                {
                    var entity = Deserialize(fs);
                    result.Add(GetKey(entity), entity);
                }
            }

            return result;
        }
    }
}