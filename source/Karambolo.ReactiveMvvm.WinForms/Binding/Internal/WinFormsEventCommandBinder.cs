﻿using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reflection;
using System.Windows.Input;
using Karambolo.ReactiveMvvm.Internal;

namespace Karambolo.ReactiveMvvm.Binding.Internal
{
    public class WinFormsEventCommandBinder : EventCommandBinder
    {
        protected new class ContainerMetadata : EventCommandBinder.ContainerMetadata
        {
            public PropertyInfo EnabledProperty;
        }

        protected override EventCommandBinder.ContainerMetadata CreateContainerMetadata(Type containerType, EventInfo @event, Type eventHandlerType, Type eventArgType)
        {
            return new ContainerMetadata
            {
                Event = @event,
                EventHandlerType = eventHandlerType,
                EventArgType = eventArgType,
                EnabledProperty =
                    typeof(Component).IsAssignableFrom(containerType) ?
                    containerType.GetProperty("Enabled", typeof(bool)) :
                    null
            };
        }

        protected override IDisposable Bind<TEventHandler, TEventArgs, TParam>(ICommand command, object container, IObservable<TParam> commandParameters, EventCommandBinder.ContainerMetadata containerMetadata, 
            IScheduler scheduler, Action<Exception> onError)
        {
            var actualContainerMetadata = (ContainerMetadata)containerMetadata;

            var commandBindingDisposable = base.Bind<TEventHandler, TEventArgs, TParam>(command, container, commandParameters, actualContainerMetadata, scheduler, onError);

            if (actualContainerMetadata.EnabledProperty == null)
                return commandBindingDisposable;

            var originalEnabled = actualContainerMetadata.EnabledProperty.GetValue(container, null);

            var enabledBindingSubscription = Observable.FromEventPattern(handler => command.CanExecuteChanged += handler, handler => command.CanExecuteChanged -= handler)
                .StartWith(default(EventPattern<object>))
                .WithLatestFrom(commandParameters.StartWith(default(TParam)), (_, param) => param)
                .Select(param => command.CanExecute(param))
                .ObserveOnSafe(scheduler)
                .Subscribe(
                    canExec => actualContainerMetadata.EnabledProperty.SetValue(container, canExec, null),
                    onError);

            var restoreDisposable = Disposable.Create(() => actualContainerMetadata.EnabledProperty.SetValue(container, originalEnabled, null));

            return new CompositeDisposable
            {
                commandBindingDisposable,
                enabledBindingSubscription,
                restoreDisposable,
            };
        }
    }
}
