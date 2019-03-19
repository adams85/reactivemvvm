using System;
using System.Globalization;
using Karambolo.Common;

namespace Karambolo.ReactiveMvvm.Binding.Internal
{
    public class NullBindingConverter<T> : IBindingConverter<T, T>
    {
        public static readonly NullBindingConverter<T> Instance = new NullBindingConverter<T>();

        private NullBindingConverter() { }

        public bool CanConvert(Type fromType, Type toType)
        {
            return typeof(T).IsAssignableFrom(fromType) && toType.IsAssignableFrom(typeof(T));
        }

        public bool TryConvert(ObservedValue<object> value, Type toType, object parameter, CultureInfo culture, out ObservedValue<object> result)
        {
            if ((!value.IsAvailable || typeof(T).IsAssignableFrom(value.Value)) &&
                TryConvert(value.Cast<T>(), parameter, culture, out var resultValue))
            {
                result = resultValue.Cast<object>();
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        public bool TryConvert(ObservedValue<T> value, object parameter, CultureInfo culture, out ObservedValue<T> result)
        {
            result = value;
            return true;
        }
    }
}
