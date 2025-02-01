namespace FashionApp
{
    public partial class MainPage : ContentPage
    {
        int count = 0;
        private readonly HttpClient _client = new HttpClient { Timeout = TimeSpan.FromSeconds(300) };
        private const string ApiUrl = "https://eminently-verified-walleye.ngrok-free.app";
        bool isGuest = true;

        public MainPage()
        {
            InitializeComponent();
            
            WelcomeMessage.Text = $"Welcome Guest!";
            LoginBtn.Text = "Login as User";
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            //count++;

            //if (count == 1)
            //    LoginBtn.Text = $"Clicked {count} time";
            //else
            //    LoginBtn.Text = $"Clicked {count} times";

            // Аудио четете на екрана
            //SemanticScreenReader.Announce(CounterBtn.Text);
            isGuest = !isGuest;

            if (isGuest)
            {
                WelcomeMessage.Text = "Welcome Guest!";
                WelcomeInfo.Text = "The logged-in users have access to additional features.";
                LoginBtn.Text = "Login as User";

                InputEntry.IsEnabled = false;
                InputEntry.IsVisible = false;
            }
            else
            {
                WelcomeMessage.Text = "Welcome User!";
                WelcomeInfo.Text = "Adding a prompt field for creation an image.";
                LoginBtn.Text = "Logout";

                InputEntry.IsEnabled = true;
                InputEntry.IsVisible = true;
            }

            ResponseImage.IsVisible = false;
            ResponseText.IsVisible = false;
            InputEntry.Text = "";
        }

        private async void OnLoadClicked(object sender, EventArgs e)
        {
            try
            {
                ToggleLoading(true);
                ResponseImage.IsVisible = false;
                ResponseText.IsVisible = false;

                var inputText = InputEntry.Text?.Trim();
                if (string.IsNullOrEmpty(inputText) && InputEntry.IsEnabled)
                {
                    ResponseText.Text = "Please enter some text.";
                    ResponseText.IsVisible = true;
                    ToggleLoading(false);
                    return;
                }

                var requestUrl = $"{ApiUrl}?input={Uri.EscapeDataString(isGuest ? "bottle" : inputText)}";

                var response = await _client.GetAsync(requestUrl);

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
