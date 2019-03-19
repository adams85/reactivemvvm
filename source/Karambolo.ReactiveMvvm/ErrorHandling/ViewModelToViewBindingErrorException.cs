using System;
using Karambolo.ReactiveMvvm.Binding;
using Karambolo.ReactiveMvvm.Expressions;

namespace Karambolo.ReactiveMvvm.ErrorHandling
{
    public sealed class ViewModelToViewBindingErrorException : BindingErrorException
    {
        internal static ViewModelToViewBindingErrorException Create<TViewModel>(IBoundView<TViewModel> view, DataMemberAccessChain viewModelAccessChain, DataMemberAccessChain viewAccessChain,
            ReactiveBindingMode bindingMode, Exception exception)
            where TViewModel : class
        {
            return new ViewModelToViewBindingErrorException(view, viewModelAccessChain, viewAccessChain, bindingMode, exception);
        }

        ViewModelToViewBindingErrorException(object sourceObject, DataMemberAccessChain viewModelAccessChain, DataMemberAccessChain viewAccessChain, ReactiveBindingMode bindingMode, Exception exception)
            : base(sourceObject, bindingMode, exception)
        {
            ViewModelAccessChain = viewModelAccessChain;
            ViewAccessChain = viewAccessChain;
        }

        public DataMemberAccessChain ViewModelAccessChain { get; }
        public DataMemberAccessChain ViewAccessChain { get; }
    }
}
