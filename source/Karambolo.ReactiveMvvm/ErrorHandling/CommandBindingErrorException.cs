using System;
using Karambolo.ReactiveMvvm.Expressions;

namespace Karambolo.ReactiveMvvm.ErrorHandling
{
    public sealed class CommandBindingErrorException : BindingErrorException
    {
        internal static CommandBindingErrorException Create<TViewModel>(IBoundView<TViewModel> view, DataMemberAccessChain commandAccessChain, DataMemberAccessChain containerAccessChain,
            Exception exception)
            where TViewModel : class
        {
            return new CommandBindingErrorException(view, commandAccessChain, containerAccessChain, exception);
        }

        private CommandBindingErrorException(object sourceObject, DataMemberAccessChain commandAccessChain, DataMemberAccessChain containerAccessChain, Exception exception)
            : base(sourceObject, ReactiveBindingMode.OneWay, exception)
        {
            CommandAccessChain = commandAccessChain;
            ContainerAccessChain = containerAccessChain;
        }

        public DataMemberAccessChain CommandAccessChain { get; }
        public DataMemberAccessChain ContainerAccessChain { get; }
    }
}
