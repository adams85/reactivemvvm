using System;
using System.Globalization;

namespace Karambolo.ReactiveMvvm.Binding.Internal
{
    public class GenericBindingConverterAdapter<TFrom, TTo> : IBindingConverter<TFrom, TTo>
    {
        private readonly IBindingConverter _converter;

        public GenericBindingConverterAdapter(IBindingConverter converter)
        {
            _converter = converter;
        }

        public bool CanConvert(Type fromType, Type toType)
        {
            return _converter.CanConvert(fromType, toType);
        }

        public bool TryConvert(ObservedValue<object> value, Type toType, object parameter, CultureInfo culture, out ObservedValue<object> result)
        {
            return _converter.TryConvert(value, toType, parameter, culture, out result);
        }

        public bool TryConvert(ObservedValue<TFrom> value, object parameter, CultureInfo culture, out ObservedValue<TTo> result)
        {
            if (TryConvert(value.Cast<object>(), typeof(TTo), parameter, culture, out ObservedValue<object> resultValue))
            {
                result = value.Cast<TTo>();
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }
    }
}
