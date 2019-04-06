using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Karambolo.Common;
using Karambolo.ReactiveMvvm.ErrorHandling;

namespace Karambolo.ReactiveMvvm
{
    public sealed class CombinedReactiveCommand<TParam, TResult> : ReactiveCommand<TParam, TResult[]>
    {
        private new class Initializer : ReactiveCommand<TParam, TResult[]>.Initializer
        {
            private readonly ReactiveCommand<TParam, TResult>[] _commands;

            public Initializer(ReactiveCommand<TParam, TResult>[] commands)
            {
                _commands = commands;
            }

            public override Func<TParam, CancellationToken, Task<TResult[]>> GetExecute(Func<TParam, CancellationToken, Task<TResult[]>> execute, Func<Exception, IObservable<TResult[]>> errorFilter)
            {
                return (param, ct) => Task.WhenAll(_commands.Select(command => command.ExecuteAsync(param, ct)));
            }

            public override IObservable<bool> GetCanExecute(IObservable<bool> canExecute, Func<Exception, IObservable<bool>> errorFilter)
            {
                IObservable<bool> combinedCanExecute = Observable
                    .CombineLatest(_commands.Select(command => command.WhenCanExecuteChanged))
                    .Select(canExecs => canExecs.TrueForAll(canExec => canExec));

                return base.GetCanExecute(canExecute, errorFilter)
                    .CombineLatest(combinedCanExecute, (canExec, combCanExec) => canExec && combCanExec);
            }
        }

        public CombinedReactiveCommand(IEnumerable<ReactiveCommand<TParam, TResult>> commands, IObservable<bool> canExecute = null,
            IScheduler scheduler = null, ObservedErrorHandler errorHandler = null)
            : base(new Initializer(commands?.ToArray() ?? throw new ArgumentNullException(nameof(commands))), null, canExecute, scheduler, errorHandler) { }
    }
}
