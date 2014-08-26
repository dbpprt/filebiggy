using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using FileBiggy.Common;
using FileBiggy.Contracts;
using FileBiggy.Exceptions;

namespace FileBiggy
{
    public class BiggyContext
    {
        private const string ProviderIdentifier = "provider";
        private readonly Dictionary<Type, object> _typeStores; 

        public class Obj
        {
            public string Test;
        }

        public BiggyList<Obj> Tests { get; set; }

        public BiggyContext(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            var segments = connectionString.Split(';');
            var tuples = segments
                .Select(segment => segment.Split('='))
                .ToDictionary(parts => parts.First().ToLowerInvariant().Trim(), parts => parts.Last().Trim());
            
            string provider;
            if (!tuples.TryGetValue(ProviderIdentifier, out provider))
            {
                throw new InvalidConnectionStringException(
                    String.Format("ConnectionString must provide a valid {0} segment", ProviderIdentifier), 
                    connectionString);
            }

            Type providerType;
            TypeHelper.TryGetTypeByName(provider, out providerType);

            if (providerType == null)
            {
                throw new InvalidProviderException("The requested provider was not found in any loaded assembly", provider);
            }

            _typeStores = new Dictionary<Type, object>();
            var props = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var propertyInfo in props)
            {
                var propertyType = propertyInfo.PropertyType;
                var genericTypeArgument = propertyType.GenericTypeArguments.Single();
                var genericInterface = typeof (IBiggy<>).MakeGenericType(genericTypeArgument);

                // okay lets check wether the current property is an IBiggy<T>
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
                        throw new EntityTypeException("Only one entity type per context is allowed.", genericTypeArgument.FullName);
                    }

                    propertyInfo.SetValue(this, biggyInstance);
                }
            }
        }

        public IBiggy<T> Set<T>()
        {
            return Set(typeof (T)) as IBiggy<T>;
        }

        public object Set(Type entityType)
        {
            object result;
            _typeStores.TryGetValue(entityType, out result);
            return result;
        } 
    }
}
