namespace Karambolo.ReactiveMvvm
{
    public interface IBoundView<TViewModel> : IBoundView
        where TViewModel : class
    {
        new TViewModel ViewModel { get; set; }
    }
}
