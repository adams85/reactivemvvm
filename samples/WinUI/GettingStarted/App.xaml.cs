#nullable enable

using System;
using GettingStarted.ViewModels;
using GettingStarted.Views;
using Karambolo.ReactiveMvvm;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Graphics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GettingStarted
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public static new App Current => (App)Application.Current;

        static IReactiveMvvmContext Configure()
        {
            // ReactiveMvvm services needs to be configured at application startup
            return ReactiveMvvmContext
                .Initialize(builder => builder
                    // registers platform services
                    .UseWinUI()
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

        private readonly IReactiveMvvmContext _context;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();

            _context = Configure();
        }

        private Window? _mainWindow;
        public Window MainWindow
        {
            get => _mainWindow ?? throw new InvalidOperationException("Main window is not available yet.");
            set => _mainWindow = value;
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            // with the help of ViewModelFactory you can create view models with services resolved from the IoC container,
            // (CreateViewModelScoped, as opposed to CreateViewModel, creates the view model in a dedicated service scope,
            // so disposing view models created like this will dispose their scoped/transient dependencies, as well)
            var mainViewModel = _context.ViewModelFactory.CreateViewModelScoped<MainViewModel>();

            _mainWindow = new Window
            {
                Title = "ReactiveMvvm WinUI Demo",
                SystemBackdrop = new MicaBackdrop(),
            };

            _mainWindow.AppWindow.Resize(new SizeInt32(800, 450));

            var rootFrame = new Frame();

            rootFrame.Unloaded += (s, _) =>
            {
                ((MainView)((Frame)s).Content).Dispose();
                mainViewModel.Dispose();
            };

            rootFrame.Navigate(typeof(MainView), mainViewModel);

            _mainWindow.Content = rootFrame;
            _mainWindow.Activate();
        }
    }
}
