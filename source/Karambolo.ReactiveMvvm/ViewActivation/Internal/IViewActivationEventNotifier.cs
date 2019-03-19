using System;

namespace Karambolo.ReactiveMvvm.ViewActivation.Internal
{
    public interface IViewActivationEventNotifier
    {
        IObservable<bool> WhenActivationStateChanged { get; }
    }
}
