namespace Karambolo.ReactiveMvvm.Binding.Internal
{
    public interface ICommandBinderProvider
    {
        ICommandBinder Provide<TContainer>(ObservedValue<TContainer> container, string eventName);
    }
}
