using System;
using GettingStarted.ViewModels;
using GettingStarted.Views;
using Karambolo.ReactiveMvvm;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GettingStarted
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
            using (var mainViewModel = Configure().ViewModelFactory.CreateViewModelScoped<MainViewModel>())
            using (var mainView = new MainView { ViewModel = mainViewModel })
                application.Run(mainView);
        }

        static IReactiveMvvmContext Configure()
        {
            // ReactiveMvvm services needs to be configured at application startup
            return ReactiveMvvmContext
                .Initialize(builder => builder
                    // registers platform services
                    .UseWpf()
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
            // register your own services
        }
    }
}
