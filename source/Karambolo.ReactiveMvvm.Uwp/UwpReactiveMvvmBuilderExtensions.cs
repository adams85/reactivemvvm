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
    public static class UwpReactiveMvvmBuilderExtensions
    {
        public static IReactiveMvvmBuilder UseUwp(this IReactiveMvvmBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            return builder.UseUwp(UwpSchedulerProvider.CaptureUIThreadScheduler());
        }

        public static IReactiveMvvmBuilder UseUwp(this IReactiveMvvmBuilder builder, IScheduler mainThreadScheduler)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            if (mainThreadScheduler == null)
                throw new ArgumentNullException(nameof(mainThreadScheduler));

            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<ReactiveMvvmOptions>, UwpReactiveMvvmOptionsSetup>());

            builder.Services.Replace(ServiceDescriptor.Singleton<IPlatformSchedulerProvider>(_ => new UwpSchedulerProvider(mainThreadScheduler)));

            return builder;
        }

        public static IReactiveMvvmBuilder UseUwp(this IReactiveMvvmBuilder builder, IObservable<IScheduler> mainThreadSchedulers)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            if (mainThreadSchedulers == null)
                throw new ArgumentNullException(nameof(mainThreadSchedulers));

            return builder.UseUwp(new VariableScheduler(mainThreadSchedulers));
        }
    }
}
