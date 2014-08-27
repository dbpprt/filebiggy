using System.Collections.Generic;
using FileBiggy.Bson;
using FileBiggy.Json;
using FileBiggy.Memory;

namespace FileBiggy.Factory
{
    public partial class ContextBuilder<T>
    {
        public readonly Dictionary<string, string> Tuples;

        public ContextBuilder()
        {
            Tuples = new Dictionary<string, string>();    
        }
    }

    public static class ContextBuilderExtensions
    {
        public static JsonContextBuilder<T> AsJsonDatabase<T>(this ContextBuilder<T> contextBuilder)
        {
            contextBuilder.Tuples.Add(ConnectionStringConstants.Provider, typeof (JsonStore<>).FullName);
            return new JsonContextBuilder<T>(contextBuilder.Tuples);
        }

        public static BsonContextBuilder<T> AsBsonDatabase<T>(this ContextBuilder<T> contextBuilder)
        {
            contextBuilder.Tuples.Add(ConnectionStringConstants.Provider, typeof(BsonStore<>).FullName);
            return new BsonContextBuilder<T>(contextBuilder.Tuples);
        }

        public static Builder<T> AsInMemoryDatabase<T>(this ContextBuilder<T> contextBuilder)
        {
            contextBuilder.Tuples.Add(ConnectionStringConstants.Provider, typeof(MemoryStore<>).FullName);
            return new Builder<T>(contextBuilder.Tuples);
        }
    }
}