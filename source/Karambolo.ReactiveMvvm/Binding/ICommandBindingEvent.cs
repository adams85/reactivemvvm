using System.Windows.Input;

namespace Karambolo.ReactiveMvvm
{
    public interface ICommandBindingEvent<TCommand, TContainer> : IReactiveBindingEvent<TCommand, TContainer>
        where TCommand : ICommand { }
}
