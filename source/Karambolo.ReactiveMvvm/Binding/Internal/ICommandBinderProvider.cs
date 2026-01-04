using System.Diagnostics.CodeAnalysis;

namespace Karambolo.ReactiveMvvm.Binding.Internal
{
    public interface ICommandBinderProvider
    {
        ICommandBinder Provide<
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicEvents)]
#endif
            TContainer>(ObservedValue<TContainer> container, string eventName);
    }
}
