using System;
using Karambolo.ReactiveMvvm.Expressions;

namespace Karambolo.ReactiveMvvm.Binding
{
    public sealed class ObservableSourceBinding<TSource, TTargetRoot, TTarget> : ReactiveBinding<TSource, TTarget, IDataBindingEvent<TSource, TTarget>>
    {
        internal ObservableSourceBinding(IObservable<TSource> source, TTargetRoot targetRoot, DataMemberAccessChain targetAccessChain,
            IObservable<IDataBindingEvent<TSource, TTarget>> whenBind, IDisposable bindingDisposable) :
            base(ReactiveBindingMode.OneWay, whenBind, bindingDisposable)
        {
            Source = source;
            TargetRoot = targetRoot;
            TargetAccessChain = targetAccessChain;
        }

        public IObservable<TSource> Source { get; }

        public TTargetRoot TargetRoot { get; }

        public DataMemberAccessChain TargetAccessChain { get; }
    }
}
