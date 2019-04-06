namespace Karambolo.ReactiveMvvm
{
    public interface IReactiveView<TViewModel> : IActivableView, IBoundView<TViewModel>
        where TViewModel : class
    { }
}
