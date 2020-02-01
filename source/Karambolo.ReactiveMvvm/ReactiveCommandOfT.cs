using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Karambolo.Common;
using Karambolo.ReactiveMvvm.ErrorHandling;
using Karambolo.ReactiveMvvm.Properties;

namespace Karambolo.ReactiveMvvm
{
    public class ReactiveCommand<TParam, TResult> : ReactiveCommandBase, IObservable<TResult>, IDisposable
    {
        protected class Initializer
        {
            public static readonly Initializer Default = new Initializer();

            protected Initializer() { }

            public virtual Func<TParam, CancellationToken, Task<TResult>> GetExecute(Func<TParam, CancellationToken, Task<TResult>> execute, Func<Exception, IObservable<TResult>> errorFilter)
            {
                return async (param, ct) =>
                {
                    try { return await execute(param, ct).ConfigureAwait(false); }
                    catch (Exception ex) when (!(ex is OperationCanceledException)) { return await errorFilter(ex); }
                };
            }

            public virtual IObservable<bool> GetCanExecute(IObservable<bool> canExecute, Func<Exception, IObservable<bool>> errorFilter)
            {
                return canExecute != null ? canExecute.Catch(errorFilter) : Internal.True.Observable;
            }
        }

        private static readonly bool s_paramCanBeNull = typeof(TParam) == typeof(Unit) || typeof(TParam).AllowsNull();
        private readonly Func<TParam, CancellationToken, Task<TResult>> _execute;
        private readonly Subject<ExecutionEvent<TResult>> _executionEventSubject;
        private readonly ISubject<ExecutionEvent<TResult>> _executionEventSubjectSync;
        private readonly IObservable<TResult> _results;
        private readonly IDisposable _onCanExecuteChangedSubscription;

        protected ReactiveCommand(Initializer initializer,
            Func<TParam, CancellationToken, Task<TResult>> execute, IObservable<bool> canExecute = null,
            IScheduler scheduler = null, ObservedErrorHandler errorHandler = null)
        {
            ErrorHandler = errorHandler ?? ObservedErrorHandler.Default;

            _execute = initializer.GetExecute(execute,
                execute != null ? ex => ErrorHandler.Filter<TResult>(CommandExecuteErrorException.Create(this, ex)) : (Func<Exception, IObservable<TResult>>)null);

            canExecute = initializer.GetCanExecute(canExecute,
                canExecute != null ? ex => ErrorHandler.Filter<bool>(CommandCanExecuteErrorException.Create(this, ex)) : (Func<Exception, IObservable<bool>>)null);

            _executionEventSubject = new Subject<ExecutionEvent<TResult>>();
            _executionEventSubjectSync = Subject.Synchronize(_executionEventSubject);

            _results = CreateResults();

            WhenIsBusyChanged = CreateWhenIsBusyChanged();

            WhenCanExecuteChanged = CreateWhenCanExecuteChanged(canExecute);

            _onCanExecuteChangedSubscription = WhenCanExecuteChanged
                .ObserveOnSafe(scheduler)
                .Subscribe(_ => OnCanExecuteChanged(), OnError);
        }

        public ReactiveCommand(Func<TParam, CancellationToken, Task<TResult>> execute, IObservable<bool> canExecute = null,
            IScheduler scheduler = null, ObservedErrorHandler errorHandler = null)
            : this(Initializer.Default, execute ?? throw new ArgumentNullException(nameof(execute)), canExecute, scheduler, errorHandler) { }

        private IObservable<TResult> CreateResults()
        {
            return _executionEventSubjectSync
                .Where(info => info.Kind == ExecutionEventKind.Result)
                .Select(info => info.Value);
        }

        private IObservable<bool> CreateWhenIsBusyChanged()
        {
            return _executionEventSubjectSync
                .Scan(0, (busyCount, info) =>
                    info.Kind == ExecutionEventKind.Begin ? busyCount + 1 :
                    info.Kind == ExecutionEventKind.End ? busyCount - 1 :
                    busyCount)
                .Select(busyCount => busyCount > 0)
                .StartWith(false)
                .DistinctUntilChanged()
                .Replay(1)
                .RefCount();
        }

        private IObservable<bool> CreateWhenCanExecuteChanged(IObservable<bool> canExecute)
        {
            return canExecute
                .StartWith(false)
                .CombineLatest(WhenIsBusyChanged, (canExec, isBusy) => canExec && !isBusy)
                .DistinctUntilChanged()
                .Replay(1)
                .RefCount();
        }

        protected virtual void DisposeCore() { }

        public void Dispose()
        {
            DisposeCore();

            _onCanExecuteChangedSubscription.Dispose();
            _executionEventSubject.Dispose();
        }

        public sealed override ObservedErrorHandler ErrorHandler { get; }

        public sealed override IObservable<bool> WhenCanExecuteChanged { get; }

        public sealed override IObservable<bool> WhenIsBusyChanged { get; }

        private void OnError(Exception exception)
        {
            ErrorHandler.Handle(exception);
        }

        public IDisposable Subscribe(IObserver<TResult> observer)
        {
            return _results.Subscribe(observer);
        }

        public async Task<TResult> ExecuteAsync(TParam parameter, CancellationToken cancellationToken = default)
        {
            _executionEventSubjectSync.OnNext(ExecutionEvent<TResult>.Begin());
            try
            {
                TResult result = await _execute(parameter, cancellationToken);
                _executionEventSubjectSync.OnNext(ExecutionEvent<TResult>.Result(result));
                return result;
            }
            finally { _executionEventSubjectSync.OnNext(ExecutionEvent<TResult>.End()); }
        }

        protected sealed override bool CanExecute(object parameter)
        {
            return WhenCanExecuteChanged.FirstAsync().Wait();
        }

        protected sealed override void Execute(object parameter)
        {
            TParam param;
            // comparing to null involves boxing but that's ok as optimized away by the jitter
            if (parameter == null)
            {
                if (!s_paramCanBeNull)
                    throw new ArgumentException(string.Format(Resources.NonNullableCommandParamType, typeof(TParam)), nameof(parameter));

                param = default;
            }
            else if (parameter is TParam castParam)
                param = castParam;
            else
                throw new ArgumentException(string.Format(Resources.IncompatibleCommandParamType, typeof(TParam), parameter.GetType()), nameof(parameter));

            ExecuteAsync(param).FireAndForget(ex =>
            {
                if (!(ex is OperationCanceledException))
                    OnError(ex);
            });
        }
    }
}
