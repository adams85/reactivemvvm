namespace Karambolo.ReactiveMvvm
{
    public interface IViewModelFactory
    {
        TViewModel CreateViewModel<TViewModel>(bool withScope = true)
            where TViewModel : class, ILifetime;
    }
}
