using System;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using GettingStarted.Infrastructure;
using GettingStarted.ViewModels;
using GettingStarted.Views;
using Karambolo.ReactiveMvvm;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism;
using Prism.Ioc;
using Prism.Logging;
using Prism.Navigation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace GettingStarted
{
    // ReactiveMvvm can be used with other frameworks,
    // eg. we integrate with Prism in this demo app
    sealed partial class App : PrismApplicationBase
    {
        public new static App Current = (App)PrismApplicationBase.Current;

        Subject<IScheduler> _mainThreadSchedulerSubject;

        public App()
        {
            InitializeComponent();
        }

        public IFrameProvider FrameProvider { get; private set; }

        public new CustomNavigationService NavigationService => (CustomNavigationService)base.NavigationService;

        protected override IContainerExtension CreateContainerExtension()
        {
            // we use Autofac in this sample because Prism requires support for named services
            return new AutofacContainerExtension();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            var builder = ((AutofacContainerExtension)containerRegistry).Builder;

            builder.Populate(ConfigureReactiveMvvm());

            // implementations for Prism

            builder.RegisterType<Frame>().AsSelf();

            builder.RegisterType<CustomNavigationService>().Named<IPlatformNavigationService>(NavigationServiceParameterName);

            builder.RegisterType<LoggerFacade>().As<ILoggerFacade>().SingleInstance();

            // register your views

            // (if you specify the view model type, Prism will automatically instantiate the view model when navigating to the corresponding view,
            // however, the current implementation doesn't care about disposing them, we need to do that manually:
            // the navigated from event handler is usually a good place to dispose of them)

            containerRegistry.RegisterForNavigation<MainView, MainViewModel>(nameof(MainView));

            // register your own services
        }

        IServiceCollection ConfigureReactiveMvvm()
        {
            // CoreDispatcher is not available at the time this method executes,
            // so we need to defer the capturing of the UI thread scheduler
            _mainThreadSchedulerSubject = new Subject<IScheduler>();

            // ReactiveMvvm services needs to be configured at application startup
            return new ServiceCollection()
                .AddReactiveMvvm()
                    // registers platform services
                    .UseUwp(_mainThreadSchedulerSubject)
                    // configures logging
                    .ConfigureLogging(ConfigureLogging)
                .Services;
        }

        static void ConfigureLogging(ILoggingBuilder builder)
        {
#if DEBUG
            builder.SetMinimumLevel(LogLevel.Trace);
            builder.AddDebug();
#endif
            // add the loggers of your choice
        }

        protected override void OnInitialized()
        {
            var container = ((AutofacContainerExtension)Container).Instance;

            // by using the AutofacServiceProvider adapter, we can make ReactiveMvvm to use the Autofac container we've just built
            ReactiveMvvmContext.Initialize(new AutofacServiceProvider(container));

            _mainThreadSchedulerSubject.OnNext(UwpSchedulerProvider.CaptureUIThreadScheduler());
            _mainThreadSchedulerSubject.OnCompleted();

            NavigationService.SetAsWindowContent(Window.Current, activate: true);
        }

        protected override async Task OnStartAsync(StartArgs args)
        {
            await NavigationService.NavigateAsync(nameof(MainView));
        }

        protected override void OnSuspending()
        {
            var frameContent = NavigationService.FrameProvider.Frame.Content;

            var viewModel = (frameContent as IBoundView)?.ViewModel;

            (frameContent as IDisposable)?.Dispose();
            (viewModel as IDisposable)?.Dispose();
        }
    }
}

