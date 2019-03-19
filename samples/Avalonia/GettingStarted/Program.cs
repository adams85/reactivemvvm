using System;
using Avalonia;
using Avalonia.Logging;
using GettingStarted.Infrastructure;
using GettingStarted.ViewModels;
using GettingStarted.Views;
using Karambolo.ReactiveMvvm;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GettingStarted
{
    class Program
    {
        static void Main(string[] args)
        {
            MainViewModel mainViewModel = null;
            try
            {
                // by default ViewModelFactory.CreateViewModel creates a new, dedicated service scope for each view model instance it creates,
                // so disposing a view model instance will dispose its scoped/transient dependencies, as well
                BuildAvaloniaApp().Start<MainView>(() => mainViewModel = ReactiveMvvmContext.Current.ViewModelFactory.CreateViewModel<MainViewModel>());
            }
            finally
            {
                (Application.Current.MainWindow as IDisposable)?.Dispose();
                mainViewModel?.Dispose();
            }
        }

        public static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder
                .Configure<App>()
                .UsePlatformDetect()
                .AfterSetup(Configure);
        }

        static void Configure(AppBuilder appBuilder)
        {
            // ReactiveMvvm services needs to be configured at application startup
            var serviceProvider = ReactiveMvvmContext
               .Initialize(builder => builder
                   // registers platform services
                   .UseAvalonia()
                   // configures logging
                   .ConfigureLogging(ConfigureLogging)
                   // configures services
                   .ConfigureServices(ConfigureServices)
                   // discovers and registers view models as transient dependencies
                   .RegisterViewModels(typeof(Program).Assembly));

            var context = serviceProvider.GetRequiredService<IReactiveMvvmContext>();
            Logger.Sink = new LogSink(context.LoggerFactory);
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
