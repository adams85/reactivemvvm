using Karambolo.ReactiveMvvm.Expressions;

namespace Karambolo.ReactiveMvvm.Binding.Internal
{
    readonly struct DataBindingChange<TSource>
    {
        public DataBindingChange(ObservedValue<TSource> sourceValue, ObservedValue<object> targetContainer, DataMemberAccessLink targetLink)
        {
            SourceValue = sourceValue;
            TargetContainer = targetContainer;
            TargetLink = targetLink;
        }

        public ObservedValue<TSource> SourceValue { get; }
        public ObservedValue<object> TargetContainer { get; }
        public DataMemberAccessLink TargetLink { get; }
    }
}
