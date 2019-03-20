using System;
using Karambolo.ReactiveMvvm.ViewActivation.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Karambolo.ReactiveMvvm
{
    public static class ViewActivationExtensions
    {
        static readonly IViewActivationService _viewActivationService = ReactiveMvvmContext.ServiceProvider.GetRequiredService<IViewActivationService>();

        public static IDisposable EnableViewActivation(this IActivableView view)
        {
            return _viewActivationService.EnableViewActivation(view);
        }
    }
}
