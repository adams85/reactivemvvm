using System;
using Karambolo.ReactiveMvvm.Expressions;

namespace Karambolo.ReactiveMvvm.Binding
{
    public sealed class ViewModelToViewBinding<TViewModel, TViewModelValue, TView, TViewValue> : ReactiveBinding<TViewModelValue, TViewValue, IDataBindingEvent<TViewModelValue, TViewValue>>
        where TViewModel : class
        where TView : IBoundView<TViewModel>
    {
        internal ViewModelToViewBinding(TView view, DataMemberAccessChain viewModelAccessChain, DataMemberAccessChain viewAccessChain, ReactiveBindingMode bindingMode,
            IObservable<IDataBindingEvent<TViewModelValue, TViewValue>> whenBind, IDisposable bindingDisposable) :
            base(bindingMode, whenBind, bindingDisposable)
        {
            View = view;
            ViewModelAccessChain = viewModelAccessChain;
            ViewAccessChain = viewAccessChain;
        }

        public TViewModel ViewModel => View.ViewModel;

        public DataMemberAccessChain ViewModelAccessChain { get; }

        public TView View { get; }

        public DataMemberAccessChain ViewAccessChain { get; }
    }
}
