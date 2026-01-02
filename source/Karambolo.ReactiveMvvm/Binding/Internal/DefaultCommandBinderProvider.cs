using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;

namespace Karambolo.ReactiveMvvm.Binding.Internal
{
    public class DefaultCommandBinderProvider : ICommandBinderProvider
    {
        private static readonly ConcurrentDictionary<(Type, string), ICommandBinder> s_binderCache = new ConcurrentDictionary<(Type, string), ICommandBinder>();
        private readonly ICommandBinder[] _binders;

        public DefaultCommandBinderProvider(IOptions<ReactiveMvvmOptions> options)
        {
            _binders = options.Value.CommandBinders.ToArray();
        }

        public ICommandBinder Provide<TContainer>(ObservedValue<TContainer> container, string eventName)
        {
            return
                container.IsAvailable && container.Value != null ?
                s_binderCache.GetOrAdd((container.Value.GetType(), eventName), key =>
                {
                    (Type containerType, string evtName) = key;
                    ICommandBinder binder;
                    for (int i = 0; i < _binders.Length; i++)
                        if ((binder = _binders[i]).CanBind(containerType, evtName))
                            return binder;

                    return null;
                }) :
                null;
        }
    }
}
