using System;
using System.Windows.Input;

namespace Karambolo.ReactiveMvvm.ErrorHandling
{
    public sealed class CommandCanExecuteErrorException : ObservedErrorException
    {
        internal static CommandCanExecuteErrorException Create(ICommand command, Exception exception)
        {
            return new CommandCanExecuteErrorException(command, exception);
        }

        CommandCanExecuteErrorException(object sourceObject, Exception exception) : base(sourceObject, exception) { }
    }
}
