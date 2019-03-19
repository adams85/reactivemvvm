namespace Karambolo.ReactiveMvvm
{
    public interface IBoundViewProvider<TViewModel, TView>
        where TView : IBoundView<TViewModel>
        where TViewModel : class
    {
        TView View { get; }
    }
}
