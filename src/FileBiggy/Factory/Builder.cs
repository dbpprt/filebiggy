using System;
using System.Collections.Generic;
using System.Globalization;

namespace FileBiggy.Factory
{
    public class Builder<T>
    {
        private readonly Dictionary<string, string> _tuples;

        public Builder(Dictionary<string, string> tuples)
        {
            _tuples = tuples;
        }

        public T Build()
        {
            var segments = new List<string>();

            foreach (var tuple in _tuples)
            {
                var segment = String.Format("{0}{1}{2}", tuple.Key, ConnectionStringConstants.SegmentSeperator, tuple.Value);
                segments.Add(segment);
            }

            var connectionString = string.Join(
                ConnectionStringConstants.TupleSeperator.ToString(CultureInfo.InvariantCulture), 
                segments);
            return (T)Activator.CreateInstance(typeof (T), connectionString);
        }
    }
}