using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui;
using FashionApp.core.services;
using FashionApp.Pages;

namespace FashionApp
{
    public static class MauiProgram
    {
        public static IServiceProvider ServiceProvider { get; private set; }
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkitCamera()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.UseMauiCommunityToolkitCore();
            builder.Services.AddSingleton<IFileChecker, FileChecker>();
            builder.Services.AddSingleton<CheckForAndroidPermissions>();
            builder.Services.AddSingleton<ExecutionGuardService>();

            //builder.Services.AddTransient<AppShell>();
            //builder.Services.AddTransient<App>();
            //builder.Services.AddTransient<LoginPage>();
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<MaskEditor>();
            builder.Services.AddSingleton<PartnersPage>();
            builder.Services.AddSingleton<WebViewPage>();
            builder.Services.AddTransient<BaseGallery>();
            builder.Services.AddSingleton<CombineImages>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            //return builder.Build();

            var app = builder.Build();
            ServiceProvider = app.Services; // Запазваме DI контейнера за по-късен достъп
            return app;
        }
    }
}


