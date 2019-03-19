using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Threading;
using System.Threading.Tasks;
using Karambolo.ReactiveMvvm.ErrorHandling;

namespace Karambolo.ReactiveMvvm
{
    public static partial class ReactiveCommand
    {
        #region From delegates

        public static ReactiveCommand<Unit, Unit> ToCommandUnattached(this Action execute, IObservable<bool> canExecute = null,
            IScheduler scheduler = null, ObservedErrorHandler errorHandler = null)
        {
            return ToCommandUnattached<Unit>(_ => execute(), canExecute, scheduler, errorHandler);
        }

        public static ReactiveCommand<Unit, Unit> ToCommand<TOwner>(this Action execute, TOwner owner, IObservable<bool> canExecute = null,
            IScheduler scheduler = null, ObservedErrorHandler errorHandler = null)
            where TOwner : IObservedErrorSource, ILifetime
        {
            return ToCommandUnattached(execute, canExecute, scheduler, errorHandler ?? owner.ErrorHandler)
                .AttachTo(owner);
        }

        public static ReactiveCommand<TParam, Unit> ToCommandUnattached<TParam>(this Action<TParam> execute, IObservable<bool> canExecute = null,
            IScheduler scheduler = null, ObservedErrorHandler errorHandler = null)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));

            return new ReactiveCommand<TParam, Unit>(
                (param, _) =>
                {
                    try { execute(param); }
                    catch (Exception ex) { return Task.FromException<Unit>(ex); }
                    return Task.FromResult(Unit.Default);
                },
                canExecute, scheduler ?? ReactiveMvvmContext.Current.MainThreadScheduler, errorHandler ?? ReactiveMvvmContext.Current.DefaultErrorHandler);
        }

        public static ReactiveCommand<TParam, Unit> ToCommand<TOwner, TParam>(this Action<TParam> execute, TOwner owner, IObservable<bool> canExecute = null,
            IScheduler scheduler = null, ObservedErrorHandler errorHandler = null)
            where TOwner : IObservedErrorSource, ILifetime
        {
            return ToCommandUnattached(execute, canExecute, scheduler, errorHandler ?? owner.ErrorHandler)
                .AttachTo(owner);
        }

        public static ReactiveCommand<Unit, TResult> ToCommandUnattached<TResult>(this Func<TResult> execute, IObservable<bool> canExecute = null,
            IScheduler scheduler = null, ObservedErrorHandler errorHandler = null)
        {
            return ToCommandUnattached<Unit, TResult>(_ => execute(), canExecute, scheduler, errorHandler);
        }

        public static ReactiveCommand<Unit, TResult> ToCommand<TOwner, TResult>(this Func<TResult> execute, TOwner owner, IObservable<bool> canExecute = null,
            IScheduler scheduler = null, ObservedErrorHandler errorHandler = null)
            where TOwner : IObservedErrorSource, ILifetime
        {
            return ToCommandUnattached(execute, canExecute, scheduler, errorHandler ?? owner.ErrorHandler)
                .AttachTo(owner);
        }

        public static ReactiveCommand<TParam, TResult> ToCommandUnattached<TParam, TResult>(this Func<TParam, TResult> execute, IObservable<bool> canExecute = null,
            IScheduler scheduler = null, ObservedErrorHandler errorHandler = null)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));

            return new ReactiveCommand<TParam, TResult>(
                (param, _) =>
                {
                    TResult result;
                    try { result = execute(param); }
                    catch (Exception ex) { return Task.FromException<TResult>(ex); }
                    return Task.FromResult(result);
                },
                canExecute, scheduler ?? ReactiveMvvmContext.Current.MainThreadScheduler, errorHandler ?? ReactiveMvvmContext.Current.DefaultErrorHandler);
        }

        public static ReactiveCommand<TParam, TResult> ToCommand<TOwner, TParam, TResult>(this Func<TParam, TResult> execute, TOwner owner, IObservable<bool> canExecute = null,
            IScheduler scheduler = null, ObservedErrorHandler errorHandler = null)
            where TOwner : IObservedErrorSource, ILifetime
        {
            return ToCommandUnattached(execute, canExecute, scheduler, errorHandler ?? owner.ErrorHandler)
                .AttachTo(owner);
        }

        #endregion

        #region From tasks

        public static ReactiveCommand<Unit, Unit> ToCommandUnattached(this Func<CancellationToken, Task> execute, IObservable<bool> canExecute = null,
            IScheduler scheduler = null, ObservedErrorHandler errorHandler = null)
        {
            return ToCommandUnattached<Unit, Unit>(
                async (_, ct) =>
                {
                    await execute(ct).ConfigureAwait(false);
                    return Unit.Default;
                },
                canExecute, scheduler, errorHandler);
        }

        public static ReactiveCommand<Unit, Unit> ToCommand<TOwner>(this Func<CancellationToken, Task> execute, TOwner owner, IObservable<bool> canExecute = null,
            IScheduler scheduler = null, ObservedErrorHandler errorHandler = null)
            where TOwner : IObservedErrorSource, ILifetime
        {
            return ToCommandUnattached(execute, canExecute, scheduler, errorHandler ?? owner.ErrorHandler)
                .AttachTo(owner);
        }

        public static ReactiveCommand<TParam, Unit> ToCommandUnattached<TParam>(this Func<TParam, CancellationToken, Task> execute, IObservable<bool> canExecute = null,
            IScheduler scheduler = null, ObservedErrorHandler errorHandler = null)
        {
            return ToCommandUnattached<TParam, Unit>(
                async (param, ct) =>
                {
                    await execute(param, ct).ConfigureAwait(false);
                    return Unit.Default;
                },
                canExecute, scheduler, errorHandler);
        }

        public static ReactiveCommand<TParam, Unit> ToCommand<TOwner, TParam>(this Func<TParam, CancellationToken, Task> execute, TOwner owner, IObservable<bool> canExecute = null,
            IScheduler scheduler = null, ObservedErrorHandler errorHandler = null)
            where TOwner : IObservedErrorSource, ILifetime
        {
            return ToCommandUnattached(execute, canExecute, scheduler, errorHandler ?? owner.ErrorHandler)
                .AttachTo(owner);
        }

        public static ReactiveCommand<Unit, TResult> ToCommandUnattached<TResult>(this Func<CancellationToken, Task<TResult>> execute, IObservable<bool> canExecute = null,
            IScheduler scheduler = null, ObservedErrorHandler errorHandler = null)
        {
            return ToCommandUnattached<Unit, TResult>(
                async (_, ct) => await execute(ct).ConfigureAwait(false),
                canExecute, scheduler, errorHandler);
        }

        public static ReactiveCommand<Unit, TResult> ToCommand<TOwner, TResult>(this Func<CancellationToken, Task<TResult>> execute, TOwner owner, IObservable<bool> canExecute = null,
            IScheduler scheduler = null, ObservedErrorHandler errorHandler = null)
            where TOwner : IObservedErrorSource, ILifetime
        {
            return ToCommandUnattached(execute, canExecute, scheduler, errorHandler ?? owner.ErrorHandler)
                .AttachTo(owner);
        }

        public static ReactiveCommand<TParam, TResult> ToCommandUnattached<TParam, TResult>(this Func<TParam, CancellationToken, Task<TResult>> execute, IObservable<bool> canExecute = null,
            IScheduler scheduler = null, ObservedErrorHandler errorHandler = null)
        {
            return new ReactiveCommand<TParam, TResult>(execute,
                canExecute, scheduler ?? ReactiveMvvmContext.Current.MainThreadScheduler, errorHandler ?? ReactiveMvvmContext.Current.DefaultErrorHandler);
        }

        public static ReactiveCommand<TParam, TResult> ToCommand<TOwner, TParam, TResult>(this Func<TParam, CancellationToken, Task<TResult>> execute, TOwner owner, IObservable<bool> canExecute = null,
            IScheduler scheduler = null, ObservedErrorHandler errorHandler = null)
            where TOwner : IObservedErrorSource, ILifetime
        {
            return ToCommandUnattached(execute, canExecute, scheduler, errorHandler ?? owner.ErrorHandler)
                .AttachTo(owner);
        }

        #endregion

        #region From commands

        public static CombinedReactiveCommand<TParam, TResult> ToCombinedCommandUnattached<TParam, TResult>(this IEnumerable<ReactiveCommand<TParam, TResult>> commands, IObservable<bool> canExecute = null,
            IScheduler scheduler = null, ObservedErrorHandler errorHandler = null)
        {
            return new CombinedReactiveCommand<TParam, TResult>(commands, canExecute, scheduler, errorHandler ?? ReactiveMvvmContext.Current.DefaultErrorHandler);
        }

        public static CombinedReactiveCommand<TParam, TResult> ToCombinedCommand<TOwner, TParam, TResult>(this IEnumerable<ReactiveCommand<TParam, TResult>> commands, TOwner owner, IObservable<bool> canExecute = null,
            IScheduler scheduler = null, ObservedErrorHandler errorHandler = null)
            where TOwner : IObservedErrorSource, ILifetime
        {
            return new CombinedReactiveCommand<TParam, TResult>(commands, canExecute, scheduler, errorHandler ?? owner.ErrorHandler)
                .AttachTo(owner);
        }

        #endregion
    }
}
