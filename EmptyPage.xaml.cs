namespace FashionApp;

public partial class EmptyPage : ContentPage
{
    public EmptyPage()
    {
        InitializeComponent();
    }

    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
} 