using System;
using Karambolo.ReactiveMvvm.Binding;
using Karambolo.ReactiveMvvm.Expressions;

namespace Karambolo.ReactiveMvvm.ErrorHandling
{
    public sealed class ObservableSourceBindingErrorException : BindingErrorException
    {
        internal static ObservableSourceBindingErrorException Create<TSource>(IObservable<TSource> source, object targetRoot, DataMemberAccessChain targetAccessChain, Exception exception)
        {
            return new ObservableSourceBindingErrorException(source, targetRoot, targetAccessChain, exception);
        }

        ObservableSourceBindingErrorException(object sourceObject, object targetRoot, DataMemberAccessChain targetAccessChain, Exception exception)
            : base(sourceObject, ReactiveBindingMode.OneWay, exception)
        {
            TargetRoot = targetRoot;
            TargetAccessChain = targetAccessChain;
        }

        public object TargetRoot { get; }
        public DataMemberAccessChain TargetAccessChain { get; }
    }
}
