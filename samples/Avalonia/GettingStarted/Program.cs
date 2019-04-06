using Avalonia;
using Avalonia.Logging;
using Avalonia.Logging.Serilog;
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
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args)
        {
            BuildAvaloniaApp().Start(AppMain, args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder
                .Configure<App>()
                .UsePlatformDetect();
        }

        // Your application's entry point. Here you can initialize your MVVM framework, DI
        // container, etc.
        private static void AppMain(Application app, string[] args)
        {
            // ReactiveMvvm services needs to be configured at application startup
            var serviceProvider = ReactiveMvvmContext
                .Initialize(builder => builder
                    // registers platform services
                    .UseAvalonia()
                    // configures logging
                    .ConfigureLogging(ConfigureLogging)
                    // configures other services for the application
                    .ConfigureServices(ConfigureServices));

            var context = serviceProvider.GetRequiredService<IReactiveMvvmContext>();

            Logger.Sink = new LogSink(context.LoggerFactory);

            // with the help of ViewModelFactory you can create view models with services resolved from the IoC container,
            // (CreateViewModelScoped, as opposed to CreateViewModel, creates the view model in a dedicated service scope,
            // so disposing view models created like this will dispose their scoped/transient dependencies, as well)
            using (var mainViewModel = context.ViewModelFactory.CreateViewModelScoped<MainViewModel>())
            using (var mainView = new MainView { ViewModel = mainViewModel })
                app.Run(mainView);
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
