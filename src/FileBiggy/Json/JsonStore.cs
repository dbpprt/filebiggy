using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileBiggy.Common;
using FileBiggy.Properties;
using Newtonsoft.Json;

namespace FileBiggy.Json
{
    [JsonConverter(typeof (BiggyListSerializer))]
    [UsedImplicitly]
    public class JsonStore<T> : FileSystemStore<T> where T : new()
    {
        public JsonStore(Dictionary<string, string> connectionString)
            : base(connectionString)
        {
        }

        protected string DatabaseFilePath
        {
            get
            {
                return FileUtilities.EnsureExists(
                    Path.Combine(DatabaseDirectory, String.Format("{0}.{1}", DatabaseName, "json")));
            }
        }

        protected List<T> Load(Stream stream)
        {
            var reader = new StreamReader(stream);
            var json = "[" + reader.ReadToEnd().Replace(Environment.NewLine, ",") + "]";
            return JsonConvert.DeserializeObject<List<T>>(json);
        }

        protected void FlushToDisk(Stream stream, List<T> items)
        {
            using (var outstream = new StreamWriter(stream))
            {
                var writer = new JsonTextWriter(outstream);
                var serializer = JsonSerializer.CreateDefault();
                var biggySerializer = new BiggyListSerializer();
                biggySerializer.WriteJson(writer, items, serializer);
            }
        }

        protected void Append(Stream stream, IEnumerable<T> items)
        {
            using (var writer = new StreamWriter(stream))
            {
                foreach (var json in items.Select(item => JsonConvert.SerializeObject(item)))
                {
                    writer.WriteLine(json);
                }
            }
        }

        protected override void RemoveFileSystemItems(IEnumerable<T> items)
        {
            using (var stream = new FileStream(DatabaseFilePath, FileMode.Create))
            {
                FlushToDisk(stream, Items.Select(tuple => tuple.Value).ToList());
            }
        }

        protected override void AddFileSystemItem(T item)
        {
            using (var stream = new FileStream(DatabaseFilePath, FileMode.Append))
            {
                Append(stream, new[] {item});
            }
        }

        protected override void AddFileSystemItems(List<T> item)
        {
            using (var stream = new FileStream(DatabaseFilePath, FileMode.Append))
            {
                Append(stream, Items.Select(_ => _.Value));
            }
        }

        protected override void ClearFileSystemItems()
        {
            using (var stream = new FileStream(DatabaseFilePath, FileMode.Create))
            {
                FlushToDisk(stream, new List<T>());
            }
        }

        protected override void RemoveFileSystemItem(T item)
        {
            // this is really ulgy.. updating 1 item causes the entire database file to be rewritten 
            using (var stream = new FileStream(DatabaseFilePath, FileMode.Create))
            {
                FlushToDisk(stream, Items.Select(tuple => tuple.Value).ToList());
            }
        }

        protected override void UpdateFileSystemItem(T item)
        {
            // this is really ulgy.. updating 1 item causes the entire database file to be rewritten 
            using (var stream = new FileStream(DatabaseFilePath, FileMode.Create))
            {
                FlushToDisk(stream, Items.Select(tuple => tuple.Value).ToList());
            }
        }

        protected override Dictionary<object, T> Initialize()
        {
            List<T> result;
            using (var stream = File.OpenRead(DatabaseFilePath))
            {
                result = Load(stream);
            }

            return result.ToDictionary(
                GetKey,
                value => value
                );
        }
    }
}