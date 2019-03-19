using System;
using System.Windows.Input;
using Karambolo.ReactiveMvvm.Expressions;

namespace Karambolo.ReactiveMvvm.Binding
{
    public sealed class CommandBinding<TViewModel, TCommand, TView, TContainer, TParam> : ReactiveBinding<TCommand, TContainer, ICommandBindingEvent<TCommand, TContainer>>
        where TView : IBoundView<TViewModel>
        where TViewModel : class
        where TCommand : ICommand
    {
        internal CommandBinding(TView view, DataMemberAccessChain commandAccessChain, DataMemberAccessChain containerAccessChain, IObservable<TParam> commandParameters,
            IObservable<ICommandBindingEvent<TCommand, TContainer>> whenBind, IDisposable bindingDisposable) :
            base(ReactiveBindingMode.OneWay, whenBind, bindingDisposable)
        {
            View = view;
            ViewModelAccessChain = commandAccessChain;
            ViewAccessChain = containerAccessChain;
            CommandParameters = commandParameters;
        }

        public TViewModel ViewModel => View.ViewModel;

        public DataMemberAccessChain ViewModelAccessChain { get; }

        public TView View { get; }

        public DataMemberAccessChain ViewAccessChain { get; }

        public IObservable<TParam> CommandParameters { get; }
    }
}
