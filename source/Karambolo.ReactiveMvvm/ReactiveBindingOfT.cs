using System;
using System.Threading;

namespace Karambolo.ReactiveMvvm
{
    public abstract class ReactiveBinding<TSource, TTarget, TEvent> : IDisposable
        where TEvent : IReactiveBindingEvent<TSource, TTarget>
    {
        IDisposable _bindingDisposable;

        internal ReactiveBinding(ReactiveBindingMode bindingMode, IObservable<TEvent> whenBind, IDisposable bindingDisposable)
        {
            BindingMode = bindingMode;
            WhenBind = whenBind;

            Volatile.Write(ref _bindingDisposable, bindingDisposable);
        }

        public ReactiveBindingMode BindingMode { get; }

        public IObservable<TEvent> WhenBind { get; }

        public void Dispose()
        {
            Interlocked.Exchange(ref _bindingDisposable, null)?.Dispose();
        }
    }
}
