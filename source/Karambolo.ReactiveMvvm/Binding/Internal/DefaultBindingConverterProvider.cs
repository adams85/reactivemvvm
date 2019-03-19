﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;

namespace Karambolo.ReactiveMvvm.Binding.Internal
{
    public class DefaultBindingConverterProvider : IBindingConverterProvider
    {
        static readonly ConcurrentDictionary<(Type, Type), IBindingConverter> s_converterCache = new ConcurrentDictionary<(Type, Type), IBindingConverter>();

        readonly IReadOnlyList<IBindingConverter> _globalConverters;

        public DefaultBindingConverterProvider(IOptions<ReactiveMvvmOptions> options)
        {
            _globalConverters = options.Value.BindingConverters.ToArray();
        }

        public IBindingConverter<TFrom, TTo> Provide<TFrom, TTo>()
        {
            return (IBindingConverter<TFrom, TTo>)s_converterCache.GetOrAdd((typeof(TFrom), typeof(TTo)), key =>
            {
                var (fromType, toType) = key;
                IBindingConverter converter;
                for (int i = 0, n = _globalConverters.Count; i < n; i++)
                    if ((converter = _globalConverters[i]).CanConvert(fromType, toType))
                        return converter is IBindingConverter<TFrom, TTo> ? converter : new GenericBindingConverterAdapter<TFrom, TTo>(converter);

                if (typeof(TFrom) == typeof(TTo))
                    return NullBindingConverter<TFrom>.Instance;

                return FallbackBindingConverter<TFrom, TTo>.Instance;
            });
        }
    }
}
