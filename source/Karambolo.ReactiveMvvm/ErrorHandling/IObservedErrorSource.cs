namespace Karambolo.ReactiveMvvm.ErrorHandling
{
    public interface IObservedErrorSource
    {
        ObservedErrorHandler ErrorHandler { get; }
    }
}
