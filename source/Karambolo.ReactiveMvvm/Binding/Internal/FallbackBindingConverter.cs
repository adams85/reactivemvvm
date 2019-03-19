using System;
using System.ComponentModel;
using System.Globalization;

namespace Karambolo.ReactiveMvvm.Binding.Internal
{
    public class FallbackBindingConverter<TFrom, TTo> : BindingConverter<TFrom, TTo>
    {
        public static readonly FallbackBindingConverter<TFrom, TTo> Instance = new FallbackBindingConverter<TFrom, TTo>();

        readonly TypeConverter _typeConverter;
        readonly Func<TFrom, CultureInfo, TTo> _converter;

        private FallbackBindingConverter()
        {
            if (typeof(TTo).IsAssignableFrom(typeof(TFrom)))
                _converter = (value, culture) => (TTo)(object)value;
            else if ((_typeConverter = TypeDescriptor.GetConverter(typeof(TTo))).CanConvertFrom(typeof(TFrom)))
                _converter = (value, culture) => (TTo)_typeConverter.ConvertFrom(null, culture, value);
            else if ((_typeConverter = TypeDescriptor.GetConverter(typeof(TFrom))).CanConvertTo(typeof(TTo)))
                _converter = (value, culture) => (TTo)_typeConverter.ConvertTo(null, culture, value, typeof(TTo));
            else
            {
                _typeConverter = null;
                _converter = (value, culture) => (TTo)System.Convert.ChangeType(value, typeof(TTo), culture);
            }
        }

        public override bool CanConvert(Type fromType, Type toType)
        {
            return true;
        }

        protected override bool TryConvertValue(TFrom value, object parameter, CultureInfo culture, out TTo result)
        {
            try
            {
                result = _converter(value, culture);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }
    }
}
