#nullable enable

using CommunityToolkit.Maui;
using GettingStarted.ViewModels;
using GettingStarted.Views;
using Karambolo.ReactiveMvvm;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

namespace GettingStarted
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder()
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .RegisterServices()
                .RegisterViewModels()
                .RegisterViews()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            var app = builder.Build();
            ReactiveMvvmContext.Initialize(app.Services);
            return app;
        }

        static MauiAppBuilder RegisterServices(this MauiAppBuilder builder)
        {
            // ReactiveMvvm services needs to be configured at application startup
            builder.Services.AddReactiveMvvm()
                // registers platform services
                .UseMaui();

            // configures logging
            builder.Services.AddLogging(ConfigureLogging);

            // More services registered here.

            return builder;
        }

        static MauiAppBuilder RegisterViewModels(this MauiAppBuilder builder)
        {
            builder.Services.AddScoped(sp =>
            {
                // with the help of ViewModelFactory you can create view models with services resolved from the IoC container,
                // (CreateViewModelScoped, as opposed to CreateViewModel, creates the view model in a dedicated service scope,
                // so disposing view models created like this will dispose their scoped/transient dependencies, as well)
                return sp.GetRequiredService<IReactiveMvvmContext>().ViewModelFactory.CreateViewModelScoped<MainViewModel>();
            });

            // More view-models registered here.

            return builder;
        }

        static MauiAppBuilder RegisterViews(this MauiAppBuilder builder)
        {
            builder.Services.AddScoped<MainView>();

            // More views registered here.

            return builder;
        }

        static void ConfigureLogging(ILoggingBuilder builder)
        {
#if DEBUG
            builder.SetMinimumLevel(LogLevel.Trace);
            builder.AddDebug();
#endif
            // add the loggers of your choice
        }
    }
}
