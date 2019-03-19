using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Karambolo.ReactiveMvvm.Binding.Internal;
using Karambolo.ReactiveMvvm.ChangeNotification.Internal;
using Karambolo.ReactiveMvvm.ErrorHandling;
using Karambolo.ReactiveMvvm.ErrorHandling.Internal;
using Karambolo.ReactiveMvvm.Internal;
using Karambolo.ReactiveMvvm.Internal.Platform;
using Karambolo.ReactiveMvvm.ViewActivation.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Karambolo.ReactiveMvvm
{
    public static class ReactiveMvvmServiceCollectionExtensions
    {
        class Builder : IReactiveMvvmBuilder
        {
            readonly Dictionary<Assembly, Type[]> _assemblyTypes;

            public Builder(IServiceCollection services)
            {
                Services = services;
                _assemblyTypes = new Dictionary<Assembly, Type[]>();
            }

            public IServiceCollection Services { get; }

            public IReactiveMvvmBuilder RegisterAssemblyTypes(Func<IEnumerable<Type>, IEnumerable<Type>> filter, Action<IServiceCollection, Type> register, params Assembly[] assemblies)
            {
                if (filter == null)
                    throw new ArgumentNullException(nameof(filter));

                if (register == null)
                    throw new ArgumentNullException(nameof(register));

                if (assemblies == null)
                    throw new ArgumentNullException(nameof(assemblies));

                var allTypes = assemblies
                    .SelectMany(assembly =>
                    {
                        if (!_assemblyTypes.TryGetValue(assembly, out var types))
                            _assemblyTypes.Add(assembly, types = assembly.GetTypes());
                        return types;
                    });

                foreach (var type in filter(allTypes))
                    register(Services, type);

                return this;
            }
        }

        public static IReactiveMvvmBuilder AddReactiveMvvm(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddOptions();

            services.AddLogging();

            services.TryAddSingleton<IReactiveMvvmContext, ReactiveMvvmContext>();

            services.TryAddSingleton<IDefaultObservedErrorHandlerFactory, DefaultObservedErrorHandlerFactory>();

            services.TryAddSingleton<IPlatformSchedulerProvider, FallbackPlatformSchedulerProvider>();

            services.TryAddSingleton<IChainChangeProvider, DefaultChainChangeProvider>();

            services.TryAddSingleton<IMessageBus, DefaultMessageBus>();

            services.TryAddSingleton<IViewActivationService, DefaultViewActivationService>();

            services.TryAddSingleton<IBindingConverterProvider, DefaultBindingConverterProvider>();

            services.TryAddSingleton<ICommandBinderProvider, DefaultCommandBinderProvider>();

            services.TryAddSingleton<IViewModelFactory, DefaultViewModelFactory>();

            services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<ReactiveMvvmOptions>, ReactiveMvvmOptionsSetup>());

            return new Builder(services);
        }
    }
}
