using System;
using System.Reactive.Linq;
using Karambolo.Common;
using Karambolo.ReactiveMvvm.Internal;
using Microsoft.UI.Xaml;

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
            return Observable
                .FromEventPattern<RoutedEventHandler, object>(
                    handler => fe.Unloaded += handler,
                    handler => fe.Unloaded -= handler)
                .Select(CachedDelegates.False<object>.Func);
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
