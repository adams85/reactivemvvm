using System;

namespace Karambolo.ReactiveMvvm.ErrorHandling.Internal
{
    internal sealed class DerivedObservedErrorHandler<T> : ObservedErrorHandler
    {
        private readonly ObservedErrorHandler _base;
        private readonly ObservedErrorFilter<T> _filter;
        private readonly Action<Exception> _handler;

        public DerivedObservedErrorHandler(ObservedErrorHandler @base, ObservedErrorFilter<T> filter, Action<Exception> handler)
        {
            if (@base == null)
                throw new ArgumentNullException(nameof(@base));

            _base = @base;
            _filter = filter;
            _handler = handler;
        }

        public override IObservable<TResult> Filter<TResult>(ObservedErrorException exception)
        {
            return
                (_filter != null && typeof(IObservable<TResult>).IsAssignableFrom(typeof(IObservable<T>)) ? (IObservable<TResult>)_filter(exception) : null) ??
                _base.Filter<TResult>(exception);
        }

        public override void Handle(Exception exception)
        {
            (_handler ?? _base.Handle)(exception);
        }
    }
}
