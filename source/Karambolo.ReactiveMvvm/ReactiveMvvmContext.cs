using System;
using System.Reactive.Concurrency;
using System.Threading;
using Karambolo.Common;
using Karambolo.ReactiveMvvm.ErrorHandling;
using Karambolo.ReactiveMvvm.Internal.Platform;
using Karambolo.ReactiveMvvm.Properties;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Karambolo.ReactiveMvvm
{
    public class ReactiveMvvmContext : IReactiveMvvmContext
    {
        private static readonly Func<IReactiveMvvmContext> s_getCurrentCached = () => ServiceProvider.GetRequiredService<IReactiveMvvmContext>();
        private static IReactiveMvvmContext s_current;
        public static IReactiveMvvmContext Current => LazyInitializer.EnsureInitialized(ref s_current, s_getCurrentCached);

        private static readonly Func<IServiceProvider> s_defaultServiceProviderFactory = () => BuildServiceProvider(Noop<IReactiveMvvmBuilder>.Action);
        private static Func<IServiceProvider> s_serviceProviderFactory = s_defaultServiceProviderFactory;
        private static IServiceProvider s_serviceProvider;

        public static IServiceProvider ServiceProvider
        {
            get
            {
                IServiceProvider serviceProvider;

                Func<IServiceProvider> serviceProviderFactory = Interlocked.Exchange(ref s_serviceProviderFactory, null);
                if (serviceProviderFactory != null)
                {
                    serviceProvider = serviceProviderFactory();
                    Volatile.Write(ref s_serviceProvider, serviceProvider);
                    return serviceProvider;
                }
                else
                    for (; ; )
                        if ((serviceProvider = Volatile.Read(ref s_serviceProvider)) != null)
                            return serviceProvider;
            }
            private set
            {
                Func<IServiceProvider> newServiceProviderFactory = value != null ? () => value : s_defaultServiceProviderFactory;

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
            ServiceProvider = BuildServiceProvider(configure);
            return ServiceProvider;
        }

        public static IServiceProvider Initialize(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            return ServiceProvider;
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
