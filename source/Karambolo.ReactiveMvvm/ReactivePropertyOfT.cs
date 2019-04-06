using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using Karambolo.Common;
using Karambolo.ReactiveMvvm.ErrorHandling;

namespace Karambolo.ReactiveMvvm
{
    public sealed class ReactiveProperty<T> : IObservedErrorSource, IDisposable
    {
        private static readonly IDisposable s_notSubscribed = Disposable.Create(Noop.Action);
        private static readonly IDisposable s_subscribing = Disposable.Create(Noop.Action);
        private readonly IObservable<T> _source;
        private readonly Action<T> _onChanging;
        private readonly Action<T> _onChanged;
        private readonly IScheduler _scheduler;
        private readonly IEqualityComparer<T> _comparer;
        private readonly ReactivePropertyOptions _options;
        private bool _hasChanged;
        private T _currentValue;
        private IDisposable _subscription;

        public ReactiveProperty(IObservable<T> source, T initialValue = default, Action<T> onChanging = null, Action<T> onChanged = null,
            ReactivePropertyOptions options = ReactivePropertyOptions.Default, IScheduler scheduler = null, ObservedErrorHandler errorHandler = null, IEqualityComparer<T> comparer = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            ErrorHandler = errorHandler ?? ObservedErrorHandler.Default;

            _currentValue = initialValue;

            _onChanging = onChanging ?? Noop<T>.Action;
            _onChanged = onChanged ?? Noop<T>.Action;

            _comparer = comparer ?? EqualityComparer<T>.Default;
            _scheduler = scheduler;
            _options = options;

            _source = CreateSource(source, initialValue);

            Volatile.Write(ref _subscription, !HasOptions(ReactivePropertyOptions.DeferSubscription) ? Subscribe() : s_notSubscribed);
        }

        private IObservable<T> CreateSource(IObservable<T> source, T initialValue)
        {
            source = source.Catch<T, Exception>(ex => ErrorHandler.Filter<T>(PropertySourceErrorException.Create(this, ex)));

            if (!HasOptions(ReactivePropertyOptions.SkipInitial))
                source = source.StartWith(initialValue);

            return source;
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref _subscription, null)?.Dispose();
        }

        public ObservedErrorHandler ErrorHandler { get; }

        public T Value
        {
            get
            {
                // ensuring that only one thread can trigger the subscription
                if (Interlocked.CompareExchange(ref _subscription, s_subscribing, s_notSubscribed) == s_notSubscribed)
                {
                    IDisposable subscription = Subscribe();

                    // on the very unlikely occasion when dispose happens during subscribing,
                    // our best effort is to dispose the subscription immediately,
                    // however, by doing so some notifications may be pushed after disposal;
                    // this is acceptable in exchange for lock-free performance
                    // (even in Rx this kind of disposal-notification race condition exists without explicit synchronization)
                    if (Interlocked.CompareExchange(ref _subscription, subscription, s_subscribing) != s_subscribing)
                        subscription.Dispose();
                }

                return _currentValue;
            }
        }

        public bool IsSubscribed
        {
            get
            {
                IDisposable subscription = Volatile.Read(ref _subscription);
                return subscription != s_notSubscribed && subscription != s_subscribing && _subscription != null;
            }
        }

        private IDisposable Subscribe()
        {
            return _source
                .ObserveOnSafe(_scheduler)
                .Subscribe(OnNext, OnError);
        }

        private void OnError(Exception exception)
        {
            ErrorHandler.Handle(exception);
        }

        private void OnNext(T value)
        {
            if (!_hasChanged || !_comparer.Equals(_currentValue, value))
            {
                _hasChanged = true;

                _onChanging(value);
                _currentValue = value;
                _onChanged(value);
            }
        }

        private bool HasOptions(ReactivePropertyOptions options)
        {
            return (_options & options) == options;
        }
    }
}
