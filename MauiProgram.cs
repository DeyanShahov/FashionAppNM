﻿using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui;
using FashionApp.core.services;
using FashionApp.Pages;
//using Plugin.AdMob;
//using Plugin.AdMob.Configuration;


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
                //.UseAdMob()
                //.UseAdMob(new AdMobConfiguration
                //{
                //    //AdUnitId = "ca-app-pub-3940256099942544/6300978111", // Test ad unit ID for Interstitial
                //    //AdUnitId = "ca-app-pub-3940256099942544/2247696110", // Test ad unit ID for Banner
                //    //AdUnitId = "ca-app-pub-3940256099942544/5224354917", // Test ad unit ID for Rewarded
                //    //AdUnitId = "ca-app-pub-3940256099942544/5224354917" // Test ad unit ID for Rewarded Interstitial
                //})
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
            builder.Services.AddTransient<MaskEditor>();
            builder.Services.AddTransient<PartnersPage>();
            builder.Services.AddTransient<WebViewPage>();
            builder.Services.AddTransient<BaseGallery>();
            builder.Services.AddTransient<CombineImages>();

            builder.Services.AddTransient<ImageEditPage>();
            builder.Services.AddSingleton<TemporaryGallery>();

#if DEBUG
            builder.Logging.AddDebug();
            //AdConfig.UseTestAdUnitIds = true; // Use test ad unit IDs. Setwa testowi reklami
            //AdConfig.DisableConsentCheck = true; // Disable consent check. 
#endif

            //return builder.Build();

            var app = builder.Build();
            ServiceProvider = app.Services; // Запазваме DI контейнера за по-късен достъп
            return app;
        }
    }
}


