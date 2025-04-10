using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui;
using FashionApp.core.services;
using FashionApp.Pages;
using Plugin.AdMob;
using Plugin.AdMob.Configuration;
using FashionApp.core;
using FashionApp.core.Services;
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui.Camera;


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
                .UseAdMob()
                // Инициализация на CommunityToolkit
                .UseMauiCommunityToolkit()
                .UseMauiCommunityToolkitCamera()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.UseMauiCommunityToolkitCore();
            builder.Services.AddSingleton<IFileChecker, FileChecker>();
            builder.Services.AddSingleton<CheckForAndroidPermissions>();

            // Основни услуги на приложението (Singleton)
            builder.Services.AddSingleton<Settings>();
            builder.Services.AddSingleton<ExecutionGuardService>();

            // Услуги за CombineImages (Transient или Scoped, зависи от нуждите)
            // Регистрираме HttpClientFactory
            //builder.Services.AddHttpClient("CombineApiClient", client =>
            //{
            //    // Тук може да се добавят базови настройки за клиента, ако е необходимо
            //    // client.BaseAddress = new Uri(...);
            //});

            // Услуги с инжектиране на DisplayAlert
            builder.Services.AddTransient<IImageSelectionService>(sp =>
                new ImageSelectionService(
                    (title, message, cancel) => Shell.Current.DisplayAlert(title, message, cancel)
                )
            );
            builder.Services.AddTransient<IImageSavingService>(sp =>
               new ImageSavingService(
                    (title, message, cancel) => Shell.Current.DisplayAlert(title, message, cancel)
               )
            );

            // Услуги, които може да се нуждаят от IServiceProvider (за платформени услуги)
            builder.Services.AddTransient<IMaskManagementService>(sp =>
                new MaskManagementService(sp) // Подаваме IServiceProvider
            );

            // API Услуга
            builder.Services.AddTransient<ICombineApiService, CombineApiService>(); // Инжектира IHttpClientFactory автоматично



            //builder.Services.AddTransient<AppShell>();
            //builder.Services.AddTransient<App>();
            //builder.Services.AddTransient<LoginPage>();

            // --- Регистрация на страници ---
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddTransient<MaskEditor>();
            builder.Services.AddTransient<PartnersPage>();
            builder.Services.AddTransient<WebViewPage>();
            builder.Services.AddTransient<BaseGallery>();
            builder.Services.AddTransient<CombineImages>();
            builder.Services.AddTransient<AdvertisementPage>();

            builder.Services.AddTransient<ImageEditPage>();
            builder.Services.AddSingleton<TemporaryGallery>();

#if ANDROID
            builder.Services.AddSingleton<IFileChecker, FileChecker>(); // Пример
            builder.Services.AddSingleton<CheckForAndroidPermissions>(); // Пример
#endif

            AdConfig.UseTestAdUnitIds = true; // Use test ad unit IDs. Setwa testowi reklami
            AdConfig.DisableConsentCheck = true; // Disable consent check.           
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


