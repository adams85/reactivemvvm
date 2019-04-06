using System;
using System.Reactive.Concurrency;
using System.Reflection;
using Karambolo.ReactiveMvvm.ErrorHandling;
using Karambolo.ReactiveMvvm.Internal.Platform;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Karambolo.ReactiveMvvm
{
    public static class ReactiveMvvmBuilderExtensions
    {
        public static IReactiveMvvmBuilder ConfigureServices(this IReactiveMvvmBuilder builder, Action<IServiceCollection> configure)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            configure(builder.Services);

            return builder;
        }

        public static IReactiveMvvmBuilder ConfigureServices(this IReactiveMvvmBuilder builder, Action<IServiceCollection, Type> configure, params Assembly[] assemblies)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            if (configure == null)
                throw new ArgumentNullException(nameof(configure));

            return builder.ConfigureServices(
                (services, types) =>
                {
                    foreach (Type type in types)
                        configure(services, type);
                },
                assemblies);
        }

        public static IReactiveMvvmBuilder ConfigureOptions(this IReactiveMvvmBuilder builder, Action<ReactiveMvvmOptions> configure)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.Configure(configure);

            return builder;
        }

        public static IReactiveMvvmBuilder ConfigureLogging(this IReactiveMvvmBuilder builder, Action<ILoggingBuilder> configure)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.AddLogging(configure);

            return builder;
        }

        public static IReactiveMvvmBuilder EnableUnitTesting(this IReactiveMvvmBuilder builder, IScheduler mainThreadScheduler, IScheduler taskPoolScheduler)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IPlatformSchedulerProvider>(_ => new TestPlatformSchedulerProvider(mainThreadScheduler, taskPoolScheduler)));

            return builder;
        }

        public static IReactiveMvvmBuilder UseErrorHandler(this IReactiveMvvmBuilder builder, IDefaultObservedErrorHandlerFactory handler)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            builder.Services.Replace(ServiceDescriptor.Singleton(handler));

            return builder;
        }
    }
}
