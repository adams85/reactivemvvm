using System;
using System.Reactive.Disposables;

namespace Karambolo.ReactiveMvvm
{
    public sealed class ViewActivationLifetime : ILifetime
    {
        readonly CompositeDisposable _disposables = new CompositeDisposable();

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public void AttachDisposable(IDisposable disposable)
        {
            _disposables.Add(disposable);
        }

        public void DetachDisposable(IDisposable disposable)
        {
            _disposables.Remove(disposable);
        }
    }
}
