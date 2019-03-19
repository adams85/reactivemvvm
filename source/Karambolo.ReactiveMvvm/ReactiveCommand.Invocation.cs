using System;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Input;
using Karambolo.Common;
using Karambolo.ReactiveMvvm.ErrorHandling;
using Karambolo.ReactiveMvvm.Expressions;
using Karambolo.ReactiveMvvm.Internal;

namespace Karambolo.ReactiveMvvm
{
    public static partial class ReactiveCommand
    {
        static IDisposable InvokeCommandSyncCore<T>(this IObservable<(ICommand command, T param)> executionInfo,
            IScheduler scheduler, ObservedErrorHandler errorHandler)
        {
            return executionInfo
                .ObserveOnSafe(scheduler)
                .Subscribe(info =>
                {
                    try
                    {
                        if (info.command.CanExecute(info.param))
                            info.command.Execute(info.param);
                    }
                    catch (Exception ex) { errorHandler.Handle(ex); }
                }, errorHandler.Handle);
        }

        public static IDisposable InvokeCommandSync<TParam>(this IObservable<TParam> parameters, ICommand command,
            IScheduler scheduler = null, ObservedErrorHandler errorHandler = null)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (errorHandler == null)
                errorHandler = ReactiveMvvmContext.Current.DefaultErrorHandler;

            var canExecuteChanges = Observable
                .FromEventPattern(handler => command.CanExecuteChanged += handler, handler => command.CanExecuteChanged -= handler)
                .Select(_ => Unit.Default)
                .StartWith(Unit.Default);

            return InvokeCommandSyncCore(
                parameters
                    .Catch<TParam, Exception>(ex => errorHandler.Filter<TParam>(CommandParamsErrorException.Create(command, ex)))
                    .Select(param => (command, param)),
                scheduler, errorHandler);
        }

        public static IDisposable InvokeCommandSync<TParam, TSource>(this IObservable<TParam> parameters, TSource source,
            Expression<Func<TSource, ICommand>> propertyExpression,
            IScheduler scheduler = null, ObservedErrorHandler errorHandler = null)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            if (errorHandler == null)
                errorHandler = ReactiveMvvmContext.Current.DefaultErrorHandler;

            var chain = DataMemberAccessChain.From(propertyExpression);
            var commandValues = source.WhenChange<ICommand>(chain);

            var commands = commandValues
                .Select(command =>
                    command.IsAvailable && command.Value != null ?
                    Observable.FromEventPattern(handler => command.Value.CanExecuteChanged += handler, handler => command.Value.CanExecuteChanged -= handler)
                        .Select(_ => command.Value)
                        .StartWith(command.Value) :
                    Empty<ICommand>.Observable)
                .Switch();

            return InvokeCommandSyncCore(
                parameters
                    .Catch<TParam, Exception>(ex => errorHandler.Filter<TParam>(CommandParamsErrorException.Create(chain.GetValue<ICommand>(source).GetValueOrDefault(), ex)))
                    .WithLatestFrom(commands, (param, command) => (command, param)),
                scheduler, errorHandler);
        }

        static IDisposable InvokeCommandCore<TParam, TResult>(this IObservable<(ReactiveCommand<TParam, TResult> command, bool canExecute, TParam param)> executionInfo,
            IScheduler scheduler, ObservedErrorHandler errorHandler, CancellationToken cancellationToken)
        {
            return executionInfo
                .Where(info => info.canExecute)
                .ObserveOnSafe(scheduler)
                .Subscribe(info => info.command.ExecuteAsync(info.param, cancellationToken).FireAndForget(errorHandler.Handle), errorHandler.Handle);
        }

        public static IDisposable InvokeCommand<TParam, TResult>(this IObservable<TParam> parameters, ReactiveCommand<TParam, TResult> command,
            IScheduler scheduler = null, ObservedErrorHandler errorHandler = null, CancellationToken cancellationToken = default)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (errorHandler == null)
                errorHandler = ReactiveMvvmContext.Current.DefaultErrorHandler;

            return InvokeCommandCore(
                parameters
                    .Catch<TParam, Exception>(ex => errorHandler.Filter<TParam>(CommandParamsErrorException.Create(command, ex)))
                    .WithLatestFrom(command.WhenCanExecuteChanged, (param, canExecute) => (command, canExecute, param)),
                scheduler, errorHandler, cancellationToken);
        }

        public static IDisposable InvokeCommand<TParam, TSource, TResult>(this IObservable<TParam> parameters, TSource source,
            Expression<Func<TSource, ReactiveCommand<TParam, TResult>>> propertyExpression,
            IScheduler scheduler = null, ObservedErrorHandler errorHandler = null, CancellationToken cancellationToken = default)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            if (errorHandler == null)
                errorHandler = ReactiveMvvmContext.Current.DefaultErrorHandler;

            var chain = DataMemberAccessChain.From(propertyExpression);
            var commandValues = source.WhenChange<ReactiveCommand<TParam, TResult>>(chain);

            var commands = commandValues
                .Select(command =>
                    command.IsAvailable && command.Value != null ?
                    command.Value.WhenCanExecuteChanged.Select(canExecute => (value: command.Value, canExecute)) :
                    Observable.Empty<(ReactiveCommand<TParam, TResult> value, bool canExecute)>())
                .Switch();

            return InvokeCommandCore(
                parameters
                    .Catch<TParam, Exception>(ex => errorHandler.Filter<TParam>(CommandParamsErrorException.Create(chain.GetValue<ICommand>(source).GetValueOrDefault(), ex)))
                    .WithLatestFrom(commands, (param, command) => (command.value, command.canExecute, param)),
                scheduler, errorHandler, cancellationToken);
        }
    }
}
