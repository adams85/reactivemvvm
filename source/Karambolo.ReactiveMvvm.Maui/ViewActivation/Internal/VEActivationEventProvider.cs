using System;
using System.Reactive;
using System.Reactive.Linq;
using Karambolo.Common;
using Karambolo.ReactiveMvvm.Internal;
using Microsoft.Maui.Controls;

namespace Karambolo.ReactiveMvvm.ViewActivation.Internal
{
    public sealed class VEActivationEventProvider : IViewActivationEventProvider
    {
        public bool CanProvideFor(IActivableView view)
        {
            return view is VisualElement;
        }

        private static IObservable<bool> ProduceActivationEvents(VisualElement ve)
        {
            IObservable<EventPattern<object>> events;

            if (ve is Page page)
            {
                events = Observable
                    .FromEventPattern<EventHandler, object>(
                        handler => page.Appearing += handler,
                        handler => page.Appearing -= handler);
            }
            else
            {
                events = Observable
                    .FromEventPattern<EventHandler, object>(
                        handler => ve.Loaded += handler,
                        handler => ve.Loaded -= handler);
            }

            return events.Select(CachedDelegates.True<object>.Func);
        }

        private static IObservable<bool> ProduceDeactivationEvents(VisualElement ve)
        {
            IObservable<EventPattern<object>> events;

            if (ve is Page page)
            {
                events = Observable
                    .FromEventPattern<EventHandler, object>(
                        handler => page.Disappearing += handler,
                        handler => page.Disappearing -= handler);
            }
            else
            {
                events = Observable
                    .FromEventPattern<EventHandler, object>(
                        handler => ve.Unloaded += handler,
                        handler => ve.Unloaded -= handler);
            }

            return events.Select(CachedDelegates.False<object>.Func);
        }

        public IObservable<bool> GetActivationEvents(IActivableView view)
        {
            var ve = (VisualElement)view;

            return Observable
                .Return(false)
                .Expand(isActivated => (!isActivated ? ProduceActivationEvents(ve) : ProduceDeactivationEvents(ve)).FirstAsync())
                .Select(isActivated => isActivated ?
                    ve.WhenChange(e => e.IsVisible).Select(value => value.GetValueOrDefault()) :
                    CachedObservables.False.Observable)
                .Switch()
                .DistinctUntilChanged();
        }
    }
}
