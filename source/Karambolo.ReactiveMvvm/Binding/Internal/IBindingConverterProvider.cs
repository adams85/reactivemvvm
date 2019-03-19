namespace Karambolo.ReactiveMvvm.Binding.Internal
{
    public interface IBindingConverterProvider
    {
        IBindingConverter<TFrom, TTo> Provide<TFrom, TTo>();
    }
}
