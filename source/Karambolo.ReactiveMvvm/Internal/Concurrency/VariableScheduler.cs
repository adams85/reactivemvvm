using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;

namespace Karambolo.ReactiveMvvm.Internal.Concurrency
{
    public class VariableScheduler : IScheduler
    {
        private IScheduler _scheduler;
        private IDisposable _subscription;

        public VariableScheduler(IObservable<IScheduler> schedulers, IScheduler defaultScheduler = null)
        {
            Volatile.Write(ref _scheduler, defaultScheduler ?? ImmediateScheduler.Instance);

            Volatile.Write(ref _subscription, schedulers.Finally(Freeze).Subscribe(scheduler => Volatile.Write(ref _scheduler, scheduler)));
        }

        public void Freeze()
        {
            Interlocked.Exchange(ref _subscription, Disposable.Empty).Dispose();
        }

        public IScheduler Current => Volatile.Read(ref _scheduler);

        public IDisposable Schedule<TState>(TState state, Func<IScheduler, TState, IDisposable> action)
        {
            return Current.Schedule(state, action);
        }

        public IDisposable Schedule<TState>(TState state, TimeSpan dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            return Current.Schedule(state, dueTime, action);
        }

        public IDisposable Schedule<TState>(TState state, DateTimeOffset dueTime, Func<IScheduler, TState, IDisposable> action)
        {
            return Current.Schedule(state, dueTime, action);
        }

        public DateTimeOffset Now
        {
            get { return Current.Now; }
        }
    }
}
