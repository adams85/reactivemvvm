using System;
using System.Windows.Forms;
using Karambolo.ReactiveMvvm;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using GettingStarted.ViewModels;
using GettingStarted.Views;

namespace GettingStarted
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // with the help of ViewModelFactory you can create view models with services resolved from the IoC container,
            // (CreateViewModelScoped, as opposed to CreateViewModel, creates the view model in a dedicated service scope,
            // so disposing view models created like this will dispose their scoped/transient dependencies, as well)
            using (var mainViewModel = Configure().ViewModelFactory.CreateViewModelScoped<MainViewModel>())
                Application.Run(new MainView { ViewModel = mainViewModel });
        }

        static IReactiveMvvmContext Configure()
        {
            // ReactiveMvvm services needs to be configured at application startup
            return ReactiveMvvmContext
                .Initialize(builder => builder
                    // registers platform services
                    .UseWinForms()
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
