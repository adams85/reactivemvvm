using System;
using System.Reactive.Concurrency;
using System.Windows.Forms;
using Karambolo.ReactiveMvvm.Internal;
using Karambolo.ReactiveMvvm.Internal.Concurrency;
using Karambolo.ReactiveMvvm.Internal.Platform;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Karambolo.ReactiveMvvm
{
    public static class WinFormsReactiveMvvmBuilderExtensions
    {
        public static IReactiveMvvmBuilder UseWinForms(this IReactiveMvvmBuilder builder)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            WindowsFormsSynchronizationContext.AutoInstall = true;
            return builder.UseWinForms(new SynchronizationContextScheduler(new WindowsFormsSynchronizationContext()));
        }

        public static IReactiveMvvmBuilder UseWinForms(this IReactiveMvvmBuilder builder, IScheduler mainThreadScheduler)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            if (mainThreadScheduler == null)
                throw new ArgumentNullException(nameof(mainThreadScheduler));

            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<ReactiveMvvmOptions>, WinFormsReactiveMvvmOptionsSetup>());

            builder.Services.Replace(ServiceDescriptor.Singleton<IPlatformSchedulerProvider>(_ => new WinFormsSchedulerProvider(mainThreadScheduler)));

            return builder;
        }

        public static IReactiveMvvmBuilder UseWinForms(this IReactiveMvvmBuilder builder, IObservable<IScheduler> mainThreadSchedulers)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            if (mainThreadSchedulers == null)
                throw new ArgumentNullException(nameof(mainThreadSchedulers));

            return builder.UseWinForms(new VariableScheduler(mainThreadSchedulers));
        }
    }
}
