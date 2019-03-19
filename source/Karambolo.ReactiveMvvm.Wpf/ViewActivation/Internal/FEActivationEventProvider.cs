using System;
using System.Reactive.Linq;
using System.Windows;
using Karambolo.Common;

namespace Karambolo.ReactiveMvvm.ViewActivation.Internal
{
    public sealed class FEActivationEventProvider : IViewActivationEventProvider
    {
        public bool CanProvideFor(IActivableView view)
        {
            return view is FrameworkElement;
        }

        static IObservable<bool> ProduceActivationEvents(FrameworkElement fe)
        {
            return Observable
                .FromEventPattern<RoutedEventHandler, object>(
                    handler => fe.Loaded += handler,
                    handler => fe.Loaded -= handler)
                .Select(True<object>.Func);
        }

        static IObservable<bool> ProduceDeactivationEvents(FrameworkElement fe)
        {
            var events = Observable
                .FromEventPattern<RoutedEventHandler, object>(
                    handler => fe.Unloaded += handler,
                    handler => fe.Unloaded -= handler);

            var window = Window.GetWindow(fe);
            if (window != null)
            {
                var rootClosedEvents = Observable
                    .FromEventPattern(
                        handler => window.Closed += handler,
                        handler => window.Closed -= handler);

                events = events.Merge(rootClosedEvents);
            }

            return events.Select(False<object>.Func);
        }

        public IObservable<bool> GetActivationEvents(IActivableView view)
        {
            var fe = (FrameworkElement)view;

            return Observable
                .Return(false)
                .Expand(isActivated => (!isActivated ? ProduceActivationEvents(fe) : ProduceDeactivationEvents(fe)).FirstAsync())
                .Select(isActivated => isActivated ?
                    fe.WhenChange(e => e.IsHitTestVisible).Select(value => value.GetValueOrDefault()) :
                    ReactiveMvvm.Internal.False.Observable)
                .Switch()
                .DistinctUntilChanged();
        }
    }
}
