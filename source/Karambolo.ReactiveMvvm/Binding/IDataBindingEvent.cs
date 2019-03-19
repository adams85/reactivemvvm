namespace Karambolo.ReactiveMvvm
{
    public interface IDataBindingEvent<TSource, TTarget> : IReactiveBindingEvent<TSource, TTarget>
    {
        bool FlowsToSource { get; }
        bool ConversionFailed { get; }
    }
}
