using System;
using Karambolo.ReactiveMvvm;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NugetSearchDemo.Converters;
using NugetSearchDemo.ViewModels;
using NugetSearchDemo.Views;

namespace NugetSearchDemo
{
    class Program
    {
        [STAThread]
        public static void Main()
        {
            var application = new App();
            application.InitializeComponent();

            // with the help of ViewModelFactory you can create view models with services resolved from the IoC container,
            // (CreateViewModelScoped, as opposed to CreateViewModel, creates the view model in a dedicated service scope,
            // so disposing view models created like this will dispose their scoped/transient dependencies, as well)
            using (var mainViewModel = Configure().ViewModelFactory.CreateViewModelScoped<AppViewModel>())
                application.Run(new MainWindow { ViewModel = mainViewModel });
        }

        static IReactiveMvvmContext Configure()
        {
            // ReactiveMvvm services needs to be configured at application startup
            return ReactiveMvvmContext
                .Initialize(builder => builder
                    // registers platform services
                    .UseWpf()
                    // configures options (we add a global binding converter here)
                    .ConfigureOptions(options => options.BindingConverters.Add(new BooleanToVisibilityConverter()))
                    // configures logging
                    .ConfigureLogging(ConfigureLogging)
                    // configures other services for the application
                    .ConfigureServices(ConfigureServices))
                .GetRequiredService<IReactiveMvvmContext>();
        }

        static void ConfigureLogging(ILoggingBuilder builder)
        {
#if DEBUG
            builder.SetMinimumLevel(LogLevel.Trace);
            builder.AddDebug();
#endif
            // add the loggers of your choice
        }

        static void ConfigureServices(IServiceCollection services)
        {
            var providers = Repository.Provider.GetCoreV3();
            var packageSource = new PackageSource("https://api.nuget.org/v3/index.json");
            var sourceRepository = new SourceRepository(packageSource, providers);

            services.AddSingleton(sourceRepository);
        }
    }
}
