using System;
using System.Collections.Concurrent;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows.Input;
using Karambolo.ReactiveMvvm.Internal;

namespace Karambolo.ReactiveMvvm.Binding.Internal
{
    public class PropertyCommandBinder : ICommandBinder
    {
        protected class ContainerMetadata
        {
            public PropertyInfo CommandProperty;
            public PropertyInfo CommandParameterProperty;
        }

        static readonly (string Name, Type ReturnType) s_commandProperty = ("Command", typeof(ICommand));
        static readonly (string Name, Type ReturnType) s_commandParameterProperty = ("CommandParameter", typeof(object));

        static readonly ConcurrentDictionary<Type, ContainerMetadata> s_containerMetadataCache = new ConcurrentDictionary<Type, ContainerMetadata>();

        ContainerMetadata CreateContainerMetadata(PropertyInfo commandProperty, PropertyInfo commandParameterProperty)
        {
            return new ContainerMetadata
            {
                CommandProperty = commandProperty,
                CommandParameterProperty = commandParameterProperty
            };
        }

        ContainerMetadata GetCachedContainerMetadata(Type containerType, string eventName)
        {
            if (eventName != null)
                return null;

            if (!s_containerMetadataCache.TryGetValue(containerType, out var containerMetadata))
            {
                var commandProperty = containerType.GetProperty(s_commandProperty.Name, s_commandProperty.ReturnType);
                var commandParameterProperty = containerType.GetProperty(s_commandParameterProperty.Name, s_commandParameterProperty.ReturnType);

                if (commandProperty == null || commandParameterProperty == null)
                    return null;

                s_containerMetadataCache[containerType] = containerMetadata = CreateContainerMetadata(commandProperty, commandParameterProperty);
            }

            return containerMetadata;
        }

        public bool CanBind(Type containerType, string eventName)
        {
            return GetCachedContainerMetadata(containerType, eventName) != null;
        }

        protected virtual IDisposable Bind<TParam>(ICommand command, object container, IObservable<TParam> commandParameters, ContainerMetadata containerMetadata,
            IScheduler scheduler, Action<Exception> onError)
        {
            var originalCommand = containerMetadata.CommandProperty.GetValue(container, null);
            var originalCommandParam = containerMetadata.CommandParameterProperty.GetValue(container, null);

            var restoreDisposable = Disposable.Create(() =>
            {
                containerMetadata.CommandProperty.SetValue(container, originalCommand, null);
                containerMetadata.CommandParameterProperty.SetValue(container, originalCommandParam, null);
            });

            var commandParameterSubscription = commandParameters
                .ObserveOnSafe(scheduler)
                .Subscribe(
                    param => containerMetadata.CommandParameterProperty.SetValue(container, param, null),
                    onError);

            containerMetadata.CommandProperty.SetValue(container, command, null);

            return new CompositeDisposable(restoreDisposable, commandParameterSubscription);
        }

        public IDisposable Bind<TParam>(ICommand command, object container, IObservable<TParam> commandParameters, string eventName, IScheduler scheduler, Action<Exception> onError)
        {
            var containerMetadata = GetCachedContainerMetadata(container.GetType(), eventName);
            return Bind(command, container, commandParameters, containerMetadata, scheduler, onError);
        }

        public MemberInfo GetContainerMember(Type containerType, string eventName)
        {
            return GetCachedContainerMetadata(containerType, eventName)?.CommandProperty;
        }
    }
}
