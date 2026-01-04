using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows.Input;

namespace Karambolo.ReactiveMvvm.Binding.Internal
{
    public class EventCommandBinder : ICommandBinder
    {
        protected class ContainerMetadata
        {
            public EventInfo Event;
            public Type EventArgType;
        }

        private static readonly string[] s_defaultEvents =
        {
            "Click",
            "MouseUp",
        };

        private static readonly ConcurrentDictionary<(Type, string), ContainerMetadata> s_containerMetadataCache = new ConcurrentDictionary<(Type, string), ContainerMetadata>();
        
        private static readonly MethodInfo s_getEventsMethodDefinition = 
            new Func<object, ContainerMetadata, IObservable<object>>(new EventCommandBinder().GetEvents<Delegate, object>)
                .Method.GetGenericMethodDefinition();

        protected virtual IEnumerable<string> DefaultEventNames => s_defaultEvents;

        protected virtual ContainerMetadata CreateContainerMetadata(
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicEvents)]
#endif
            Type containerType,
            EventInfo @event, Type eventArgType)
        {
            return new ContainerMetadata
            {
                Event = @event,
                EventArgType = eventArgType
            };
        }

        private ContainerMetadata GetCachedContainerMetadata(
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicEvents)]
#endif
            Type containerType,
            string eventName)
        {
            if (!s_containerMetadataCache.TryGetValue((containerType, eventName), out ContainerMetadata containerMetadata))
            {
                EventInfo @event =
                    eventName == null ?
                    DefaultEventNames.Select(evtName => containerType.GetEvent(evtName)).FirstOrDefault(evt => evt != null) :
                    containerType.GetEvent(eventName);

                if (@event == null)
                    return null;

                Type eventArgType;
                if (ReactiveMvvmContext.IsUntrimmed && ReactiveMvvmContext.IsDynamicCodeCompiled)
                {
                    ParameterInfo eventArgParam;
                    MethodInfo invokeMethod = @event.EventHandlerType.GetMethod("Invoke");
                    ParameterInfo[] invokeMethodParams = invokeMethod.GetParameters();

                    if (invokeMethodParams.Length != 2 ||
                        !invokeMethodParams[0].ParameterType.IsAssignableFrom(containerType) ||
                        (eventArgParam = invokeMethodParams[1]).ParameterType.IsByRef)
                        return null;

                    eventArgType = eventArgParam.ParameterType;
                }
                else
                    eventArgType = null;

                containerMetadata = CreateContainerMetadata(containerType, @event, eventArgType);
                if (containerMetadata == null)
                    return null;

                s_containerMetadataCache[(containerType, eventName)] = containerMetadata;
            }

            return containerMetadata;
        }

        public virtual bool CanBind(
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
            TParam>(
                ICommand command, TContainer container, IObservable<TParam> commandParameters, ContainerMetadata containerMetadata,
                IScheduler scheduler, Action<Exception> onError)
        {
            IObservable<object> events;

            if (ReactiveMvvmContext.IsUntrimmed && ReactiveMvvmContext.IsDynamicCodeCompiled)
            {
                MethodInfo getEventsMethod = s_getEventsMethodDefinition.MakeGenericMethod(containerMetadata.Event.EventHandlerType, containerMetadata.EventArgType);
                events = (IObservable<object>)getEventsMethod.Invoke(this, new object[] { container, containerMetadata });
            }
            else
            {
#pragma warning disable IL2026 // in theory, covariance allows a less derived container type but that's unrealistic, so we ignore this edge case
                events = Observable.FromEventPattern(container, containerMetadata.Event.Name);
#pragma warning restore IL2026
            }

            return events
                .WithLatestFrom(commandParameters.StartWith(default(TParam)), (_, param) => param)
                .Subscribe(
                    param =>
                    {
                        if (command.CanExecute(param))
                            command.Execute(param);
                    },
                    onError);
        }

        private IObservable<object> GetEvents<TEventHandler, TEventArgs>(object container, ContainerMetadata containerMetadata)
            where TEventHandler : Delegate
        {
            return Observable.FromEventPattern<TEventHandler, TEventArgs>(
                handler => containerMetadata.Event.AddEventHandler(container, handler),
                handler => containerMetadata.Event.RemoveEventHandler(container, handler))
                .Select(_ => default(object));
        }

        public MemberInfo GetContainerMember(
#if NET5_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.PublicEvents)]
#endif
            Type containerType,
            string eventName)
        {
            return GetCachedContainerMetadata(containerType, eventName)?.Event;
        }
    }
}
