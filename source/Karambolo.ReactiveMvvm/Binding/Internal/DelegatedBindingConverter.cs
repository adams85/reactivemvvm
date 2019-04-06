using System;
using System.Globalization;

namespace Karambolo.ReactiveMvvm.Binding.Internal
{
    public class DelegatedBindingConverter<TFrom, TTo> : BindingConverter<TFrom, TTo>
    {
        private readonly Func<TFrom, TTo> _converter;

        public DelegatedBindingConverter(Func<TFrom, TTo> converter)
        {
            _converter = converter;
        }

        protected override bool TryConvertValue(TFrom value, object parameter, CultureInfo culture, out TTo result)
        {
            try
            {
                result = _converter(value);
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
