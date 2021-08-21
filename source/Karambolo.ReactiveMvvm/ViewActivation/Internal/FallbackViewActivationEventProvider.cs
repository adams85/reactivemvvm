using System;
using Karambolo.ReactiveMvvm.Internal;

namespace Karambolo.ReactiveMvvm.ViewActivation.Internal
{
    public sealed class FallbackViewActivationEventProvider : IViewActivationEventProvider
    {
        public static readonly FallbackViewActivationEventProvider Instance = new FallbackViewActivationEventProvider();

        private FallbackViewActivationEventProvider() { }

        public bool CanProvideFor(IActivableView view)
        {
            return true;
        }

        public IObservable<bool> GetActivationEvents(IActivableView view)
        {
            return CachedObservables.Never<bool>.Observable;
        }
    }
}
