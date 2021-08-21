using System;
using System.Reactive.Linq;
using Karambolo.ReactiveMvvm.Internal;
using Windows.UI.Xaml;

namespace Karambolo.ReactiveMvvm.ViewActivation.Internal
{
    public sealed class FEActivationEventProvider : IViewActivationEventProvider
    {
        private enum ActivationState
        {
            WaitForActivation,
            Activated,
            SentToBackground,
        }

        public bool CanProvideFor(IActivableView view)
        {
            return view is FrameworkElement;
        }

        private IObservable<ActivationState> ProduceActivationEvents(ActivationState state, FrameworkElement fe)
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
                    IObservable<ActivationState> unloadedEvents = Observable
                        .FromEventPattern<RoutedEventHandler, object>(
                            handler => fe.Unloaded += handler,
                            handler => fe.Unloaded -= handler)
                        .Select(_ => ActivationState.WaitForActivation);

                    IObservable<ActivationState> enteredBackgroundEvents = Observable
#if !UNO
                        .FromEventPattern<EnteredBackgroundEventHandler, object>(
                            handler => Application.Current.EnteredBackground += handler,
                            handler => Application.Current.EnteredBackground -= handler)
#else
                        .FromEventPattern<SuspendingEventHandler, object>(
                            handler => Application.Current.Suspending += handler,
                            handler => Application.Current.Suspending -= handler)
#endif
                        .Select(_ => ActivationState.SentToBackground);

                    return Observable.Merge(unloadedEvents, enteredBackgroundEvents);

                case ActivationState.SentToBackground:
                    return Observable
#if !UNO
                        .FromEventPattern<LeavingBackgroundEventHandler, object>(
                            handler => Application.Current.LeavingBackground += handler,
                            handler => Application.Current.LeavingBackground -= handler)
#else
                        .FromEventPattern<object>(
                            handler => Application.Current.Resuming += handler,
                            handler => Application.Current.Resuming -= handler)
#endif
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
                    CachedObservables.False.Observable)
                .Switch()
                .DistinctUntilChanged();
        }
    }
}
