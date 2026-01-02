using System;
using System.Collections.Concurrent;
using System.Linq;
using Karambolo.Common;
using Microsoft.Extensions.DependencyInjection;

namespace Karambolo.ReactiveMvvm.Internal
{
    public class DefaultViewModelFactory : IViewModelFactory
    {
        private static readonly ConcurrentDictionary<(Type, Type[]), ObjectFactory> s_factoryCache = new ConcurrentDictionary<(Type, Type[]), ObjectFactory>();
        private readonly IServiceProvider _serviceProvider;

        public DefaultViewModelFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private static ObjectFactory GetCachedFactory<TViewModel>(object[] parameters) where TViewModel : class
        {
            Type[] paramTypes = !ArrayUtils.IsNullOrEmpty(parameters) ? parameters.Select(param => param?.GetType()).ToArray() : Type.EmptyTypes;

            return s_factoryCache.GetOrAdd((typeof(TViewModel), paramTypes), key => ActivatorUtilities.CreateFactory(key.Item1, key.Item2));
        }

        public TViewModel CreateViewModel<TViewModel>(params object[] parameters) where TViewModel : class
        {
            return (TViewModel)GetCachedFactory<TViewModel>(parameters)(_serviceProvider, parameters);
        }

        public TViewModel CreateViewModelScoped<TViewModel>(object[] parameters = null)
            where TViewModel : class, ILifetime
        {
            IServiceScope scope = _serviceProvider.CreateScope();
            TViewModel viewModel = null;
            try
            {
                viewModel = (TViewModel)GetCachedFactory<TViewModel>(parameters)(scope.ServiceProvider, parameters);
                scope.AttachTo(viewModel);
                return viewModel;
            }
            catch
            {
                viewModel?.Dispose();
                scope.Dispose();
                throw;
            }
        }
    }
}
