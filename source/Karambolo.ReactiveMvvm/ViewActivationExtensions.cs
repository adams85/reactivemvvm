using System;
using Karambolo.ReactiveMvvm.ViewActivation.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Karambolo.ReactiveMvvm
{
    public static class ViewActivationExtensions
    {
        private static readonly IViewActivationService s_viewActivationService = ReactiveMvvmContext.ServiceProvider.GetRequiredService<IViewActivationService>();

        public static IDisposable EnableViewActivation(this IActivableView view)
        {
            return s_viewActivationService.EnableViewActivation(view);
        }
    }
}
