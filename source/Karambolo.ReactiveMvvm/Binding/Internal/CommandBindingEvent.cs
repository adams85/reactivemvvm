using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

namespace Karambolo.ReactiveMvvm.Binding.Internal
{
    internal sealed class CommandBindingEvent<
        TCommand,
#if NET5_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicEvents)]
#endif
        TContainer>
        : ICommandBindingEvent<TCommand, TContainer> where TCommand : ICommand
    {
        public CommandBindingEvent(ObservedValue<TCommand> command, ObservedValue<TContainer> container, string eventName, ICommandBinderProvider binderProvider)
        {
            SourceValue = command;
            TargetValue = container;
            EventName = eventName;
            Binder = binderProvider.Provide(container, eventName);
        }

        public ObservedValue<TCommand> SourceValue { get; }
        public ObservedValue<TContainer> TargetValue { get; }
        public string EventName { get; }
        public ICommandBinder Binder { get; }
        public bool BindingNotPossible => Binder == null;
    }
}
