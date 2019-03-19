using System;

namespace Karambolo.ReactiveMvvm.ViewActivation.Internal
{
    public interface IViewActivationService
    {
        IDisposable EnableViewActivation(IActivableView view);
    }
}
