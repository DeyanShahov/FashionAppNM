using FashionApp.core;
using FashionApp.core.services;
using FashionApp.Data.Constants;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using System.Text.Json;


namespace FashionApp.Pages;

public partial class CombineImages : ContentPage
{
    private readonly HttpClient _client = new HttpClient { Timeout = TimeSpan.FromSeconds(300) };
    private const string ApiUrl = "http://77.77.134.134:82";
    //private const string ApiUrl = "https://eminently-verified-walleye.ngrok-free.app";
    //private const string ApiUrl = "http://192.168.0.101:80";
    //private const string ApiUrl = "http://127.0.0.1:80";
    private byte[] _imageData = [];
    private string _clothImagePath = String.Empty;
    private string _bodyImagePath = String.Empty;

    private readonly SingleImageLoader singleImageLoader;

    public ObservableCollection<JacketModel> ListAvailableClothesForMacros { get; set; } = new ObservableCollection<JacketModel>();

    private bool isFromClothImage = true;

    public CombineImages()
    {
        InitializeComponent();

        BindingContext = this;

        singleImageLoader = new SingleImageLoader(
            setErrorMessage: (msg) => ResponseText.Text = msg,
            setImage: (uri) => SelectedBodyImage.Source = Microsoft.Maui.Controls.ImageSource.FromUri(new System.Uri(uri))
        );

#if ANDROID
        CheckAvailableMasksAndroid(true);
#endif
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        Tokens.Text = $"{AppSettings.Tokens}";

        SetVisibilityOnCombineImagesButtonBasedOnTokens();

        PanelButtons.IsVisible = false;
    }

    //---------------------------------------- BUTONS ACTIONS --------------------------------------------------------------
    private async void OnNavigateClicked(object sender, EventArgs e) => await Navigation.PopAsync();
    private void SelectClothImageButton_Clicked(object sender, EventArgs e) => SetAnImageAsSourceAsync(SelectedClothImage, nameof(_clothImagePath));
    private void OnSelectedClothImageTapped(object sender, TappedEventArgs e) => PanelButtons.IsVisible = !PanelButtons.IsVisible;

    private void OnSelectedBodyImageTapped(object sender, TappedEventArgs e) => SetAnImageAsSourceAsync(SelectedBodyImage, nameof(_bodyImagePath));
    private async void OnCreateRewardedInterstitialClicked(object sender, EventArgs e)
    {
        bool pageAlreadyExists = Navigation.ModalStack.Any(p => p is AdvertisementPage);
        if (pageAlreadyExists) return;

        var page = MauiProgram.ServiceProvider.GetRequiredService<AdvertisementPage>();
        await Navigation.PushModalAsync(page);
    }

    private async void ShopPage_Clicked(object sender, EventArgs e)
    {
        bool pageAlreadyExists = Navigation.ModalStack.Any(p => p is ShopPage);
        if (pageAlreadyExists) return;

        var page = MauiProgram.ServiceProvider.GetRequiredService<ShopPage>();
        await Navigation.PushAsync(page);
    }

    private async void OnCombineImages_Clicked(object sender, EventArgs e) => await CombineImagesAction();

    private async Task CombineImagesAction()
    {
        if (AppSettings.Tokens < 1) return;
        if (await CheckForMissingInput()) return; ;

        try
        {
            InputStack.IsVisible = false;
            ResultStack.IsVisible = true;

            ToggleLoading(true);

            SaveButton.IsVisible = false;
            SaveButton.IsEnabled = false;

            ResponseText.IsVisible = false;

            if (_clothImagePath == "") _clothImagePath = SelectedClothImage.Source.ToString()?.Remove(0, 6);  //???????


            if (!await UploadImages()) return; // Качваме двата файла

            // Изпращаме POST заявка към API
            await Task.Delay(5000); // Изчакване от 5 секунди

            bool result = await SendCombineRequest(ApiUrl, _imageData, ResponseImage, ResponseText.Text);
            if (result)
            {
                SaveButton.IsVisible = true;
                SaveButton.IsEnabled = true;
            }
            else
            {
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

            AppSettings.Tokens--;
            Tokens.Text = $"{AppSettings.Tokens}";

            GoogleAdsButton.IsEnabled = true;
            GoogleAdsButton.IsVisible = true;

            SetVisibilityOnCombineImagesButtonBasedOnTokens();
        }
    }

    private async Task<bool> SendCombineRequest(string Url, byte[] _imageData, Image ResponseImage, string responseText)
    {
        var requestUrl = $"{Url}/{AppConstants.Parameters.CONFY_FUNCTION_COMBINE_ENDPOINT}";
        var requestBody = new
        {
            function_name = AppConstants.Parameters.CONFY_FUNCTION_GENERATE_NAME,
            cloth_image = AppConstants.Parameters.INPUT_IMAGE_CLOTH,
            body_image = AppConstants.Parameters.INPUT_IMAGE_BODY,
            mask_detection_method = true,  // Задаване типа на маската: ръчна ( true ) / АI ( false )
            args = new List<string>() // Списъка с евентуалните зони за маркиране от АЙ-то
        };

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json"
        );

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
            return true;
        }
        else
        {
            responseText = Encoding.UTF8.GetString(_imageData);         
            return false;
        }
    }

    private async Task<bool> CheckForMissingInput()
    {
        var result1 = SelectedClothImage.Source.ToString()?.Remove(0, 6);
        var result2 = SelectedBodyImage.Source.ToString()?.Remove(0, 6);
        if (result1 == "Icons/blank_image_photo.png" || result2 == "Icons/blank_image_photo.png")
        {
            await DisplayAlert(AppConstants.Errors.ERROR, AppConstants.Errors.SELECT_BOTH_IMAGES, AppConstants.Messages.OK);
            return true;
        }

        return false;

        //if(SelectedClothImage.Source.ToString().Contains("blank") || SelectedBodyImage.Source.ToString().Contains("blank")) return true;
        //else return false;
    }

    private async void CameraButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new CameraPage(SelectedClothImage, ref _clothImagePath));
    }


    private void OnBackButtonClicked(object sender, EventArgs e) => ReturnToCombinePageAfterBackOrSave();

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
            ReturnToCombinePageAfterBackOrSave();
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                AppConstants.Errors.ERROR,
                $"{AppConstants.Errors.FAILED_TO_SAVE_IMAGE}: {ex.Message}",
                AppConstants.Messages.OK);
        }
    }


    private void MacroButton_Clicked(object sender, EventArgs e)
    {
        ImageButton clickedButton = (ImageButton)sender;
        string value = (string)clickedButton.CommandParameter;

        if (string.IsNullOrEmpty(value)) return;

        string? futureImageName = new MacroIconToPhoto(value).Value;

        if (futureImageName == null || futureImageName == "Default") return;

#if ANDROID
        LoadLargeImage(futureImageName);
#endif
    }


    //-------------------------------------------- FUNCTIONS --------------------------------------------------------- 
    private async Task<bool> UploadImages()
    {
        var uploader = new ComfyUIUploader(ApiUrl);
        if (!string.IsNullOrEmpty(_clothImagePath) && !string.IsNullOrEmpty(_bodyImagePath))
        {
            await uploader.UploadImageAsync(_clothImagePath); // Качваме първата картинка            
            await uploader.UploadImageAsync(_bodyImagePath); // Качваме втората картинка

            DeleteTemporaryImage();
            return true;
        }
        else
        {
            await DisplayAlert(AppConstants.Errors.ERROR, AppConstants.Errors.MISSING_PATH_TO_FILES, AppConstants.Messages.OK);
            return false;
        }
    }

    public async Task<byte[]> LoadEmbeddedImageAsync(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourcePath = $"FashionApp.Resources.EmbeddedResource.{fileName}"; // Задай правилното име на namespace-а!

        using Stream stream = assembly.GetManifestResourceStream(resourcePath);
        if (stream == null)
            throw new FileNotFoundException($"Embedded resource {fileName} not found.");

        using MemoryStream memoryStream = new();
        await stream.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }

    public async Task UploadEmbeddedImageAsync(string fileName)
    {
        try
        {
            var result = Path.Combine("EmbeddedResource", Path.GetFileName(fileName));
            using var stream = File.OpenRead(result);
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            byte[] imageData = memoryStream.ToArray();

            using var content = new MultipartFormDataContent{
                { new ByteArrayContent(imageData), "file", fileName }};

            using var client = new HttpClient();
            var response = await client.PostAsync("http://127.0.0.1:8188/upload_image", content);

            if (response.IsSuccessStatusCode) Console.WriteLine("Image uploaded successfully!");
            else Console.WriteLine($"Upload failed: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private async void DeleteTemporaryImage()
    {
        // Получаване на пътя към кеш директорията
        string cacheDir = FileSystem.CacheDirectory;

        // Изтриване на стари файлове, които съвпадат с шаблона "testImage_*.png"
        var oldFiles = Directory.GetFiles(cacheDir, "testImage_*.png");
        foreach (var oldFile in oldFiles)
        {
            try
            {
                File.Delete(oldFile);
            }
            catch (Exception ex)
            {
                await DisplayAlert(
                    AppConstants.Errors.ERROR,
                    $"{AppConstants.Errors.ERROR_DELETE_FILE} {oldFile}: {ex.Message}",
                    AppConstants.Messages.OK);
            }
        }
    }

    private async void SetAnImageAsSourceAsync(Image image, string imageToSave)
    {      
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Images,
                PickerTitle = AppConstants.Messages.PICK_AN_IMAGE
            });

            if (result != null)
            {
                if (result.FileName.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
                    result.FileName.EndsWith("png", StringComparison.OrdinalIgnoreCase))
                {
                    var stream = await result.OpenReadAsync();
                    var memoryStream = new MemoryStream();
                    await stream.CopyToAsync(memoryStream);
                    memoryStream.Position = 0;
                    image.Source = ImageSource.FromStream(() => new MemoryStream(memoryStream.ToArray()));
                    image.IsVisible = true;

                    if (imageToSave == "_clothImagePath") _clothImagePath = result.FullPath;
                    else _bodyImagePath = result.FullPath;
                }
                else
                    await DisplayAlert(AppConstants.Errors.ERROR, AppConstants.Errors.SELECT_A_VALID_IMAGE, AppConstants.Messages.OK);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert(AppConstants.Errors.ERROR, $"{AppConstants.Errors.ERROR_OCCURRED}: {ex.Message}", AppConstants.Messages.OK);
        }

        PanelButtons.IsVisible = false;
    }

#if ANDROID
    private async void CheckAvailableMasksAndroid(bool toSet)
    {
        //премахваме стари записи
        ListAvailableClothesForMacros.Clear();

        // Проверка за разрешения
        App.Current?.Handler.MauiContext?.Services.GetService<CheckForAndroidPermissions>()?.CheckStorage();

        try
        {
            var fileChecker = App.Current?.Handler.MauiContext?.Services.GetService<IFileChecker>();

            var fileExists = await fileChecker.CheckFileExistsAsync(AppConstants.ImagesConstants.DRESS_MASK);
            if (fileExists) ListAvailableClothesForMacros.Add( new JacketModel { ImagePath = "Macros/icons_dress.png", Data = "dress" });

            fileExists = await fileChecker.CheckFileExistsAsync(AppConstants.ImagesConstants.DRESS_FULL_MASK);
            if (fileExists) ListAvailableClothesForMacros.Add( new JacketModel { ImagePath = "Macros/icons_dress_full.png", Data = "dress_full" });

            fileExists = await fileChecker.CheckFileExistsAsync(AppConstants.ImagesConstants.JACKET_MASK);
            if (fileExists) ListAvailableClothesForMacros.Add( new JacketModel { ImagePath = "Macros/icons_jacket.png", Data = "jacket" });

            fileExists = await fileChecker.CheckFileExistsAsync(AppConstants.ImagesConstants.CLOSED_JACKET_MASK);
            if (fileExists) ListAvailableClothesForMacros.Add( new JacketModel { ImagePath = "Macros/icons_jacket_closed.png", Data = "jacket_closed"  });

            fileExists = await fileChecker.CheckFileExistsAsync(AppConstants.ImagesConstants.OPEN_JACKET_MASK);
            if (fileExists) ListAvailableClothesForMacros.Add( new JacketModel { ImagePath = "Macros/icons_jacket_open.png", Data = "jacket_open" });

            fileExists = await fileChecker.CheckFileExistsAsync(AppConstants.ImagesConstants.NO_SET_MASK );
            if (fileExists) ListAvailableClothesForMacros.Add( new JacketModel { ImagePath = "Macros/icons_no_set.png", Data = "no_set" });

            fileExists = await fileChecker.CheckFileExistsAsync(AppConstants.ImagesConstants.PANTS_MASK );
            if (fileExists) ListAvailableClothesForMacros.Add( new JacketModel { ImagePath = "Macros/icons_pants.png", Data = "pants" });

            fileExists = await fileChecker.CheckFileExistsAsync(AppConstants.ImagesConstants.PANTS_SHORT_MASK );
            if (fileExists) ListAvailableClothesForMacros.Add( new JacketModel { ImagePath = "Macros/icons_pants_short.png", Data = "pants_short" });

            fileExists = await fileChecker.CheckFileExistsAsync(AppConstants.ImagesConstants.RAINCOAT_MASK );
            if (fileExists) ListAvailableClothesForMacros.Add( new JacketModel { ImagePath = "Macros/icons_raincoat.png", Data = "raincoat" });

            fileExists = await fileChecker.CheckFileExistsAsync(AppConstants.ImagesConstants.SHIRT_MASK );
            if (fileExists) ListAvailableClothesForMacros.Add( new JacketModel { ImagePath = "Macros/icons_shirt.png", Data = "shirt" });

            fileExists = await fileChecker.CheckFileExistsAsync(AppConstants.ImagesConstants.SKIRT_MASK );
            if (fileExists) ListAvailableClothesForMacros.Add( new JacketModel { ImagePath = "Macros/icons_skirt.png", Data = "skirt" });

            fileExists = await fileChecker.CheckFileExistsAsync(AppConstants.ImagesConstants.SKIRT_LONG_MASK );
            if (fileExists) ListAvailableClothesForMacros.Add( new JacketModel { ImagePath = "Macros/icons_skirt_long.png", Data = "skirt_long" });

            fileExists = await fileChecker.CheckFileExistsAsync(AppConstants.ImagesConstants.TANK_TOP_MASK );
            if (fileExists) ListAvailableClothesForMacros.Add( new JacketModel { ImagePath = "Macros/icons_tank_top.png", Data = "tank_top" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{AppConstants.Errors.ERROR_CKECK_MASKS}: {ex.Message}");
        }
    }

    private async void LoadLargeImage(string imageUri)
    {
        string imagePath = Path.Combine(
                AppConstants.Parameters.APP_BASE_PATH,
                AppConstants.Parameters.APP_NAME,
                AppConstants.Parameters.APP_FOLDER_MASK,
                imageUri);
        _bodyImagePath = imagePath;

        //await CopyFileToCacheAsync(imagePath);

        await singleImageLoader.LoadSingleImageAsync(imagePath);  // Зареждане на изображението асинхронно
    }
#endif

    public async Task CopyFileToCacheAsync(string sourcePath)//string imagePath)
    {        
        // Извличаме името на файла
        var fileName = Path.GetFileName(sourcePath);
        // Директорията за кеш на приложението
        var cacheDir = FileSystem.CacheDirectory;
        // Композиране на пълния път за копието
        var destPath = Path.Combine(cacheDir, fileName);

        // Копиране на файла
        using (var sourceStream = File.OpenRead(sourcePath))
        using (var destStream = File.Create(destPath))
        {
            await sourceStream.CopyToAsync(destStream);
        }

        _bodyImagePath = destPath;
    }


    //------------------------------------------ VISUAL EFECTS--------------------------------------------------------
    private void ToggleLoading(bool isLoading)
    {      
        CombineImagesButton.IsEnabled = !isLoading;
        LoadingIndicator.IsRunning = isLoading;
        LoadingIndicator.IsVisible = isLoading;
    }

    private void ReturnToCombinePageAfterBackOrSave()
    {
        ToggleLoading(false);

        SaveButton.IsVisible = false;
        SaveButton.IsEnabled = false;

        InputStack.IsVisible = true;
        ResultStack.IsVisible = false;

        _clothImagePath = String.Empty;
        _bodyImagePath = String.Empty;

        SelectedClothImage.Source = null;
        SelectedClothImage.Source = "Icons/blank_image_photo.png";
        SelectedBodyImage.Source = null;
        SelectedBodyImage.Source = "Icons/blank_image_photo.png";
        ResponseImage.Source = null;
        ResponseImage.Source = "Icons/blank_image_photo_2.png";
    }

    private void SetVisibilityOnCombineImagesButtonBasedOnTokens()
    {
        CombineImagesButton.IsVisible = AppSettings.Tokens > 0;
        CombineImagesButton.IsEnabled = AppSettings.Tokens > 0;
    }
}
