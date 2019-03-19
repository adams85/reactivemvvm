using System;
using System.Diagnostics;
using System.Reactive.Linq;
using Karambolo.ReactiveMvvm.Internal;
using Windows.UI.Xaml;

namespace Karambolo.ReactiveMvvm.ViewActivation.Internal
{
    public sealed class FEActivationEventProvider : IViewActivationEventProvider
    {
        enum ActivationState
        {
            WaitForActivation,
            Activated,
            SentToBackground,
        }

        public bool CanProvideFor(IActivableView view)
        {
            return view is FrameworkElement;
        }

        IObservable<ActivationState> ProduceActivationEvents(ActivationState state, FrameworkElement fe)
        {
            // UWP app lifecycle:
            // https://docs.microsoft.com/en-us/windows/uwp/launch-resume/app-lifecycle

            switch (state)
            {
                case ActivationState.WaitForActivation:
                    return Observable
                        .FromEventPattern<RoutedEventHandler, object>(
                            handler => fe.Loaded += handler,
                            handler => fe.Loaded -= handler)
                        .Select(_ => ActivationState.Activated);

                case ActivationState.Activated:
                    var unloadedEvents = Observable
                        .FromEventPattern<RoutedEventHandler, object>(
                            handler => fe.Unloaded += handler,
                            handler => fe.Unloaded -= handler)
                        .Select(_ => ActivationState.WaitForActivation);

                    var enteredBackgroundEvents = Observable
                        .FromEventPattern<EnteredBackgroundEventHandler, object>(
                            handler => Application.Current.EnteredBackground += handler,
                            handler => Application.Current.EnteredBackground -= handler)
                        .Select(_ => ActivationState.SentToBackground);

                    return Observable.Merge(unloadedEvents, enteredBackgroundEvents);

                case ActivationState.SentToBackground:
                    return Observable
                        .FromEventPattern<LeavingBackgroundEventHandler, object>(
                            handler => Application.Current.LeavingBackground += handler,
                            handler => Application.Current.LeavingBackground -= handler)
                        .Select(_ => ActivationState.Activated);

                default:
                    throw new InvalidOperationException();
            }
        }

        public IObservable<bool> GetActivationEvents(IActivableView view)
        {
            var fe = (FrameworkElement)view;

            return Observable
                .Return(ActivationState.WaitForActivation)
                .Expand(state => ProduceActivationEvents(state, fe).FirstAsync())
                .Select(state => state == ActivationState.Activated ?
                    fe.WhenChange(e => e.IsHitTestVisible).Select(value => value.GetValueOrDefault()) :
                    False.Observable)
                .Switch()
                .DistinctUntilChanged();
        }
    }
}
