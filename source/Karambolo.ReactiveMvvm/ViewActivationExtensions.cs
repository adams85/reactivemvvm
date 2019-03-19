using System;
using Karambolo.ReactiveMvvm.ViewActivation.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Karambolo.ReactiveMvvm
{
    public static class ViewActivationExtensions
    {
        static readonly IViewActivationService _viewActivationService = ReactiveMvvmContext.ServiceProvider.GetRequiredService<IViewActivationService>();

        public static IDisposable EnableViewActivationUnattached(this IActivableView view)
        {
            return _viewActivationService.EnableViewActivation(view);
        }

        public static void EnableViewActivation<TView>(this TView view)
            where TView : IActivableView, ILifetime
        {
            view.EnableViewActivationUnattached()
                .AttachTo(view);
        }
    }
}
