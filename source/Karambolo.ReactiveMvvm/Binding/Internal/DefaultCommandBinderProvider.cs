using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
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

        public ICommandBinder Provide<
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicEvents)]
#endif
            TContainer>(ObservedValue<TContainer> container, string eventName)
        {
            if (!container.IsAvailable || container.Value == null)
            {
                return null;
            }

            Func<(Type, string), DefaultCommandBinderProvider, ICommandBinder> binderFactory = (key, @this) =>
            {
                (Type containerType, string evtName) = key;
                ICommandBinder binder;
                for (int i = 0; i < @this._binders.Length; i++)
                    // In theory, covariance allows a less derived container type but that's unrealistic, so we ignore this edge case.
                    if ((binder = @this._binders[i]).CanBind(containerType, evtName))
                        return binder;

                return null;
            };

            (Type, string eventName) binderKey = (container.Value.GetType(), eventName);

#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER || NET472_OR_GREATER
            return s_binderCache.GetOrAdd(binderKey, binderFactory, this);
#else
            if (!s_binderCache.TryGetValue(binderKey, out ICommandBinder commandBinder))
                commandBinder = s_binderCache.GetOrAdd(binderKey, key => binderFactory(key, this));

            return commandBinder;
#endif
        }
    }
}
