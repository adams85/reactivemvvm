using System;

namespace Karambolo.ReactiveMvvm.ViewActivation.Internal
{
    public interface IViewActivationEventProvider
    {
        bool CanProvideFor(IActivableView view);
        IObservable<bool> GetActivationEvents(IActivableView view);
    }
}
