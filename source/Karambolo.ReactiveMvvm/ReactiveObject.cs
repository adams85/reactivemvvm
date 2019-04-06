using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using Karambolo.ReactiveMvvm.ChangeNotification;
using Karambolo.ReactiveMvvm.ErrorHandling;

namespace Karambolo.ReactiveMvvm
{
    public abstract class ReactiveObject : ChangeNotifier, IObservedErrorSource, ILifetime
    {
        private readonly CompositeDisposable _disposables;
        private int _suppressChangeNotificationsFlag;

        protected ReactiveObject() : this(null) { }

        protected ReactiveObject(ObservedErrorHandler errorHandler)
        {
            ErrorHandler = errorHandler ?? ObservedErrorHandler.Default;

            WhenChanging = CreateWhenChanging();
            WhenChanged = CreateWhenChanged();

            _disposables = new CompositeDisposable();
        }

        private IObservable<EventPattern<PropertyChangingEventArgs>> CreateWhenChanging()
        {
            return Observable.FromEventPattern<PropertyChangingEventHandler, PropertyChangingEventArgs>(handler => _propertyChanging += handler, handler => _propertyChanging -= handler);
        }

        private IObservable<EventPattern<PropertyChangedEventArgs>> CreateWhenChanged()
        {
            return Observable.FromEventPattern<PropertyChangedEventHandler, PropertyChangedEventArgs>(handler => _propertyChanged += handler, handler => _propertyChanged -= handler);
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public void AttachDisposable(IDisposable disposable)
        {
            _disposables.Add(disposable);
        }

        public void DetachDisposable(IDisposable disposable)
        {
            _disposables.Remove(disposable);
        }

        public ObservedErrorHandler ErrorHandler { get; }

        public IObservable<EventPattern<PropertyChangingEventArgs>> WhenChanging { get; }
        public IObservable<EventPattern<PropertyChangedEventArgs>> WhenChanged { get; }

        protected virtual void OnPropertyChangingCore(PropertyChangingEventArgs args)
        {
            base.OnPropertyChanging(args);
        }

        protected sealed override void OnPropertyChanging(PropertyChangingEventArgs args)
        {
            if (Volatile.Read(ref _suppressChangeNotificationsFlag) == 0)
                OnPropertyChangingCore(args);
        }

        protected virtual void OnPropertyChangedCore(PropertyChangedEventArgs args)
        {
            base.OnPropertyChanged(args);
        }

        protected sealed override void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            if (Volatile.Read(ref _suppressChangeNotificationsFlag) == 0)
                OnPropertyChangedCore(args);
        }

        public IDisposable SuppressChangeNotifications()
        {
            Interlocked.Increment(ref _suppressChangeNotificationsFlag);
            return Disposable.Create(() => Interlocked.Decrement(ref _suppressChangeNotificationsFlag));
        }
    }
}
