using System;
using System.Reactive.Linq;
using Karambolo.ReactiveMvvm.ErrorHandling.Internal;
using Karambolo.ReactiveMvvm.Properties;

namespace Karambolo.ReactiveMvvm.ErrorHandling
{
    public class ObservedErrorHandler
    {
        public static readonly ObservedErrorHandler Default = new ObservedErrorHandler();

        protected ObservedErrorHandler() { }

        public virtual IObservable<TResult> Filter<TResult>(ObservedErrorException exception)
        {
            return Observable.Throw<TResult>(exception);
        }

        public virtual void Handle(Exception exception)
        {
            throw new UnhandledErrorException(string.Format(Resources.UnhandledObservedError, nameof(ObservedErrorHandler)), exception);
        }

        public ObservedErrorHandler Derive<T>(ObservedErrorFilter<T> filter)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            return new DerivedObservedErrorHandler<T>(this, filter, null);
        }

        public ObservedErrorHandler Derive(Action<Exception> handler)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            return new DerivedObservedErrorHandler<object>(this, null, handler);
        }

        public ObservedErrorHandler Derive<T>(ObservedErrorFilter<T> filter, Action<Exception> handler)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            return new DerivedObservedErrorHandler<T>(this, filter, handler);
        }
    }
}
