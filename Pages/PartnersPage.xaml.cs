using Microsoft.Maui.Controls;
//using Microsoft.Maui.Launcher;

namespace FashionApp.Pages
{
    public partial class PartnersPage : ContentPage
    {
        public PartnersPage()
        {
            InitializeComponent();
        }

        private async void OnGucciClicked(object sender, EventArgs e)
        {
            try
            {
                //await Launcher.Default.OpenAsync(new Uri("https://www.gucci.com"));
                await Navigation.PushAsync(new WebViewPage(true, "https://www.gucci.com"));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void ZaraButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                await Navigation.PushAsync(new WebViewPage(true, "https://www.zara.com/bg/"));
            }
            catch (Exception ex)
            {

                await DisplayAlert("Error", ex.Message, "OK");
            }
           
        }

        private async void ArmaniButton_Clicked(object sender, EventArgs e)
        {
           

            try
            {
                await Navigation.PushAsync(new WebViewPage(true, "https://www.armani.com/en-gb/"));
            }
            catch (Exception ex)
            {

                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        // Add similar methods for other brands
    }
}