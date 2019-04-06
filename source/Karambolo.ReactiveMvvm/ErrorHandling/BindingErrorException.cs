using System;

namespace Karambolo.ReactiveMvvm.ErrorHandling
{
    public class BindingErrorException : ObservedErrorException
    {
        internal BindingErrorException(object sourceObject, ReactiveBindingMode bindingMode, Exception exception)
            : base(sourceObject, exception)
        {
            BindingMode = bindingMode;
        }

        public ReactiveBindingMode BindingMode { get; }
    }
}
