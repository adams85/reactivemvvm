using System;
using System.Collections.Concurrent;
using System.Linq;
using Microsoft.Extensions.Options;

namespace Karambolo.ReactiveMvvm.Binding.Internal
{
    public class DefaultBindingConverterProvider : IBindingConverterProvider
    {
        private static readonly ConcurrentDictionary<(Type, Type), IBindingConverter> s_converterCache = new ConcurrentDictionary<(Type, Type), IBindingConverter>();
        private readonly IBindingConverter[] _globalConverters;

        public DefaultBindingConverterProvider(IOptions<ReactiveMvvmOptions> options)
        {
            _globalConverters = options.Value.BindingConverters.ToArray();
        }

        public IBindingConverter<TFrom, TTo> Provide<TFrom, TTo>()
        {
            Func<(Type, Type), DefaultBindingConverterProvider, IBindingConverter> converterFactory = (key, @this) =>
            {
                (Type fromType, Type toType) = key;
                IBindingConverter converter;
                for (int i = 0; i < @this._globalConverters.Length; i++)
                    if ((converter = @this._globalConverters[i]).CanConvert(fromType, toType))
                        return converter is IBindingConverter<TFrom, TTo> ? converter : new GenericBindingConverterAdapter<TFrom, TTo>(converter);

                if (typeof(TFrom) == typeof(TTo))
                    return NullBindingConverter<TFrom>.Instance;

                return FallbackBindingConverter<TFrom, TTo>.Instance;
            };

            (Type, Type) converterKey = (typeof(TFrom), typeof(TTo));

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER || NET472_OR_GREATER
            return (IBindingConverter<TFrom, TTo>)s_converterCache.GetOrAdd(converterKey, converterFactory, this);
#else
            if (!s_converterCache.TryGetValue(converterKey, out IBindingConverter bindingConverter))
                bindingConverter = s_converterCache.GetOrAdd(converterKey, key => converterFactory(key, this));

            return (IBindingConverter<TFrom, TTo>)bindingConverter;
#endif
        }
    }
}
