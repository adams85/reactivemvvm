namespace Karambolo.ReactiveMvvm
{
    public interface IReactiveBindingEvent<TSource, TTarget>
    {
        ObservedValue<TSource> SourceValue { get; }
        ObservedValue<TTarget> TargetValue { get; }
    }
}
