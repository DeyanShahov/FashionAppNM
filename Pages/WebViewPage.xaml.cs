using FashionApp.core.services;
using FashionApp.Data.Constants;

namespace FashionApp.Pages;

public partial class WebViewPage : ContentPage
{
	private bool isDragging = false;
    private double _startX, _startY;
    private bool _isFromPartners = false;

    public WebViewPage(bool isFromPartners = false, string url = "")
	{
        _isFromPartners = isFromPartners;
		InitializeComponent();
		View.Navigated += View_Navigated;
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
    }

    private async Task CaptureScreenshot()
    {
        if (Screenshot.Default.IsCaptureSupported)
        {
            string imageFileName = $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            var screenshot = await Screenshot.Default.CaptureAsync();
            Stream imageStream = await screenshot.OpenReadAsync();

#if ANDROID
                SaveImageToAndroid.Save($"{imageFileName}", imageStream, AppConstants.IMAGES_CAPTURE_SCREEN);
#endif
        }
        else
        {
            await DisplayAlert("Грешка", "Заснемането на екрана не се поддържа на това устройство!", "OK");
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