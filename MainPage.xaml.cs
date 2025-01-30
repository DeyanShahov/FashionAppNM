using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace FashionApp
{
    public partial class MainPage : ContentPage
    {
        int count = 0;
        private readonly HttpClient _client = new HttpClient { Timeout = TimeSpan.FromSeconds(300) };
        private const string ApiUrl = "https://eminently-verified-walleye.ngrok-free.app/run-test1";


        public MainPage()
        {
            InitializeComponent();
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }

        private async void OnLoadClicked(object sender, EventArgs e)
        {
            try
            {
                ToggleLoading(true);
                ResponseImage.IsVisible = false;
                ResponseText.IsVisible = false;

                var response = await _client.GetAsync(ApiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"HTTP error: {response.StatusCode}");
                }

                var contentType = response.Content.Headers.ContentType?.MediaType;
                var content = await response.Content.ReadAsByteArrayAsync();

                if (contentType != null && contentType.StartsWith("image/"))
                {
                    ResponseImage.Source = ImageSource.FromStream(() => new MemoryStream(content));
                    ResponseImage.IsVisible = true;
                }
                else
                {
                    ResponseText.Text = System.Text.Encoding.UTF8.GetString(content);
                    ResponseText.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                ResponseText.Text = $"Error: {ex.Message}";
                ResponseText.IsVisible = true;
            }
            finally
            {
                ToggleLoading(false);
            }
        }

        private void ToggleLoading(bool isLoading)
        {
            LoadButton.IsEnabled = !isLoading;
            LoadingIndicator.IsRunning = isLoading;
            LoadingIndicator.IsVisible = isLoading;
        }
    }

}
