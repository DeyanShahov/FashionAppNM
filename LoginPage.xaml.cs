using FashionApp.Data.Constants;

namespace FashionApp;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
        //this.BackgroundImageSource = "Resources/Images/loading_page_image.png";
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string username = UsernameEntry.Text?.Trim();
        string password = PasswordEntry.Text?.Trim();

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
            await Navigation.PushAsync(new MainPage());
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
        await Navigation.PushAsync(new MainPage());
    }
}
