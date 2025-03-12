using FashionApp.core.services;
using FashionApp.Data.Constants;

namespace FashionApp.Pages;

public partial class WebViewPage : ContentPage
{
    private bool _isFromPartners = false;

    public WebViewPage(bool isFromPartners = false, string url = "")
    {
        _isFromPartners = isFromPartners;
        InitializeComponent();
        View.Navigated += View_Navigated;
        View.Navigating += View_Navigating; // Subscribe to the Navigating event
        if (_isFromPartners ) SetWebViewForPartners(url);
    }

    private void SetWebViewForPartners(string url)
    {
        //string bingSearchUrl = $"https://www.bing.com/search?q={searchQuery}";
        //View.Source = bingSearchUrl; // Търсене в Bing
        View.Source = url; // Търсене в Bing
    }

    private void OnBackClicked(object sender, EventArgs e)
    {
#if ANDROID
        View.GoBack();
#endif
    }

    private void OnForwardClicked(object sender, EventArgs e)
    {
#if ANDROID
        View.GoForward();
#endif
    }

    private void OnRefreshClicked(object sender, EventArgs e)
    {
#if ANDROID
       View.Reload();
#endif
    }

    private void OnGoClicked(object sender, EventArgs e)
    {
        var input = UrlEntry.Text?.Trim();

        if (!string.IsNullOrEmpty(input))
        {
            if (Uri.TryCreate(input, UriKind.Absolute, out Uri uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                View.Source = input; // Задаване на URL
            }
            else
            {
                string searchQuery = Uri.EscapeDataString(input);
                string bingSearchUrl = $"https://www.bing.com/search?q={searchQuery}";
                View.Source = bingSearchUrl; // Търсене в Bing
            }
        }
    }

    private void View_Navigated(object sender, WebNavigatedEventArgs e)
    {
        BackButton.IsEnabled = View.CanGoBack;
        ForwardButton.IsEnabled = View.CanGoForward;
        currentUrlLabel.Text = $"Текущ адрес: {e.Url}"; // Прекиждан адрес
        UrlEntry.Text = e.Url; // Update the UrlEntry with the current URL
    }

    // Add the View_Navigating event handler
    private void View_Navigating(object sender, WebNavigatingEventArgs e)
    {
        UrlEntry.Text = e.Url; // Update the UrlEntry while navigating
    }

    private async Task CaptureScreenshot()
    {
        if (Screenshot.Default.IsCaptureSupported)
        {
            string imageFileName = $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            var screenshot = await Screenshot.Default.CaptureAsync();
            Stream imageStream = await screenshot.OpenReadAsync();

            // Save the screenshot to the device
#if ANDROID
            SaveImageToAndroid.Save($"{imageFileName}", imageStream, AppConstants.ImagesConstants.IMAGES_CAPTURE_SCREEN);
#endif
            imageStream.Close();

            // Get current URL from UrlEntry
            string currentUrl = UrlEntry.Text;

            // Create or load existing screenshot mapping
            //var preferences = App.Current.PersistedPreferences;
            string existingData = Preferences.Get("screenshot_urls", string.Empty);
            Dictionary<string, string> urlMapping;

            if (string.IsNullOrEmpty(existingData)) urlMapping = new Dictionary<string, string>();
            else
            {
                try
                {
                    urlMapping = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(existingData);
                }
                catch (Exception ex)
                {
                    // Invalid data format, start fresh
                    urlMapping = new Dictionary<string, string>();
                    System.Diagnostics.Debug.WriteLine($"Error deserializing screenshot URLs: {ex.Message}");
                }
            }

            // Add new screenshot URL mapping
            urlMapping[imageFileName] = currentUrl;

            // Serialize and save back to preferences
            string newData = Newtonsoft.Json.JsonConvert.SerializeObject(urlMapping);
            Preferences.Set("screenshot_urls", newData);
        }
        else
        {
            await DisplayAlert(AppConstants.Errors.ERROR, AppConstants.Errors.SCREEN_CAPTURE_NOT_SUPORTED, AppConstants.Messages.OK);
        }
    }

    private async void OnCaptureClicked(object sender, EventArgs e)
    {
        ChangeVisibilityForScreenShot();
        await CaptureScreenshot();
        ChangeVisibilityForScreenShot();
    }

    private void ChangeVisibilityForScreenShot()
    {
        MyMenuBar.IsVisible = !MyMenuBar.IsVisible;
        CaptureButton.IsVisible = !CaptureButton.IsVisible;
    }
}
