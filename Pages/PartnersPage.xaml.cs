using FashionApp.Data.Constants;

namespace FashionApp.Pages
{
    public partial class PartnersPage : ContentPage
    {
        public PartnersPage()
        {
            InitializeComponent();
        }

        private async void OnGucciClicked(object sender, EventArgs e) => await CustomWebPage("https://www.gucci.com");
        private async void ZaraButton_Clicked(object sender, EventArgs e) => await CustomWebPage("https://www.zara.com/bg/");
        private async void ArmaniButton_Clicked(object sender, EventArgs e) => await CustomWebPage("https://www.armani.com/en-gb/");

        //------------------------------------------------ METHODS ----------------------------------------------
        private async Task CustomWebPage(string url)
        {
            try
            {
                await Navigation.PushAsync(new WebViewPage(true, url));
            }
            catch (Exception ex)
            {
                CustomErrorMessage(ex.Message);
            }
        }
        private async void CustomErrorMessage(string message)
        {
            await DisplayAlert(AppConstants.Errors.ERROR, message, AppConstants.Messages.OK);
        }
    }
}