using CommunityToolkit.Maui.Core.Platform;
using FashionApp.Data.Constants;
using FashionApp.Pages;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;

namespace FashionApp.Pages
{
    public partial class MainPage : ContentPage
    {
        private readonly HttpClient _client = new HttpClient { Timeout = TimeSpan.FromSeconds(300) };
        private const string ApiUrl = "https://eminently-verified-walleye.ngrok-free.app";
        //private const string ApiUrl = "http://192.168.0.101:80";
        bool isGuest = true;
        bool isAdmin = false;
        private byte[] _imageData = [];


        private const string monkeyUrl = "https://montemagno.com/monkeys.json";
        private readonly HttpClient httpClient = new HttpClient();

        //public ObservableCollection<Monkey> Monkeys { get; set; } = new ObservableCollection<Monkey> { };

        public MainPage()
        {
            InitializeComponent();

            BindingContext = this;

            if (isAdmin)
            {
                WelcomeMessage.Text = AppConstants.Messages.WELCOME_GUEST;
                LoginBtn.Text = AppConstants.Messages.LOGIN_AS_USER;
                AdminTestFields.IsVisible = true;

                TestGalleryButton.IsVisible = true;
                Grid.SetRow(TestGalleryButton, 1);
                Grid.SetColumn(TestGalleryButton, 0);

                Grid.SetRow(WebGalleryButton, 1);
                Grid.SetColumn(WebGalleryButton, 1);
            }
            else
            {
                //GridForGallerys.ColumnSpacing = 5;
                Grid.SetRow(WebGalleryButton, 0);
                Grid.SetColumn(WebGalleryButton, 2);
            }
            
            SetContentLabel();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            //var monkeyJson = await httpClient.GetStringAsync(monkeyUrl);
            //var monkeys = JsonConvert.DeserializeObject<Monkey[]>(monkeyJson);
            //Monkeys.Clear();
            //monkeys.ToList().ForEach( monkey => Monkeys.Add(monkey));

            double screenWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
            StartGaleery.WidthRequest = screenWidth;
        }

        private void SetContentLabel()
        {
            ContentLabel.Text = AppConstants.Messages.CREATED_BY;

#if ANDROID
            // Проверка и отпечатване на екрана на текущата Андроид версия и съответната API версия
            ContentLabel.Text += $"\n Android: {Android.OS.Build.VERSION.Release}  API: {(int)(int)Android.OS.Build.VERSION.SdkInt}";
#endif
        }

        private void OnLoginButton_Clicked(object sender, EventArgs e)
        {
            isGuest = !isGuest;

            WelcomeMessage.Text = isGuest ? AppConstants.Messages.WELCOME_GUEST : AppConstants.Messages.WELCOME_USER;
            WelcomeInfo.Text = isGuest ? AppConstants.Messages.GUEST_MESSAGE : AppConstants.Messages.USER_MESSAGE;
            LoginBtn.Text = isGuest ?  AppConstants.Messages.LOGIN_AS_USER : AppConstants.Messages.LOGOUT;

            SetVisibilityElementsForGuest(isGuest);
            SetVisibilityForResult(false);

            InputEntry.Text = String.Empty;
        }   

        private async void OnLoadClicked(object sender, EventArgs e)
        {
            // Проверка на операционната система и скриване на клавиятурата 
            if (OperatingSystem.IsAndroid()) await InputEntry.HideKeyboardAsync();

            try
            {
                ToggleLoading(true);
                SetVisibilityForResult(false);

                var inputText = InputEntry.Text?.Trim();
                if (string.IsNullOrEmpty(inputText) && InputEntry.IsEnabled)
                {
                    ResponseText.Text = AppConstants.Errors.PLEASE_ENTER_SOME_TEXT;
                    ResponseText.IsVisible = true;
                    ToggleLoading(false);
                    return;
                }

                var requestUrl = $"{ApiUrl}/image";
                var requestBody = new
                {
                    function_name = AppConstants.Parameters.CONFY_FUNCTION_GENERATE_NAME,
                    args = isGuest ? AppConstants.Parameters.CONFY_FUNCTION_GENERATE_ARG : inputText
                };

                var jsonContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
                var response = await _client.PostAsync(requestUrl, jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"HTTP {AppConstants.Errors.ERROR}: {response.StatusCode}");
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
                ResponseText.Text = $"{AppConstants.Errors.ERROR}: {ex.Message}";
                ResponseText.IsVisible = true;
            }
            finally
            {
                ToggleLoading(false);
            }          
        }

        private async void OnSaveClicked(object sender, EventArgs e)
        {
            if (_imageData == null) return;

            try
            {
                var fileName = $"image_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                using var stream = new MemoryStream(_imageData);
                stream.Position = 0;

#if WINDOWS
                await File.WriteAllBytesAsync($"C:\\Users\\Public\\Pictures\\{fileName}", _imageData);
                await DisplayAlert("Success", $"Image saved to C:\\Users\\Public\\Pictures", "OK");

#elif ANDROID
                FashionApp.core.services.SaveImageToAndroid.Save(fileName, stream,  AppConstants.ImagesConstants.IMAGES_CREATED_IMAGES);                      
#endif
            }
            catch (Exception ex)
            {
                await DisplayAlert(AppConstants.Errors.ERROR, $"{AppConstants.Errors.FAILED_TO_SAVE_IMAGE}: {ex.Message}", AppConstants.Messages.OK);
            }
        }

        private async void OnEditorButtonClicked(object sender, EventArgs e)
            => await Navigation.PushAsync(new MaskEditor(isAdmin));

        private async void CombineImagesButton_Clicked(object sender, EventArgs e)
            => await Navigation.PushAsync(new CombineImages(isAdmin));

        private async void OnNavigateClickedToWeb(object sender, EventArgs e)
            => await Navigation.PushAsync(new WebViewPage());

        private async void WebGalleryButton_Clicked(object sender, EventArgs e)
            => await Navigation.PushAsync(new BaseGallery(
                AppConstants.Parameters.APP_CLOTH_GALLERY,
                $"{AppConstants.Parameters.APP_BASE_PATH}/{AppConstants.Parameters.APP_NAME}/{AppConstants.Parameters.APP_FOLDER_SCREEN}%"));

        private async void TestGalleryButton_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new TestGallery("Test Gallery", "TestGallery"));
        }

        private async void PartnersPageButton_Clicked(object sender, EventArgs e)
            => await Navigation.PushAsync(new PartnersPage());

        //private async void OnNavigateClickedToMaskJS(object sender, EventArgs e)
        //    => await Navigation.PushAsync(new MaskJS());  

        private async void GalleryButton_Clicked(object sender, EventArgs e)
           => await Navigation.PushAsync(new BaseGallery(
               AppConstants.Parameters.APP_FOLDER_IMAGES,
               $"{AppConstants.Parameters.APP_BASE_PATH}/{AppConstants.Parameters.APP_NAME}/{AppConstants.Parameters.APP_FOLDER_IMAGES}%"));

        private async void MaskGalleryButton_Clicked(object sender, EventArgs e)
           => await Navigation.PushAsync(new BaseGallery(
               AppConstants.Parameters.APP_FOLDER_MASK,
               $"{AppConstants.Parameters.APP_BASE_PATH}/{AppConstants.Parameters.APP_NAME}/{AppConstants.Parameters.APP_FOLDER_MASK}%"));


        // ------------------------------------------------------------------------------------------------------
        private void SetVisibilityElementsForGuest(bool isGuestActive)
        {
            InputEntry.IsEnabled = !isGuestActive;
            InputEntry.IsVisible = !isGuestActive;
        }
        private void ToggleLoading(bool isLoading)
        {
            LoadButton.IsEnabled = !isLoading;
            LoadingIndicator.IsRunning = isLoading;
            LoadingIndicator.IsVisible = isLoading;
        }
        private void SetVisibilityForResult(bool toSet)
        {
            ResponseImage.IsVisible = toSet;
            ResponseText.IsVisible = toSet;
            SaveButton.IsVisible = toSet;
            SaveButton.IsEnabled = toSet;
        }    
    }
}
