using System;
using System.Windows.Input;

namespace Karambolo.ReactiveMvvm.ErrorHandling
{
    public sealed class CommandExecuteErrorException : ObservedErrorException
    {
        internal static CommandExecuteErrorException Create(ICommand command, Exception exception)
        {
            return new CommandExecuteErrorException(command, exception);
        }

        CommandExecuteErrorException(object sourceObject, Exception exception) : base(sourceObject, exception) { }
    }
}
