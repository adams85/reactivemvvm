using System;
using System.Reactive.Disposables;

namespace Karambolo.ReactiveMvvm.ViewActivation.Internal
{
    internal sealed class ViewActivationTargetActivator
    {
        private readonly IViewActivationTarget _target;
        private readonly SerialDisposable _activationLifetimeSerial; // serves as lock object, as well

        private int _refCount;

        public ViewActivationTargetActivator(IViewActivationTarget target)
        {
            _target = target;
            _activationLifetimeSerial = new SerialDisposable();
        }

        public IDisposable Activate()
        {
            bool doActivate;
            ViewActivationLifetime activationLifetime = null;

            lock (_activationLifetimeSerial)
                if (doActivate = _refCount++ == 0)
                    _activationLifetimeSerial.Disposable = activationLifetime = new ViewActivationLifetime();

            if (doActivate)
                _target.OnViewActivated(activationLifetime);

            return Disposable.Create(Deactivate);
        }

        private void Deactivate()
        {
            lock (_activationLifetimeSerial)
                if (--_refCount == 0)
                    _activationLifetimeSerial.Disposable = null;
        }
    }
}
