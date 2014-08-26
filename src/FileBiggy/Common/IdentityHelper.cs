using System;
using System.Linq;
using System.Reflection;
using FileBiggy.Attributes;
using FileBiggy.Exceptions;

namespace FileBiggy.Common
{
    public static class IdentityHelper
    {
        public static object GetKeyFromEntity(this object obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            var attributes = obj.GetType().GetProperties();

            PropertyInfo identityProperty;
            try
            {
                identityProperty = attributes
                    .GroupBy(attr => attr.GetCustomAttribute<IdentityAttribute>())
                    .Where(group => group != null && group.Key != null)
                    .Where(prop => prop.Any())
                    .Select(prop => prop.SingleOrDefault())
                    .SingleOrDefault();
            }
            catch (InvalidOperationException)
            {
                throw new IdentityAttributeMismatchException("You must only specify one primary key per entity");
            }
            
            if (identityProperty == null)
            {
                return null;
            }

            var value = identityProperty.GetValue(obj);
            return value;
        }
    }
}
