using CommunityToolkit.Maui.Core.Platform;
using System.Text;
using System.Text.Json;
using Microsoft.Maui.Storage;
using System.IO;
using System;
using Microsoft.Maui.Controls.PlatformConfiguration;
using CommunityToolkit.Maui.Core.Handlers;
using CommunityToolkit.Maui.Core.Views;

namespace FashionApp
{
    public partial class MainPage : ContentPage
    {
        int count = 0;
        private readonly HttpClient _client = new HttpClient { Timeout = TimeSpan.FromSeconds(300) };
        private const string ApiUrl = "https://eminently-verified-walleye.ngrok-free.app";
        //private const string ApiUrl = "http://192.168.0.101:80";
        bool isGuest = true;
        private byte[] _imageData;

        public MainPage()
        {
            InitializeComponent();

            WelcomeMessage.Text = $"Welcome Guest!";
            LoginBtn.Text = "Login as User";
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
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
            SaveButton.IsVisible = false;
            SaveButton.IsEnabled = false;
            InputEntry.Text = "";
        }

        private async void OnLoadClicked(object sender, EventArgs e)
        {
            await InputEntry.HideKeyboardAsync();

            try
            {
                ToggleLoading(true);
                ResponseImage.IsVisible = false;
                ResponseText.IsVisible = false;
                SaveButton.IsVisible = false;
                SaveButton.IsEnabled = false;

                var inputText = InputEntry.Text?.Trim();
                if (string.IsNullOrEmpty(inputText) && InputEntry.IsEnabled)
                {
                    ResponseText.Text = "Please enter some text.";
                    ResponseText.IsVisible = true;
                    ToggleLoading(false);
                    return;

                }

                var requestUrl = $"{ApiUrl}/image";
                var requestBody = new
                {
                    function_name = "generate_image",
                    args = isGuest ? "bottle" : inputText
                };

                var jsonContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await _client.PostAsync(requestUrl, jsonContent);
                //var requestUrl = $"{ApiUrl}";

                //var response2 = await _client.GetAsync(requestUrl, jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"HTTP error: {response.StatusCode}");
                }

                var contentType = response.Content.Headers.ContentType?.MediaType;
                _imageData = await response.Content.ReadAsByteArrayAsync();

                if (contentType != null && contentType.StartsWith("image/"))
                {
                    ResponseImage.Source = ImageSource.FromStream(() => new MemoryStream(_imageData));
                    ResponseImage.IsVisible = true;
                    SaveButton.IsVisible = true;
                    SaveButton.IsEnabled = true;
                }
                else
                {
                    ResponseText.Text = Encoding.UTF8.GetString(_imageData);
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

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (_imageData == null)
                return;

            try
            {
                var fileName = $"image_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                //var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);               

                //await File.WriteAllBytesAsync(filePath, _imageData);

                //await DisplayAlert("Success", $"Image saved to {filePath}", "OK");

                using var stream = new MemoryStream(_imageData);
                stream.Position = 0;               

#if WINDOWS
                await File.WriteAllBytesAsync($"C:\\Users\\Public\\Pictures\\{fileName}", _imageData);
                await DisplayAlert("Success", $"Image saved to C:\\Users\\Public\\Pictures", "OK");

#elif ANDROID
                var context = Platform.CurrentActivity;

                if (OperatingSystem.IsAndroidVersionAtLeast(29))
                {
                    Android.Content.ContentResolver resolver = context.ContentResolver;
                    Android.Content.ContentValues contentValues = new();
                    contentValues.Put(Android.Provider.MediaStore.IMediaColumns.DisplayName, fileName);
                    contentValues.Put(Android.Provider.MediaStore.IMediaColumns.MimeType, "image/png");
                    contentValues.Put(Android.Provider.MediaStore.IMediaColumns.RelativePath, "DCIM/" + "FashionApp");
                    Android.Net.Uri imageUri = resolver.Insert(Android.Provider.MediaStore.Images.Media.ExternalContentUri, contentValues);
                    var os = resolver.OpenOutputStream(imageUri);
                    Android.Graphics.BitmapFactory.Options options = new();
                    options.InJustDecodeBounds = true;
                    var bitmap = Android.Graphics.BitmapFactory.DecodeStream(stream);
                    //var bitmap = _imageData;
                    bitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, os);
                    os.Flush();
                    os.Close();

                    await DisplayAlert("Success", $"Image saved on DCIM / FashionApp!", "OK");
                }
                else
                {
                    Java.IO.File storagePath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures);
                    string path = System.IO.Path.Combine(storagePath.ToString(), fileName);
                    System.IO.File.WriteAllBytes(path, stream.ToArray());
                    //System.IO.File.WriteAllBytes(path, _imageData);
                    var mediaScanIntent = new Android.Content.Intent(Android.Content.Intent.ActionMediaScannerScanFile);
                    mediaScanIntent.SetData(Android.Net.Uri.FromFile(new Java.IO.File(path)));
                    context.SendBroadcast(mediaScanIntent);
                }
#endif
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to save image: {ex.Message}", "OK");
            }
        }

        private async void OnNavigateClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new EmptyPage());
        }

        private async void OnNavigateClickedToPage2(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new Page2());
        }

        private async void OnNavigateClickedToWeb(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new WebViewPage());
        }

        private async void OnNavigateClickedToMaskJS(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MaskJS());
        }
        private void ToggleLoading(bool isLoading)
        {
            LoadButton.IsEnabled = !isLoading;
            LoadingIndicator.IsRunning = isLoading;
            LoadingIndicator.IsVisible = isLoading;
        }
    }

}
