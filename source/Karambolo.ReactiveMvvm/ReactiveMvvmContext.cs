using System;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Concurrency;
using System.Runtime.CompilerServices;
using System.Threading;
#if USES_COMMON_PACKAGE
using Karambolo.Common;
#endif
using Karambolo.ReactiveMvvm.ErrorHandling;
using Karambolo.ReactiveMvvm.Internal;
using Karambolo.ReactiveMvvm.Internal.Platform;
using Karambolo.ReactiveMvvm.Properties;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Karambolo.ReactiveMvvm
{
    public class ReactiveMvvmContext : IReactiveMvvmContext
    {
        // About feature switches: https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9/runtime#attribute-model-for-feature-switches-with-trimming-support

#if NET9_0_OR_GREATER
        // This should be a good enough heuristic to detect whether trimming is disabled or enabled.
        // See also: https://learn.microsoft.com/en-us/dotnet/core/deploying/trimming/trimming-options#framework-features-disabled-when-trimming
        [FeatureSwitchDefinition("System.StartupHookProvider.IsSupported")]
        [FeatureGuard(typeof(RequiresUnreferencedCodeAttribute))]
#endif
        internal static bool IsUntrimmed { get; } = !AppContext.TryGetSwitch("System.StartupHookProvider.IsSupported", out bool isSupported) || isSupported;

#if NET9_0_OR_GREATER
        [FeatureGuard(typeof(RequiresDynamicCodeAttribute))]
#endif
        internal static bool IsDynamicCodeCompiled { get; } =
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP3_0_OR_GREATER
            RuntimeFeature.IsDynamicCodeCompiled;
#else
            !AppContext.TryGetSwitch("System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported", out bool isDynamicCodeSupported) || isDynamicCodeSupported;
#endif

        private static readonly Func<IReactiveMvvmContext> s_getCurrentCached = () => ServiceProvider.GetRequiredService<IReactiveMvvmContext>();
        private static IReactiveMvvmContext s_current;
        public static IReactiveMvvmContext Current => LazyInitializer.EnsureInitialized(ref s_current, s_getCurrentCached);

        private static Func<IServiceProvider> s_serviceProviderFactory = () => BuildServiceProvider(CachedDelegates.Noop<IReactiveMvvmBuilder>.Action);
        private static IServiceProvider s_serviceProvider;

        public static IServiceProvider ServiceProvider
        {
            get
            {
                IServiceProvider serviceProvider;

                Func<IServiceProvider> serviceProviderFactory = Interlocked.Exchange(ref s_serviceProviderFactory, null);
                if (serviceProviderFactory == null)
                {
                    for (; ; )
                        if ((serviceProvider = Volatile.Read(ref s_serviceProvider)) != null)
                            return serviceProvider;
                }
                else
                {
                    serviceProvider = serviceProviderFactory();
                    Volatile.Write(ref s_serviceProvider, serviceProvider);
                    return serviceProvider;
                }
            }
            private set
            {
                Func<IServiceProvider> newServiceProviderFactory = () => value;

                Func<IServiceProvider> originalServiceProviderFactory = Volatile.Read(ref s_serviceProviderFactory);
                for (; ; )
                {
                    if (originalServiceProviderFactory == null)
                        throw new InvalidOperationException(Resources.InitializationNotPossible);

                    Func<IServiceProvider> currentServiceProviderFactory =
                        Interlocked.CompareExchange(ref s_serviceProviderFactory, newServiceProviderFactory, originalServiceProviderFactory);

                    if (originalServiceProviderFactory == currentServiceProviderFactory)
                        return;

                    originalServiceProviderFactory = currentServiceProviderFactory;
                }
            }
        }

        private static IServiceProvider BuildServiceProvider(Action<IReactiveMvvmBuilder> configure)
        {
            IReactiveMvvmBuilder builder = new ServiceCollection().AddReactiveMvvm();
            configure(builder);
            return builder.Services.BuildServiceProvider();
        }

        public static IServiceProvider Initialize(Action<IReactiveMvvmBuilder> configure)
        {
            return ServiceProvider = BuildServiceProvider(configure);
        }

        public static IServiceProvider Initialize(IServiceProvider serviceProvider)
        {
            return ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        private static int s_recommendVerifyingInitialization;
        internal static void RecommendVerifyingInitialization(ILogger logger)
        {
            if (Interlocked.CompareExchange(ref s_recommendVerifyingInitialization, 1, 0) == 0)
                logger.LogInformation(Resources.VerifyPlatformRegistration, nameof(ReactiveMvvmContext), nameof(Initialize));
        }

        public ReactiveMvvmContext(IPlatformSchedulerProvider platformSchedulers, IDefaultObservedErrorHandlerFactory defaultErrorHandlerFactory,
            IMessageBus messageBus, IViewModelFactory viewModelFactory, ILoggerFactory loggerFactory)
        {
            MainThreadScheduler = platformSchedulers.MainThreadScheduler;
            TaskPoolScheduler = platformSchedulers.TaskPoolScheduler;

            DefaultErrorHandler = defaultErrorHandlerFactory.Create();

            MessageBus = messageBus;

            ViewModelFactory = viewModelFactory;

            LoggerFactory = loggerFactory;
        }

        public IScheduler MainThreadScheduler { get; }
        public IScheduler TaskPoolScheduler { get; }

        public ObservedErrorHandler DefaultErrorHandler { get; }

        public IMessageBus MessageBus { get; }

        public IViewModelFactory ViewModelFactory { get; }

        public ILoggerFactory LoggerFactory { get; }
    }
}
