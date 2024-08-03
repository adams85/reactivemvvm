using System;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Karambolo.Common;
using Karambolo.ReactiveMvvm.Internal;

namespace Karambolo.ReactiveMvvm.ViewActivation.Internal
{
    public sealed class VisualActivationEventProvider : IViewActivationEventProvider
    {
        public bool CanProvideFor(IActivableView view)
        {
            return view is Visual;
        }

        private static IObservable<bool> ProduceActivationEvents(TopLevel topLevel)
        {
            return Observable
                .FromEventPattern(
                    handler => topLevel.Opened += handler,
                    handler => topLevel.Opened -= handler)
                .Select(CachedDelegates.True<object>.Func);
        }

        private static IObservable<bool> ProduceDeactivationEvents(TopLevel topLevel)
        {
            return Observable
                .FromEventPattern(
                    handler => topLevel.Closed += handler,
                    handler => topLevel.Closed -= handler)
                .Select(CachedDelegates.False<object>.Func);
        }

        private static IObservable<bool> ProduceActivationEvents(Visual visual)
        {
            return Observable
                .FromEventPattern<VisualTreeAttachmentEventArgs>(
                    handler => visual.AttachedToVisualTree += handler,
                    handler => visual.AttachedToVisualTree -= handler)
                .Select(CachedDelegates.True<object>.Func);
        }

        private static IObservable<bool> ProduceDeactivationEvents(Visual visual)
        {
            IObservable<object> events = Observable
                .FromEventPattern<VisualTreeAttachmentEventArgs>(
                    handler => visual.DetachedFromVisualTree += handler,
                    handler => visual.DetachedFromVisualTree -= handler);

            if (visual.GetVisualRoot() is TopLevel root)
            {
                IObservable<System.Reactive.EventPattern<object>> rootClosedEvents = Observable.FromEventPattern(
                    handler => root.Closed += handler,
                    handler => root.Closed -= handler);

                events = events.Merge(rootClosedEvents);
            }

            return events.Select(CachedDelegates.False<object>.Func);
        }

        public IObservable<bool> GetActivationEvents(IActivableView view)
        {
            var visual = (Visual)view;

            Func<IObservable<bool>> produceActivationEvents, produceDeactivationEvents;
            if (visual is TopLevel topLevel)
            {
                produceActivationEvents = () => ProduceActivationEvents(topLevel);
                produceDeactivationEvents = () => ProduceDeactivationEvents(topLevel);
            }
            else
            {
                produceActivationEvents = () => ProduceActivationEvents(visual);
                produceDeactivationEvents = () => ProduceDeactivationEvents(visual);
            }

            return Observable
                .Return(false)
                .Expand(isActivated => (!isActivated ? produceActivationEvents() : produceDeactivationEvents()).FirstAsync())
                .Select(isActivated => isActivated ?
                    visual.WhenChange(e => e.IsVisible).Select(value => value.GetValueOrDefault()) :
                    CachedObservables.False.Observable)
                .Switch()
                .DistinctUntilChanged();
        }
    }
}
