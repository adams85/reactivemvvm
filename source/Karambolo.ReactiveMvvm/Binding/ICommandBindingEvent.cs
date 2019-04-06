using System.Windows.Input;

namespace Karambolo.ReactiveMvvm.Binding
{
    public interface ICommandBindingEvent<TCommand, TContainer> : IReactiveBindingEvent<TCommand, TContainer>
        where TCommand : ICommand
    { }
}
