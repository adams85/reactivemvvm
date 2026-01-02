using System;
using Karambolo.ReactiveMvvm.Properties;

namespace Karambolo.Common
{
    internal static class ReflectionUtils
    {
        public static bool AllowsNull(this Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
        }

        public static bool IsAssignableFrom(this Type type, object obj)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return obj != null ? type.IsAssignableFrom(obj.GetType()) : type.AllowsNull();
        }

        public static Type GetInterface(this Type type, Type interfaceType)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (interfaceType == null)
                throw new ArgumentNullException(nameof(interfaceType));

            if (!interfaceType.IsInterface)
                throw new ArgumentException(CommonUtilsResources.NotInterfaceType, nameof(interfaceType));

            return Array.Find(type.GetInterfaces(), t => t == interfaceType);
        }

        public static bool HasInterface(this Type type, Type interfaceType)
        {
            return type.GetInterface(interfaceType) != null;
        }
    }
}
