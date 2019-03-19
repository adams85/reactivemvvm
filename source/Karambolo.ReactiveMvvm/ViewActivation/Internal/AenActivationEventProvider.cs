using System;

namespace Karambolo.ReactiveMvvm.ViewActivation.Internal
{
    public sealed class AenActivationEventProvider : IViewActivationEventProvider
    {
        public bool CanProvideFor(IActivableView view)
        {
            return view is IViewActivationEventNotifier;
        }

        public IObservable<bool> GetActivationEvents(IActivableView view)
        {
            return ((IViewActivationEventNotifier)view).WhenActivationStateChanged;
        }
    }
}
