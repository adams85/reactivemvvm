using System;
using System.Globalization;

namespace Karambolo.ReactiveMvvm.Binding
{
    public interface IBindingConverter
    {
        bool CanConvert(Type fromType, Type toType);
        bool TryConvert(ObservedValue<object> value, Type toType, object parameter, CultureInfo culture, out ObservedValue<object> result);
    }
}
