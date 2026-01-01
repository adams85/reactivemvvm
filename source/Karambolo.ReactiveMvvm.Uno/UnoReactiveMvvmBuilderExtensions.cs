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
    public static class UnoReactiveMvvmBuilderExtensions
    {
        public static IReactiveMvvmBuilder UseUno(this IReactiveMvvmBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            return builder.UseUno(UnoSchedulerProvider.CaptureUIThreadScheduler());
        }

        public static IReactiveMvvmBuilder UseUno(this IReactiveMvvmBuilder builder, IScheduler mainThreadScheduler)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            if (mainThreadScheduler == null)
                throw new ArgumentNullException(nameof(mainThreadScheduler));

            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<ReactiveMvvmOptions>, UnoReactiveMvvmOptionsSetup>());

            builder.Services.Replace(ServiceDescriptor.Singleton<IPlatformSchedulerProvider>(_ => new UnoSchedulerProvider(mainThreadScheduler)));

            return builder;
        }

        public static IReactiveMvvmBuilder UseUno(this IReactiveMvvmBuilder builder, IObservable<IScheduler> mainThreadSchedulers)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            if (mainThreadSchedulers == null)
                throw new ArgumentNullException(nameof(mainThreadSchedulers));

            return builder.UseUno(new VariableScheduler(mainThreadSchedulers));
        }
    }
}
