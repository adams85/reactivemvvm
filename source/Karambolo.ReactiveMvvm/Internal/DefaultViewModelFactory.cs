using System;
using Microsoft.Extensions.DependencyInjection;

namespace Karambolo.ReactiveMvvm.Internal
{
    public class DefaultViewModelFactory : IViewModelFactory
    {
        readonly IServiceProvider _serviceProvider;

        public DefaultViewModelFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public TViewModel CreateViewModel<TViewModel>(bool withScope = true)
            where TViewModel : class, ILifetime
        {
            IServiceProvider serviceProvider;
            IServiceScope scope;

            if (withScope)
            {
                scope = _serviceProvider.CreateScope();
                serviceProvider = scope.ServiceProvider;
            }
            else
            {
                scope = null;
                serviceProvider = _serviceProvider;
            }

            var viewModel = serviceProvider.GetRequiredService<TViewModel>();
            try
            {
                if (scope != null)
                    viewModel.AttachDisposable(scope);

                return viewModel;
            }
            catch
            {
                viewModel.Dispose();
                throw;
            }
        }
    }
}
