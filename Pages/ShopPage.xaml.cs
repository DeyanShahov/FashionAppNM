using FashionApp.core;

namespace FashionApp.Pages;

public partial class ShopPage : ContentPage
{
    //private readonly Settings _appSettings;

    //public ShopPage(Settings settings)
    public ShopPage()
    {
        InitializeComponent();
        //_appSettings = settings;
        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateTokenCount();
    }

    private void UpdateTokenCount()
    {
        //TokenCountLabel.Text = $"Current Tokens: {_appSettings.Tokens}";
        TokenCountLabel.Text = $"Current Tokens: {AppSettings.Tokens}";
    }

    private async void OnPackage1Tapped(object sender, TappedEventArgs e)
    {
        await SimulatePurchase(0.50m, 2);
    }

    private async void OnPackage2Tapped(object sender, TappedEventArgs e)
    {
        await SimulatePurchase(1.00m, 5);
    }

    private async void OnPackage3Tapped(object sender, TappedEventArgs e)
    {
        await SimulatePurchase(2.00m, 10);
    }

    private async Task SimulatePurchase(decimal amount, int tokensToAdd)
    {
        // Simulate PayPal interaction (e.g., show a confirmation dialog)
        bool confirmed = await DisplayAlert("Confirm Purchase", $"Do you want to purchase {tokensToAdd} tokens for ${amount}?", "Yes", "No");

        if (confirmed)
        {
            // Simulate successful payment
            // In a real app, you would integrate with PayPal SDK here
            await Task.Delay(500); // Simulate network delay

            //_appSettings.Tokens += tokensToAdd;
            AppSettings.Tokens += tokensToAdd;
            UpdateTokenCount();

            await DisplayAlert("Purchase Successful", $"You have successfully purchased {tokensToAdd} tokens.", "OK");
        }
        else
        {
            await DisplayAlert("Purchase Cancelled", "The purchase was cancelled.", "OK");
        }
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}