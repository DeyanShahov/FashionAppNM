using FashionApp.Data.Constants;
using Microsoft.Maui.Controls.PlatformConfiguration;

#if ANDROID
using Android.Content;
using Android.Provider;
#endif

namespace FashionApp.Pages;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
        SetBannerId();
        //this.BackgroundImageSource = "Resources/Images/loading_page_image.png";
        GetPhoneId();
    }

    private void GetPhoneId()
    {
#if ANDROID
        var context = Android.App.Application.Context;
        var result = Settings.Secure.GetString(context.ContentResolver, Settings.Secure.AndroidId);
#endif
    }

    private void SetBannerId()
    {
#if ANDROID
            //AdmobBanner.AdsId = "ca-app-pub-3940256099942544/9214589741";
            //AdmobBanner.AdsId = "ca-app-pub-3940256099942544/6300978111";
#endif
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string? username = UsernameEntry.Text.Trim();
        string? password = PasswordEntry.Text.Trim();

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            LoginStatusLabel.Text = AppConstants.Errors.MISSING_USERNAME_OR_PASSWORD;
            return;
        }

        // Simulate a login process (replace with actual authentication logic)
        bool isAuthenticated = await AuthenticateUser(username, password);

        if (isAuthenticated)
        {
            // Navigate to MainPage upon successful login
            var page = MauiProgram.ServiceProvider.GetRequiredService<MainPage>();
            await Navigation.PushAsync(page);
        }
        else
        {
            LoginStatusLabel.Text = AppConstants.Errors.INVALID_USERNAME_OT_PASSWORD;
        }
    }

    private Task<bool> AuthenticateUser(string username, string password)
    {
        // Replace with actual authentication logic
        return Task.FromResult(username == "user" && password == "password");
    }

    private async void LoginAsGuestButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            var page = MauiProgram.ServiceProvider.GetRequiredService<MainPage>();
            await Navigation.PushAsync(page);
        }
        catch (Exception ex)
        {
            await DisplayAlert(AppConstants.Errors.ERROR, $"{ex?.Message}\n{ex?.StackTrace}", AppConstants.Messages.OK);
        }
       
    }
}
