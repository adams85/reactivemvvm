﻿using System;
using Karambolo.ReactiveMvvm.Internal;

namespace Karambolo.ReactiveMvvm.ViewActivation.Internal
{
    public sealed class FallbackViewActivationEventProvider : IViewActivationEventProvider
    {
        public static readonly FallbackViewActivationEventProvider Instance = new FallbackViewActivationEventProvider();

        FallbackViewActivationEventProvider() { }

        public bool CanProvideFor(IActivableView view)
        {
            return true;
        }

        public IObservable<bool> GetActivationEvents(IActivableView view)
        {
            return Never<bool>.Observable;
        }
    }
}
