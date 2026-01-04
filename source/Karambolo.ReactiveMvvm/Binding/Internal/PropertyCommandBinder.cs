using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows.Input;

namespace Karambolo.ReactiveMvvm.Binding.Internal
{
    public class PropertyCommandBinder : ICommandBinder
    {
        protected class ContainerMetadata
        {
            public PropertyInfo CommandProperty;
            public PropertyInfo CommandParameterProperty;
        }

        private static readonly (string Name, Type ReturnType) s_commandProperty = ("Command", typeof(ICommand));
        private static readonly (string Name, Type ReturnType) s_commandParameterProperty = ("CommandParameter", typeof(object));
        private static readonly ConcurrentDictionary<Type, ContainerMetadata> s_containerMetadataCache = new ConcurrentDictionary<Type, ContainerMetadata>();

        protected virtual ContainerMetadata CreateContainerMetadata(PropertyInfo commandProperty, PropertyInfo commandParameterProperty)
        {
            return new ContainerMetadata
            {
                CommandProperty = commandProperty,
                CommandParameterProperty = commandParameterProperty
            };
        }

        private ContainerMetadata GetCachedContainerMetadata(
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicEvents)]
#endif
            Type containerType,
            string eventName)
        {
            if (eventName != null)
                return null;

            if (!s_containerMetadataCache.TryGetValue(containerType, out ContainerMetadata containerMetadata))
            {
                PropertyInfo commandProperty = containerType.GetProperty(s_commandProperty.Name, s_commandProperty.ReturnType);
                PropertyInfo commandParameterProperty = containerType.GetProperty(s_commandParameterProperty.Name, s_commandParameterProperty.ReturnType);

                if (commandProperty == null ||
                    commandParameterProperty == null ||
                    (containerMetadata = CreateContainerMetadata(commandProperty, commandParameterProperty)) == null)
                    return null;

                s_containerMetadataCache[containerType] = containerMetadata;
            }

            return containerMetadata;
        }

        public bool CanBind(
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicEvents)]
#endif
            Type containerType,
            string eventName)
        {
            return GetCachedContainerMetadata(containerType, eventName) != null;
        }

        public IDisposable Bind<
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicEvents)]
#endif
            TContainer,
            TParam>(
                ICommand command, TContainer container, IObservable<TParam> commandParameters, string eventName,
                IScheduler scheduler, Action<Exception> onError)
        {
#pragma warning disable IL2072 // in theory, covariance allows a less derived container type but that's unrealistic, so we ignore this edge case
            ContainerMetadata containerMetadata = GetCachedContainerMetadata(container.GetType(), eventName);
#pragma warning restore IL2072

            return Bind(command, container, commandParameters, containerMetadata, scheduler, onError);
        }

        protected virtual IDisposable Bind<
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicEvents)]
#endif
            TContainer,
            TParam
            >(
                ICommand command, TContainer container, IObservable<TParam> commandParameters, ContainerMetadata containerMetadata,
                IScheduler scheduler, Action<Exception> onError)
        {
            var originalCommand = containerMetadata.CommandProperty.GetValue(container, null);
            var originalCommandParam = containerMetadata.CommandParameterProperty.GetValue(container, null);

            IDisposable restoreDisposable = Disposable.Create(() =>
            {
                containerMetadata.CommandProperty.SetValue(container, originalCommand, null);
                containerMetadata.CommandParameterProperty.SetValue(container, originalCommandParam, null);
            });

            IDisposable commandParameterSubscription = commandParameters
                .ObserveOnSafe(scheduler)
                .Subscribe(
                    param => containerMetadata.CommandParameterProperty.SetValue(container, param, null),
                    onError);

            containerMetadata.CommandProperty.SetValue(container, command, null);

            return new CompositeDisposable(restoreDisposable, commandParameterSubscription);
        }

        public MemberInfo GetContainerMember(
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicEvents)]
#endif
            Type containerType,
            string eventName)
        {
            return GetCachedContainerMetadata(containerType, eventName)?.CommandProperty;
        }
    }
}
