using System;

namespace Karambolo.ReactiveMvvm.ErrorHandling
{
    public sealed class PropertySourceErrorException : ObservedErrorException
    {
        internal static PropertySourceErrorException Create<T>(ReactiveProperty<T> property, Exception exception)
        {
            return new PropertySourceErrorException(property, exception);
        }

        PropertySourceErrorException(object sourceObject, Exception exception) : base(sourceObject, exception) { }
    }
}
