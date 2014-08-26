using System.Collections.Generic;
using FileBiggy.Json;
using FileBiggy.Memory;

namespace FileBiggy.Factory
{
    public partial class ContextBuilder<T>
    {
        private readonly Dictionary<string, string> _tuples;

        public ContextBuilder()
        {
            _tuples = new Dictionary<string, string>();    
        } 

        public Builder<T> AsInMemoryDatabase()
        {
            _tuples.Add(ConnectionStringConstants.Provider, typeof(MemoryStore<>).FullName);
            return new Builder<T>(_tuples);
        }

        public JsonContextBuilder<T> AsJsonDatabase()
        {
            _tuples.Add(ConnectionStringConstants.Provider, typeof(JsonStore<>).FullName);
            return new JsonContextBuilder<T>(_tuples);
        } 
    }
}