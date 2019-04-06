using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows.Input;
using Karambolo.Common;

namespace Karambolo.ReactiveMvvm.Binding.Internal
{
    public class EventCommandBinder : ICommandBinder
    {
        protected class ContainerMetadata
        {
            public EventInfo Event;
            public Type EventHandlerType;
            public Type EventArgType;
        }

        private static readonly string[] s_defaultEvents =
        {
            "Click",
            "MouseUp",
        };
        private static readonly ConcurrentDictionary<(Type, string), ContainerMetadata> s_containerMetadataCache = new ConcurrentDictionary<(Type, string), ContainerMetadata>();
        private static readonly MethodInfo s_bindImplMethodDefinition = Lambda.Method((EventCommandBinder binder) => binder.Bind<Delegate, object, object>(null, null, null, null, null, null))
            .GetGenericMethodDefinition();

        protected virtual IEnumerable<string> DefaultEventNames => s_defaultEvents;

        protected virtual ContainerMetadata CreateContainerMetadata(Type containerType, EventInfo @event, Type eventHandlerType, Type eventArgType)
        {
            return new ContainerMetadata
            {
                Event = @event,
                EventHandlerType = eventHandlerType,
                EventArgType = eventArgType
            };
        }

        private ContainerMetadata GetCachedContainerMetadata(Type containerType, string eventName)
        {
            if (!s_containerMetadataCache.TryGetValue((containerType, eventName), out ContainerMetadata containerMetadata))
            {
                EventInfo @event =
                    eventName == null ?
                    DefaultEventNames.Select(evtName => containerType.GetEvent(evtName)).FirstOrDefault(evt => evt != null) :
                    containerType.GetEvent(eventName);

                if (@event == null)
                    return null;

                Type eventHandlerType = @event.EventHandlerType;

                ParameterInfo eventArgParam;
                MethodInfo invokeMethod = eventHandlerType.GetMethod("Invoke");
                ParameterInfo[] invokeMethodParams = invokeMethod.GetParameters();
                if (invokeMethodParams.Length != 2 ||
                    !invokeMethodParams[0].ParameterType.IsAssignableFrom(containerType) ||
                    (eventArgParam = invokeMethodParams[1]).ParameterType.IsByRef)
                    return null;

                Type eventArgType = eventArgParam.ParameterType;

                s_containerMetadataCache[(containerType, eventName)] = containerMetadata = CreateContainerMetadata(containerType, @event, eventHandlerType, eventArgType);
            }

            return containerMetadata;
        }

        public virtual bool CanBind(Type containerType, string eventName)
        {
            return GetCachedContainerMetadata(containerType, eventName) != null;
        }

        public IDisposable Bind<TParam>(ICommand command, object container, IObservable<TParam> commandParameters, string eventName, IScheduler scheduler, Action<Exception> onError)
        {
            ContainerMetadata containerMetadata = GetCachedContainerMetadata(container.GetType(), eventName);

            MethodInfo bindImplMethod = s_bindImplMethodDefinition.MakeGenericMethod(containerMetadata.EventHandlerType, containerMetadata.EventArgType, typeof(TParam));
            return (IDisposable)bindImplMethod.Invoke(this, new[] { command, container, commandParameters, containerMetadata, scheduler, onError });
        }

        protected virtual IDisposable Bind<TEventHandler, TEventArgs, TParam>(ICommand command, object container, IObservable<TParam> commandParameters, ContainerMetadata containerMetadata,
            IScheduler scheduler, Action<Exception> onError)
            where TEventHandler : Delegate
        {
            return Observable.FromEventPattern<TEventHandler, TEventArgs>(
                handler => containerMetadata.Event.AddEventHandler(container, handler),
                handler => containerMetadata.Event.RemoveEventHandler(container, handler))
                .WithLatestFrom(commandParameters.StartWith(default(TParam)), (_, param) => param)
                .Subscribe(
                    param =>
                    {
                        if (command.CanExecute(param))
                            command.Execute(param);
                    },
                    onError);
        }

        public MemberInfo GetContainerMember(Type containerType, string eventName)
        {
            return GetCachedContainerMetadata(containerType, eventName)?.Event;
        }
    }
}
