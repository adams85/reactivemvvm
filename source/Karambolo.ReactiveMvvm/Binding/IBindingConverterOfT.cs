using System.Globalization;

namespace Karambolo.ReactiveMvvm.Binding
{
    public interface IBindingConverter<TFrom, TTo> : IBindingConverter
    {
        bool TryConvert(ObservedValue<TFrom> value, object parameter, CultureInfo culture, out ObservedValue<TTo> result);
    }
}
