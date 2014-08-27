using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FileBiggy.Common;
using FileBiggy.Contracts;
using FileBiggy.Exceptions;

namespace FileBiggy
{
    public class BiggyContext
    {
        private readonly Dictionary<Type, object> _typeStores;

        public string ConnectionString { get; private set; }

        public Type UnderlayingStore { get; private set; }

        public BiggyContext(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            ConnectionString = connectionString;

            var segments = connectionString.Split(ConnectionStringConstants.TupleSeperator);
            var tuples = segments
                .Select(segment => segment.Split(ConnectionStringConstants.SegmentSeperator))
                .ToDictionary(parts => parts.First().ToLowerInvariant().Trim(), parts => parts.Last().Trim());

            string provider;
            if (!tuples.TryGetValue(ConnectionStringConstants.Provider, out provider))
            {
                throw new InvalidConnectionStringException(
                    String.Format("ConnectionString must provide a valid {0} segment",
                        ConnectionStringConstants.Provider),
                    connectionString);
            }

            Type providerType;
            TypeHelper.TryGetTypeByName(provider, out providerType);

            if (providerType == null)
            {
                throw new InvalidProviderException("The requested provider was not found in any loaded assembly",
                    provider);
            }

            UnderlayingStore = providerType;

            _typeStores = new Dictionary<Type, object>();
            var props = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var propertyInfo in props)
            {
                var propertyType = propertyInfo.PropertyType;
                var genericTypeArgument = propertyType.GenericTypeArguments.FirstOrDefault();

                if (genericTypeArgument == null)
                {
                    continue;
                }

                var genericInterface = typeof (IEntitySet<>).MakeGenericType(genericTypeArgument);

                // okay lets check wether the current property is an IEntitySet<T>
                if (genericInterface.IsAssignableFrom(propertyType))
                {
                    // create a instance and create the store for the given
                    // inner generic type
                    var targetType = providerType.MakeGenericType(genericTypeArgument);
                    // creating our store instance and pass in the connection string dictionary
                    var storeInstance = Activator.CreateInstance(targetType, tuples);
                    var biggyInstance = Activator.CreateInstance(propertyType, storeInstance);

                    // we cache our instances for each entity type to allow Set<T>()
                    try
                    {
                        _typeStores.Add(genericTypeArgument, biggyInstance);
                    }
                    catch (ArgumentException)
                    {
                        throw new EntityTypeException("Only one entity type per context is allowed.",
                            genericTypeArgument.FullName);
                    }

                    propertyInfo.SetValue(this, biggyInstance);
                }
            }
        }

        public IEntitySet<T> Set<T>()
        {
            return Set(typeof (T)) as IEntitySet<T>;
        }

        public object Set(Type entityType)
        {
            object result;
            _typeStores.TryGetValue(entityType, out result);
            return result;
        }
    }
}