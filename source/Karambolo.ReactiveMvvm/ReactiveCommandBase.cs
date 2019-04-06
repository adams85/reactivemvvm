using System;
using System.Windows.Input;
using Karambolo.ReactiveMvvm.ErrorHandling;

namespace Karambolo.ReactiveMvvm
{
    public abstract class ReactiveCommandBase : ICommand, IObservedErrorSource
    {
        protected enum ExecutionEventKind
        {
            Begin,
            Result,
            End
        }

        protected readonly struct ExecutionEvent<TResult>
        {
            private ExecutionEvent(ExecutionEventKind kind, TResult value)
            {
                Kind = kind;
                Value = value;
            }

            public ExecutionEventKind Kind { get; }

            public TResult Value { get; }

            public static ExecutionEvent<TResult> Begin()
            {
                return new ExecutionEvent<TResult>(ExecutionEventKind.Begin, default);
            }

            public static ExecutionEvent<TResult> Result(TResult value)
            {
                return new ExecutionEvent<TResult>(ExecutionEventKind.Result, value);
            }

            public static ExecutionEvent<TResult> End()
            {
                return new ExecutionEvent<TResult>(ExecutionEventKind.End, default);
            }
        }

        protected ReactiveCommandBase() { }

        public abstract ObservedErrorHandler ErrorHandler { get; }

        public abstract IObservable<bool> WhenCanExecuteChanged { get; }

        public abstract IObservable<bool> WhenIsBusyChanged { get; }

        protected virtual void OnCanExecuteChanged()
        {
            _canExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        internal EventHandler _canExecuteChanged;
        event EventHandler ICommand.CanExecuteChanged
        {
            add { _canExecuteChanged += value; }
            remove { _canExecuteChanged -= value; }
        }

        protected abstract bool CanExecute(object parameter);

        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute(parameter);
        }

        protected abstract void Execute(object parameter);

        void ICommand.Execute(object parameter)
        {
            Execute(parameter);
        }
    }
}
