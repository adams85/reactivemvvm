using System;
using System.Reactive;
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
        static readonly Func<IReactiveMvvmContext> s_getCurrentCached = () => ServiceProvider.GetRequiredService<IReactiveMvvmContext>();
        static IReactiveMvvmContext s_current;
        public static IReactiveMvvmContext Current => LazyInitializer.EnsureInitialized(ref s_current, s_getCurrentCached);

        static int s_serviceProviderBuilt;
        static IServiceProvider s_serviceProvider;

        public static IServiceProvider ServiceProvider
        {
            get
            {
                if (Interlocked.CompareExchange(ref s_serviceProviderBuilt, 1, 0) != 0)
                    return s_serviceProvider;
                else
                    return s_serviceProvider ?? (s_serviceProvider = BuildServiceProvider(Noop<IReactiveMvvmBuilder>.Action));
            }
            private set
            {
                if (Volatile.Read(ref s_serviceProviderBuilt) == 0)
                    s_serviceProvider = value;
                else
                    throw new InvalidOperationException(Resources.InitializationNotPossible);
            }
        }

        static IServiceProvider BuildServiceProvider(Action<IReactiveMvvmBuilder> configure)
        {
            var builder = new ServiceCollection().AddReactiveMvvm();
            configure(builder);
            return builder.Services.BuildServiceProvider();
        }

        public static IServiceProvider Initialize(Action<IReactiveMvvmBuilder> configure)
        {
            return ServiceProvider = BuildServiceProvider(configure);
        }

        public static IServiceProvider Initialize(IServiceProvider serviceProvider)
        {
            return ServiceProvider = serviceProvider;
        }

        static int s_recommendVerifyingInitialization;
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
