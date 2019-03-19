using System;
using System.Globalization;
using Karambolo.ReactiveMvvm;
using Karambolo.ReactiveMvvm.Binding;

namespace GettingStarted.Helpers
{
    class SafeTimeConverter : BindingConverter<DateTimeOffset, string>
    {
        protected override bool TryConvertValue(DateTimeOffset value, object parameter, CultureInfo culture, out string result)
        {
            throw new InvalidOperationException();
        }

        public override bool TryConvert(ObservedValue<DateTimeOffset> value, object parameter, CultureInfo culture, out ObservedValue<string> result)
        {
            result = value.IsAvailable ? value.Value.ToLocalTime().TimeOfDay.ToString("hh':'mm':'ss") : string.Empty;
            return true;
        }
    }
}
