using System;
using System.Reactive.Concurrency;
using Karambolo.ReactiveMvvm.Internal;
using Karambolo.ReactiveMvvm.Internal.Concurrency;
using Karambolo.ReactiveMvvm.Internal.Platform;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Karambolo.ReactiveMvvm
{
    public static class AvaloniaReactiveMvvmBuilderExtensions
    {
        public static IReactiveMvvmBuilder UseAvalonia(this IReactiveMvvmBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            return builder.UseAvalonia(AvaloniaSchedulerProvider.DefaultUIThreadScheduler);
        }

        public static IReactiveMvvmBuilder UseAvalonia(this IReactiveMvvmBuilder builder, IScheduler mainThreadScheduler)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            if (mainThreadScheduler == null)
                throw new ArgumentNullException(nameof(mainThreadScheduler));

            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<ReactiveMvvmOptions>, AvaloniaReactiveMvvmOptionsSetup>());

            builder.Services.Replace(ServiceDescriptor.Singleton<IPlatformSchedulerProvider>(_ => new AvaloniaSchedulerProvider(mainThreadScheduler)));

            return builder;
        }

        public static IReactiveMvvmBuilder UseAvalonia(this IReactiveMvvmBuilder builder, IObservable<IScheduler> mainThreadSchedulers)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            if (mainThreadSchedulers == null)
                throw new ArgumentNullException(nameof(mainThreadSchedulers));

            return builder.UseAvalonia(new VariableScheduler(mainThreadSchedulers));
        }
    }
}
