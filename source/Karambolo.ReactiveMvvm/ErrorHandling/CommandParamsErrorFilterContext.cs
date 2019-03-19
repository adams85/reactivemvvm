using System;
using System.Windows.Input;

namespace Karambolo.ReactiveMvvm.ErrorHandling
{
    public sealed class CommandParamsErrorException : ObservedErrorException
    {
        internal static CommandParamsErrorException Create(ICommand command, Exception exception)
        {
            return new CommandParamsErrorException(command, exception);
        }

        CommandParamsErrorException(object sourceObject, Exception exception) : base(sourceObject, exception) { }
    }
}
