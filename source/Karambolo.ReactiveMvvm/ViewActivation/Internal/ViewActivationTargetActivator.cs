using System;
using System.Reactive.Disposables;

namespace Karambolo.ReactiveMvvm.ViewActivation.Internal
{
    sealed class ViewActivationTargetActivator
    {
        readonly IViewActivationTarget _target;
        readonly SerialDisposable _activationLifetimeSerial; // serves as lock object, as well

        int _refCount;

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

        void Deactivate()
        {
            lock (_activationLifetimeSerial)
                if (--_refCount == 0)
                    _activationLifetimeSerial.Disposable = null;
        }
    }
}
