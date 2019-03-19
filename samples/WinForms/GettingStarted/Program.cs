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

            // by default CreateViewModel creates a new, dedicated service scope for each view model instance it creates,
            // so disposing a view model instance will dispose its scoped/transient dependencies, as well
            using (var mainViewModel = Configure().ViewModelFactory.CreateViewModel<MainViewModel>())
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
