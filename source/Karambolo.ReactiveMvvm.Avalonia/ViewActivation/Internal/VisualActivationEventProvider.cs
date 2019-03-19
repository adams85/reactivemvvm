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
            return view is IVisual;
        }

        static IObservable<bool> ProduceActivationEvents(TopLevel topLevel)
        {
            // TODO: use window.Opened when gets available (the nightly builds already contains it)
            return Observable
                .FromEventPattern(
                    handler => topLevel.Initialized += handler,
                    handler => topLevel.Initialized -= handler)
                .Select(True<object>.Func);
        }

        static IObservable<bool> ProduceDeactivationEvents(TopLevel topLevel)
        {
            return Observable
                .FromEventPattern(
                    handler => topLevel.Closed += handler,
                    handler => topLevel.Closed -= handler)
                .Select(False<object>.Func);
        }

        static IObservable<bool> ProduceActivationEvents(IVisual visual)
        {
            return Observable
                .FromEventPattern<VisualTreeAttachmentEventArgs>(
                    handler => visual.AttachedToVisualTree += handler,
                    handler => visual.AttachedToVisualTree -= handler)
                .Select(True<object>.Func);
        }

        static IObservable<bool> ProduceDeactivationEvents(IVisual visual)
        {
            IObservable<object> events = Observable
                .FromEventPattern<VisualTreeAttachmentEventArgs>(
                    handler => visual.DetachedFromVisualTree += handler,
                    handler => visual.DetachedFromVisualTree -= handler);

            if (visual.VisualRoot is TopLevel root)
            {
                var rootClosedEvents = Observable.FromEventPattern(
                    handler => root.Closed += handler,
                    handler => root.Closed -= handler);

                events = events.Merge(rootClosedEvents);
            }

            return events.Select(False<object>.Func);
        }

        public IObservable<bool> GetActivationEvents(IActivableView view)
        {
            var visual = (IVisual)view;

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
                    ReactiveMvvm.Internal.False.Observable)
                .Switch()
                .DistinctUntilChanged();
        }
    }
}
