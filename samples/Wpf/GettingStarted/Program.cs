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

            // by default ViewModelFactory.CreateViewModel creates a new, dedicated service scope for each view model instance it creates,
            // so disposing a view model instance will dispose its scoped/transient dependencies, as well
            using (var mainViewModel = Configure().ViewModelFactory.CreateViewModel<MainViewModel>())
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
                    // configures services
                    .ConfigureServices(ConfigureServices)
                    // discovers and registers view models as transient dependencies
                    .RegisterViewModels(typeof(Program).Assembly))
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
