namespace Karambolo.ReactiveMvvm
{
    public interface IViewModelFactory
    {
        TViewModel CreateViewModel<TViewModel>(params object[] parameters)
            where TViewModel : class;

        TViewModel CreateViewModelScoped<TViewModel>(params object[] parameters)
            where TViewModel : class, ILifetime;
    }
}
