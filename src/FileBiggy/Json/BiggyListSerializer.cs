using System;
using System.Collections;
using Newtonsoft.Json;

namespace FileBiggy.Json
{
    internal class BiggyListSerializer : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        // Custom Biggylist serialization which simply writes each object, separated by newlines, to the output
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var list = value as IEnumerable;

            // Loop over all items in the list
            foreach (var item in list)
            {
                // Serialize the object to the writer
                serializer.Serialize(writer, item);

                // Separate with newline characters
                writer.WriteRaw(Environment.NewLine);
            }
        }

        
    }
}