using System;
using System.Globalization;
using System.Windows;
using Karambolo.ReactiveMvvm;
using Karambolo.ReactiveMvvm.Binding;

namespace NugetSearchDemo.Converters
{
    class BooleanToVisibilityConverter : BindingConverter<bool, Visibility>
    {
        protected override bool TryConvertValue(bool value, object parameter, CultureInfo culture, out Visibility result)
        {
            throw new NotImplementedException();
        }

        public override bool TryConvert(ObservedValue<bool> value, object parameter, CultureInfo culture, out ObservedValue<Visibility> result)
        {
            if (!value.IsAvailable)
                result = Visibility.Collapsed;
            else
                result = value.Value ? Visibility.Visible : Visibility.Hidden;

            return true;
        }
    }
}
