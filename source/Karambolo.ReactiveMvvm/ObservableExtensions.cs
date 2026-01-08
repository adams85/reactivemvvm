using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Karambolo.ReactiveMvvm
{
    public static class ObservableExtensions
    {
        #region Scheduling

        public static IObservable<T> SubscribeOnIfNotNull<T>(this IObservable<T> observable, IScheduler scheduler)
        {
            if (observable == null)
                throw new ArgumentNullException(nameof(observable));

            return scheduler != null ? observable.SubscribeOn(scheduler) : observable;
        }

        public static IObservable<T> SubscribeOnIfNotNull<T>(this IObservable<T> observable, SynchronizationContext synchronizationContext)
        {
            if (observable == null)
                throw new ArgumentNullException(nameof(observable));

            return synchronizationContext != null ? observable.SubscribeOn(synchronizationContext) : observable;
        }

        public static IObservable<T> ObserveOnIfNotNull<T>(this IObservable<T> observable, IScheduler scheduler)
        {
            if (observable == null)
                throw new ArgumentNullException(nameof(observable));

            return scheduler != null ? observable.ObserveOn(scheduler) : observable;
        }

        public static IObservable<T> ObserveOnIfNotNull<T>(this IObservable<T> observable, SynchronizationContext synchronizationContext)
        {
            if (observable == null)
                throw new ArgumentNullException(nameof(observable));

            return synchronizationContext != null ? observable.ObserveOn(synchronizationContext) : observable;
        }

        #endregion

        #region Logging

        public static IObservable<T> Log<T>(this IObservable<T> observable, ILogger logger, Func<T, string> formatValue = null, string errorMessage = null)
        {
            if (observable == null)
                throw new ArgumentNullException(nameof(observable));

            if (formatValue == null)
                formatValue = value => value.ToString();

            return observable.Do(
                 value => logger.LogInformation("OnNext: " + formatValue(value)),
                 ex => logger.LogError(ex, "OnError: " + errorMessage ?? ex.Message),
                 () => logger.LogInformation("OnCompleted"));
        }

        public static IObservable<T> CatchWithLogging<T>(this IObservable<T> observable, ILogger logger, IObservable<T> continuation = null, string errorMessage = null)
        {
            if (observable == null)
                throw new ArgumentNullException(nameof(observable));

            return observable.Catch<T, Exception>(ex =>
            {
                logger.LogError(ex, errorMessage ?? ex.Message);
                return continuation;
            });
        }

        public static IObservable<T> CatchWithLogging<T, TException>(this IObservable<T> observable, ILogger logger, Func<TException, IObservable<T>> getContinuation, string errorMessage = null)
            where TException : Exception
        {
            return observable.Catch<T, TException>(ex =>
            {
                logger.LogError(ex, errorMessage ?? ex.Message);
                return getContinuation(ex);
            });
        }

        #endregion
    }
}
