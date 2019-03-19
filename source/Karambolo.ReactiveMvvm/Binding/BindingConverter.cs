using System;
using System.Globalization;
using Karambolo.Common;

namespace Karambolo.ReactiveMvvm.Binding
{
    public abstract class BindingConverter<TFrom, TTo> : IBindingConverter<TFrom, TTo>
    {
        protected BindingConverter() { }

        public virtual bool CanConvert(Type fromType, Type toType)
        {
            return typeof(TFrom).IsAssignableFrom(fromType) && toType.IsAssignableFrom(typeof(TTo));
        }

        protected abstract bool TryConvertValue(TFrom value, object parameter, CultureInfo culture, out TTo result);

        public bool TryConvert(ObservedValue<object> value, Type toType, object parameter, CultureInfo culture, out ObservedValue<object> result)
        {
            if ((!value.IsAvailable || typeof(TFrom).IsAssignableFrom(value.Value)) &&
                TryConvert(value.Cast<TFrom>(), parameter, culture, out var resultValue))
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

        public virtual bool TryConvert(ObservedValue<TFrom> value, object parameter, CultureInfo culture, out ObservedValue<TTo> result)
        {
            if (!value.IsAvailable)
            {
                result = ObservedValue.None;
            }
            else if (TryConvertValue(value.Value, parameter, culture, out var convertedValue))
            {
                result = convertedValue;
            }
            else
            {
                result = default;
                return false;
            }

            return true;
        }
    }
}
