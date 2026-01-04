using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Karambolo.Common;
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
        private sealed class Builder : IReactiveMvvmBuilder
        {
            private readonly Dictionary<Assembly, Type[]> _assemblyTypes;

            public Builder(IServiceCollection services)
            {
                Services = services;
                _assemblyTypes = new Dictionary<Assembly, Type[]>();
            }

            public IServiceCollection Services { get; }

#if NET5_0_OR_GREATER
            [RequiresUnreferencedCode(ReactiveMvvmBuilderExtensions.AssemblyTypesMayBeTrimmedMessage)]
#endif
            public IReactiveMvvmBuilder ConfigureServices(Action<IServiceCollection, IEnumerable<Type>> configure, params Assembly[] assemblies)
            {
                if (configure == null)
                    throw new ArgumentNullException(nameof(configure));

                if (!ArrayUtils.IsNullOrEmpty(assemblies))
                {
                    IEnumerable<Type> allTypes = assemblies
                        .SelectMany(assembly =>
                        {
                            if (!_assemblyTypes.TryGetValue(assembly, out Type[] types))
                                _assemblyTypes.Add(assembly, types = assembly.GetTypes());
                            return types;
                        });

                    configure(Services, allTypes);
                }

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
