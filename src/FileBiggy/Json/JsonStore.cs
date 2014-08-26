using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileBiggy.Common;
using Newtonsoft.Json;

namespace FileBiggy.Json
{
    [JsonConverter(typeof (BiggyListSerializer))]
    public class JsonStore<T> : FileSystemStore<T> where T : new()
    {
        public JsonStore(Dictionary<string, string> connectionString) 
            : base(connectionString)
        {

        }

        protected override string DatabaseFilePath
        {
            get
            {
                return FileUtilities.EnsureExists(
                    Path.Combine(DatabaseDirectory, String.Format("{0}.{1}", DatabaseName, "json")));
            }
        }

        public override List<T> Load(Stream stream)
        {
            var reader = new StreamReader(stream);
            var json = "[" + reader.ReadToEnd().Replace(Environment.NewLine, ",") + "]";
            return JsonConvert.DeserializeObject<List<T>>(json);
        }

        public override void FlushToDisk(Stream stream, List<T> items)
        {
            using (var outstream = new StreamWriter(stream))
            {
                var writer = new JsonTextWriter(outstream);
                var serializer = JsonSerializer.CreateDefault();
                var biggySerializer = new BiggyListSerializer();
                biggySerializer.WriteJson(writer, items, serializer);
            }
        }

        public override void Append(Stream stream, IEnumerable<T> items)
        {
            using (var writer = new StreamWriter(stream))
            {
                foreach (var json in items.Select(item => JsonConvert.SerializeObject(item)))
                {
                    writer.WriteLine(json);
                }
            }
        }
    }
}