using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;

namespace Karambolo.ReactiveMvvm.Binding.Internal
{
    public class DefaultCommandBinderProvider : ICommandBinderProvider
    {
        static readonly ConcurrentDictionary<(Type, string), ICommandBinder> s_binderCache = new ConcurrentDictionary<(Type, string), ICommandBinder>();

        readonly IReadOnlyList<ICommandBinder> _binders;

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
                    var (containerType, evtName) = key;
                    ICommandBinder binder;
                    for (int i = 0, n = _binders.Count; i < n; i++)
                        if ((binder = _binders[i]).CanBind(containerType, evtName))
                            return binder;

                    return null;
                }) :
                null;
        }
    }
}
