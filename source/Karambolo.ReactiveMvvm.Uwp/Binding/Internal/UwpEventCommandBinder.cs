using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;

namespace Karambolo.ReactiveMvvm.Binding.Internal
{
    public class UwpEventCommandBinder : EventCommandBinder
    {
        protected override IObservable<EventPattern<TEventArgs>> GetEvents<TEventHandler, TEventArgs>(object container, ContainerMetadata containerMetadata)
        {
            // UWP events are non-standard ("Adding or removing event handlers dynamically is not supported on WinRT events."),
            // so we resort to Rx to deal with subscription/unsubscription.
            return Observable.FromEventPattern<TEventArgs>(container, containerMetadata.Event.Name);
        }
    }
}
