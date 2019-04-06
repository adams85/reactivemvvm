using System;

namespace Karambolo.ReactiveMvvm.Internal
{
    internal static class ReflectionHelper
    {
        public const string LengthPropertyName = "Length";
        public const string IndexerPropertyName = "Item";
        public const string IndexerGetMethodName = "get_" + IndexerPropertyName;

        public static Type GetItemType(this Type type, params Type[] argTypes)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return type.GetProperty(IndexerPropertyName, argTypes)?.PropertyType;
        }
    }
}
