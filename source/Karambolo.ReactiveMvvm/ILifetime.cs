using System;

namespace Karambolo.ReactiveMvvm
{
    public interface ILifetime : IDisposable
    {
        void AttachDisposable(IDisposable disposable);
        void DetachDisposable(IDisposable disposable);
    }
}
