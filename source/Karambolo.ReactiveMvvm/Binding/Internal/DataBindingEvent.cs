using System.Globalization;
using Karambolo.ReactiveMvvm.Expressions;

namespace Karambolo.ReactiveMvvm.Binding.Internal
{
    class DataBindingEvent<TSource, TTarget> : IDataBindingEvent<TSource, TTarget>
    {
        readonly DataBindingEventFlags _flags;

        public DataBindingEvent(ObservedValue<TSource> sourceValue, ObservedValue<object> targetContainer, DataMemberAccessLink targetLink, 
            IBindingConverter<TSource, TTarget> converter, object converterParameter, CultureInfo converterCulture)
        {
            SourceValue = sourceValue;
            if (converter.TryConvert(sourceValue, converterParameter, converterCulture, out var convertedValue))
            {
                TargetValue = convertedValue;
                _flags = DataBindingEventFlags.None;
            }
            else
            {
                TargetValue = default;
                _flags = DataBindingEventFlags.ConversionFailed;
            }

            Container = targetContainer;
            Link = targetLink;
        }

        public DataBindingEvent(ObservedValue<TTarget> targetValue, ObservedValue<object> sourceContainer, DataMemberAccessLink sourceLink,
            IBindingConverter<TTarget, TSource> converter, object converterParameter, CultureInfo converterCulture)
        {
            TargetValue = targetValue;
            if (converter.TryConvert(targetValue, converterParameter, converterCulture, out var convertedValue))
            {
                SourceValue = convertedValue;
                _flags = DataBindingEventFlags.FlowsToSource;
            }
            else
            {
                SourceValue = default;
                _flags = DataBindingEventFlags.FlowsToSource | DataBindingEventFlags.ConversionFailed;
            }

            Container = sourceContainer;
            Link = sourceLink;
        }

        public ObservedValue<TSource> SourceValue { get; }
        public ObservedValue<TTarget> TargetValue { get; }
        public ObservedValue<object> Container { get; }
        public DataMemberAccessLink Link { get; }
        public bool FlowsToSource => (_flags & DataBindingEventFlags.FlowsToSource) != 0;
        public bool ConversionFailed => (_flags & DataBindingEventFlags.ConversionFailed) != 0;
    }
}
