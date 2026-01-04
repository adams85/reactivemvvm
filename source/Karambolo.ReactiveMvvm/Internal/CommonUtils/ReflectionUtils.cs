using System;

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
    }
}
