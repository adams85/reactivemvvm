using System;
using System.Reactive.Linq;
using System.Windows;
using Karambolo.Common;
using Karambolo.ReactiveMvvm.Internal;

namespace Karambolo.ReactiveMvvm.ViewActivation.Internal
{
    public sealed class FEActivationEventProvider : IViewActivationEventProvider
    {
        public bool CanProvideFor(IActivableView view)
        {
            return view is FrameworkElement;
        }

        private static IObservable<bool> ProduceActivationEvents(FrameworkElement fe)
        {
            return Observable
                .FromEventPattern<RoutedEventHandler, object>(
                    handler => fe.Loaded += handler,
                    handler => fe.Loaded -= handler)
                .Select(CachedDelegates.True<object>.Func);
        }

        private static IObservable<bool> ProduceDeactivationEvents(FrameworkElement fe)
        {
            IObservable<System.Reactive.EventPattern<object>> events = Observable
                .FromEventPattern<RoutedEventHandler, object>(
                    handler => fe.Unloaded += handler,
                    handler => fe.Unloaded -= handler);

            var window = Window.GetWindow(fe);
            if (window != null)
            {
                IObservable<System.Reactive.EventPattern<object>> rootClosedEvents = Observable
                    .FromEventPattern(
                        handler => window.Closed += handler,
                        handler => window.Closed -= handler);

                events = events.Merge(rootClosedEvents);
            }

            return events.Select(CachedDelegates.False<object>.Func);
        }

        public IObservable<bool> GetActivationEvents(IActivableView view)
        {
            var fe = (FrameworkElement)view;

            return Observable
                .Return(false)
                .Expand(isActivated => (!isActivated ? ProduceActivationEvents(fe) : ProduceDeactivationEvents(fe)).FirstAsync())
                .Select(isActivated => isActivated ?
                    fe.WhenChange(e => e.IsHitTestVisible).Select(value => value.GetValueOrDefault()) :
                    CachedObservables.False.Observable)
                .Switch()
                .DistinctUntilChanged();
        }
    }
}
