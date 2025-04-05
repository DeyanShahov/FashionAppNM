using FashionApp.core;
using FashionApp.core.services;
using FashionApp.Data.Constants;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Diagnostics;
//using Plugin.AdMob.Services;
//using Plugin.AdMob;

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
    public bool IsAdmin { get; set; } = false;

    private readonly SingleImageLoader singleImageLoader;
    private CameraService _cameraService;

    private int maskDetectionMethod = 1;

    private List<string> selectedItems = new List<string>();
    private List<string> selectedOptionsForZoneToMarcFromAI2 = new List<string>(); // Колекция с избрани стойности
    private List<string> selectedOptionsForZoneToMarcFromAI3 = new List<string>(); // Колекция с избрани стойности
    private List<string> selectedOptionsForZoneToMarcFromAI4 = new List<string>(); // Колекция с избрани стойности


    public ObservableCollection<JacketModel> Jackets { get; set; } = new ObservableCollection<JacketModel>();
    //public ICommand SelectJacketCommand { get; set; }

    private bool isFromClothImage = true;

    //private readonly IRewardedInterstitialAdService _rewardedInterstitialAdService;
    int tokens = 0;

    public CombineImages()
    {
        InitializeComponent();

        BindingContext = this;

        //_rewardedInterstitialAdService = FashionApp.core.services.ServiceProvider.GetRequiredService<IRewardedInterstitialAdService>();
        //_rewardedInterstitialAdService.OnAdLoaded += (_, __) => Debug.WriteLine("Rewarded interstitial ad prepared.");
        //_rewardedInterstitialAdService.PrepareAd(onUserEarnedReward: UserDidEarnReward);



        _cameraService = new CameraService(MyCameraView, CameraPanel);
        _cameraService.ImageCaptured += OnImageCaptured;
        _cameraService.StopCamera();

        singleImageLoader = new SingleImageLoader(
            setErrorMessage: (msg) => ResponseText.Text = msg,
            setImage: (uri) => SelectedBodyImage.Source = Microsoft.Maui.Controls.ImageSource.FromUri(new System.Uri(uri))
        );

        StandartMask.IsChecked = true;


#if WINDOWS
        CheckAvailableMasksWindows();
#elif ANDROID
        CheckAvailableMasksAndroid(true);
#endif
        //SelectJacketCommand = new Command<string>(OnJacketSelected);

    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        ChoiseForMaskMethod.IsVisible = IsAdmin;

        PanelButtons.IsVisible = false;
        PanelButtons2.IsVisible = false;
    }

    //---------------------------------------- BUTONS ACTIONS --------------------------------------------------------------
    private async void OnNavigateClicked(object sender, EventArgs e) => await Navigation.PopAsync();
    private void OnSelectClothImageClicked(object sender, EventArgs e) => SetAnImageAsSourceAsync(SelectedClothImage, nameof(_clothImagePath));
    private void OnSelectBodyImageClicked(object sender, EventArgs e) => SetAnImageAsSourceAsync(SelectedBodyImage, nameof(_bodyImagePath));
    private void OnCaptureClicked(object sender, EventArgs e)
    {
        CameraButtonsPanel.IsEnabled = false;
        _cameraService.CaptureClicked();
    }
    private void OnSelectedClothImageTapped(object sender, TappedEventArgs e) => PanelButtons.IsVisible = !PanelButtons.IsVisible;
    private void OnSelectedBodyImageTapped(object sender, TappedEventArgs e) => PanelButtons2.IsVisible = !PanelButtons2.IsVisible;

    private void OptionButton_Clicked(object sender, EventArgs e)
    {
        Button clickedButton = (Button)sender;
        string value = clickedButton.CommandParameter as string;

        if (string.IsNullOrEmpty(value)) return; // Предпазване от грешки

        if(maskDetectionMethod == 2)
        {
            if (selectedOptionsForZoneToMarcFromAI2.Contains(value))
            {
                // Премахване от списъка
                selectedOptionsForZoneToMarcFromAI2.Remove(value);
                clickedButton.BackgroundColor = Colors.Black;
                clickedButton.TextColor = Colors.Red;
            }
            else
            {
                // Добавяне в списъка
                selectedOptionsForZoneToMarcFromAI2.Add(value);
                clickedButton.BackgroundColor = Colors.White;
                clickedButton.TextColor = Colors.Black;
            }
        }
        else if (maskDetectionMethod == 3)
        {
            if (selectedOptionsForZoneToMarcFromAI3.Contains(value))
            {
                // Премахване от списъка
                selectedOptionsForZoneToMarcFromAI3.Remove(value);
                clickedButton.BackgroundColor = Colors.LightBlue;
                clickedButton.TextColor = Colors.Red;
            }
            else
            {
                // Добавяне в списъка
                selectedOptionsForZoneToMarcFromAI3.Add(value);
                clickedButton.BackgroundColor = Colors.White;
                clickedButton.TextColor = Colors.Black;
            }
        }
        else if (maskDetectionMethod == 4)
        {
            if (selectedOptionsForZoneToMarcFromAI4.Contains(value))
            {
                // Премахване от списъка
                selectedOptionsForZoneToMarcFromAI4.Remove(value);
                clickedButton.BackgroundColor = Colors.Orange;
                clickedButton.TextColor = Colors.Red;
            }
            else
            {
                // Добавяне в списъка
                selectedOptionsForZoneToMarcFromAI4.Add(value);
                clickedButton.BackgroundColor = Colors.White;
                clickedButton.TextColor = Colors.Black;
            }
        }    
    }


    private void OnCreateRewardedInterstitialClicked(object sender, EventArgs e)
    {
        //var rewardedInterstitialAd = _rewardedInterstitialAdService.CreateAd();
        //rewardedInterstitialAd.OnUserEarnedReward += (_, reward) =>
        //{
        //    UserDidEarnReward(reward);
        //};
        //rewardedInterstitialAd.OnAdLoaded += RewardedInterstitialAd_OnAdLoaded;
        //rewardedInterstitialAd.Load();
    }

    //private void RewardedInterstitialAd_OnAdLoaded(object? sender, EventArgs e)
    //{
    //    if (sender is IRewardedInterstitialAd rewardedInterstitialAd)
    //    {
    //        rewardedInterstitialAd.Show();
    //    }
    //}

    //private async void UserDidEarnReward(RewardItem rewardItem)
    //{
    //    Debug.WriteLine($"User earned {rewardItem.Amount} {rewardItem.Type}.");
    //    tokens += rewardItem.Amount;

    //    GoogleAdsButton.IsEnabled = false;
    //    GoogleAdsButton.IsVisible = false;

    //    CombineImagesButton.IsEnabled = true;
    //    CombineImagesButton.IsVisible = true;
    //    //await CombineImagesAction();
    //}

    private async void OnCombineImages_Clicked(object sender, EventArgs e)
    {
        await CombineImagesAction();
    }

    private async Task CombineImagesAction()
    {
        if (tokens < 10) return;

        var result1 = SelectedClothImage.Source.ToString()?.Remove(0, 6);
        var result2 = SelectedBodyImage.Source.ToString()?.Remove(0, 6);
        if (result1 == "Icons/blank_image_photo.png" || result2 == "Icons/blank_image_photo.png")
        {
            await DisplayAlert(AppConstants.Errors.ERROR, AppConstants.Errors.SELECT_BOTH_IMAGES, AppConstants.Messages.OK);
            return;
        }

        try
        {
            InputStack.IsVisible = false;
            ResultStack.IsVisible = true;

            ToggleLoading(true);

            //ResponseImage.IsVisible = toSet;
            SaveButton.IsVisible = false;
            SaveButton.IsEnabled = false;

            ResponseText.IsVisible = false;

            if (!await UploadImages()) return; // Качваме двата файла

            // Изпращаме POST заявка към API
            await Task.Delay(5000); // Изчакване от 5 секунди

            var requestUrl = $"{ApiUrl}/{AppConstants.Parameters.CONFY_FUNCTION_COMBINE_ENDPOINT}";
            var requestBody = new
            {
                function_name = AppConstants.Parameters.CONFY_FUNCTION_GENERATE_NAME,
                cloth_image = AppConstants.Parameters.INPUT_IMAGE_CLOTH,
                body_image = AppConstants.Parameters.INPUT_IMAGE_BODY,
                mask_detection_method = maskDetectionMethod, // Задаване типа на маската: ръчна ( true ) / АI ( false )
                args = GetNeededCollectionOfButtons(maskDetectionMethod) // Списъка с евентуалните зони за маркиране от АЙ-то
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

            tokens -= 10;

            GoogleAdsButton.IsEnabled = true;
            GoogleAdsButton.IsVisible = true;

            CombineImagesButton.IsEnabled = false;
            CombineImagesButton.IsVisible = false;
        }
    }

    private List<string> GetNeededCollectionOfButtons(int maskDetectionMethod)
    {
        switch (maskDetectionMethod)
        {
            case 2: return selectedOptionsForZoneToMarcFromAI2;
            case 3: return selectedOptionsForZoneToMarcFromAI3;
            case 4: return selectedOptionsForZoneToMarcFromAI4;
            default: return new List<string>();
        }
    }

    private void PanelButton_Clicked(object sender, EventArgs e)
    {
        isFromClothImage = true;
        HideMenus();
        _cameraService.StartCamera();
    }
    private void PanelButton6_Clicked(object sender, EventArgs e)
    {
        isFromClothImage = false;
        HideMenus();
        _cameraService.StartCamera();
    }

    private void HidePanelCommand(object sender, EventArgs e)
    {
        HideMenus();
        PanelButtons.IsVisible = false;
        PanelButtons2.IsVisible = false;
        _cameraService.StopCamera();
    }

    private void OnBackButtonClicked(object sender, EventArgs e)
    {
        ReturnToCombinePageAfterBackOrSave();
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
    private async void OpenJacketImageButton_Clicked(object sender, EventArgs e)
    {
        SetActiveButton((ImageButton)sender);
#if WINDOWS
        await LoadCorrectImageMaskWindows(AppConstants.ImagesConstants.OPEN_JACKET_MASK);
#elif ANDROID
        LoadLargeImage(AppConstants.ImagesConstants.OPEN_JACKET_MASK);
#endif
    }
    private async void ClosedJacketImageButton_Clicked(object sender, EventArgs e)
    {      
        SetActiveButton((ImageButton)sender);
#if WINDOWS
        await LoadCorrectImageMaskWindows(AppConstants.ImagesConstants.CLOSED_JACKET_MASK);
#elif ANDROID
        LoadLargeImage(AppConstants.ImagesConstants.CLOSED_JACKET_MASK);
#endif
    }

    private void MacroButton_Clicked(object sender, EventArgs e)
    {
        ImageButton clickedButton = (ImageButton)sender;
        string value = (string)clickedButton.CommandParameter;

        if (string.IsNullOrEmpty(value)) return;

        //SetActiveButton((ImageButton)sender);

        string? futureImageName = new MacroIconToPhoto(value).Value;

        if (futureImageName == null || futureImageName == "Default") return;

#if ANDROID
        LoadLargeImage(futureImageName);
#endif
    }

    private void SelectClothFromApp_Clicked(object sender, EventArgs e)
    {

    }

    private void SelectBodyFromApp_Clicked(object sender, EventArgs e)
    {

    }


    // Общият метод за CheckedChanged за всички RadioButton-и
    private void RadioButton_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        // Изпълняваме кода само ако бутонът е избран (e.Value == true)
        if (e.Value)
        {
            var radioButton = sender as RadioButton;
            if (radioButton == null) return;     
            
            string selectedContent = radioButton.Content?.ToString();

            // Разграничаваме кой бутон е избран чрез неговия текст
            switch (selectedContent)
            {
                case "Mask": ExecuteStandartMaskFunction(); break;
                case "AIv1": ExecuteAIv1Function(); break;
                case "AIv2": ExecuteAIv2Function(); break;
                case "AIv3": ExecuteAIv3Function(); break;
                default: break;
            }
        }
    }

    private void ExecuteStandartMaskFunction() => SetButtonForAiMasking(1, false);
    private void ExecuteAIv1Function() => SetButtonForAiMasking(2, true);
    private void ExecuteAIv2Function() => SetButtonForAiMasking(3, true);
    private void ExecuteAIv3Function() => SetButtonForAiMasking(4, true);
    private void SetButtonForAiMasking(int sender, bool toSet)
    {
        ButtonPanel.IsVisible = false;
        ButtonPanel2.IsVisible = false;
        ButtonPanel3.IsVisible = false;     

        maskDetectionMethod = sender;
        switch (sender)
        {
            case 2: ButtonPanel.IsVisible = true; break;
            case 3: ButtonPanel2.IsVisible = true; break;
            case 4: ButtonPanel3.IsVisible = true; break;
            default: break;
        }
#if ANDROID
        CheckAvailableMasksAndroid(!toSet);
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
    private async Task<string> SetCameraImageRealPathForSelectedClothImage(Stream stream)
    {
        DeleteTemporaryImage(); // Изтриваме ако е имало предищно неизтрита снимка     
        if (stream.CanSeek) stream.Position = 0; // Ако потокът е seekable, може да се наложи да го ресетнете:

        string tempPath = Path.Combine(FileSystem.CacheDirectory, $"testImage_{DateTime.Now.Ticks}.png");
        using (var fileStream = File.Create(tempPath)) await stream.CopyToAsync(fileStream);
        return tempPath;
    }
    private async void OnImageCaptured(Stream imageStream)
    {
        await ProcessSelectedImage(imageStream);
        PanelButtons.IsVisible = false;
        PanelButtons2.IsVisible = false;
        _cameraService.StopCamera();
        HideMenus();
        CameraButtonsPanel.IsEnabled = true;
    }

    private async void TestGalleryButton_Clicked(object sender, EventArgs e)
    {
        isFromClothImage = true;
        var tempGallery = new TemporaryGallery();
        await Navigation.PushModalAsync(tempGallery);
        string selectedImageName = await tempGallery.ImageSelectedTask.Task;
        await ProcessSelectedImage(selectedImageName);
    }

    private async void TestGalleryButton5_Clicked(object sender, EventArgs e)
    {
        isFromClothImage = false;
        var tempGallery = new TemporaryGallery();
        await Navigation.PushModalAsync(tempGallery);
        string selectedImageName = await tempGallery.ImageSelectedTask.Task;
        await ProcessSelectedImage(selectedImageName);
    }
    private async Task ProcessSelectedImage(Stream? stream)
    {
        var resizedImageResult = await ImageStreamResize.ResizeImageStream(stream, 500, 700); // Преоразмеряване на изображението
        var resultPath = await SetCameraImageRealPathForSelectedClothImage(resizedImageResult.ResizedStream);

        if(isFromClothImage)
        {
            _clothImagePath = resultPath;
            SelectedClothImage.Source = ImageSource.FromFile(resultPath);
            SelectedClothImage.IsVisible = true;
        }
        else
        {
            _bodyImagePath = resultPath;
            SelectedBodyImage.Source = ImageSource.FromFile(resultPath);
            SelectedBodyImage.IsVisible = true;
        }
       
    }

    private async Task ProcessSelectedImage(string fileName)
    {
        if (isFromClothImage)
        {
            _clothImagePath = Path.Combine("Gallery", $"{fileName}.jpg");
            SelectedClothImage.Source = fileName;
            SelectedClothImage.IsVisible = true;
        }
        else
        {
            _bodyImagePath = Path.Combine("Gallery", $"{fileName}.jpg");
            SelectedBodyImage.Source = fileName;
            SelectedBodyImage.IsVisible = true;
        }
        
    }

    private async Task LoadCorrectImageMaskWindows(string maskName)
    {
        try
        {
            string filePath = Path.Combine("C:", "Users", "Public", "Pictures", maskName);

            if (File.Exists(filePath))
            {
                var stream = File.OpenRead(filePath);
                var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                SelectedBodyImage.Source = ImageSource.FromStream(() => new MemoryStream(memoryStream.ToArray()));
                //SelectedBodyImage.IsVisible = true;
                _bodyImagePath = filePath; // Запазваме пътя на избрания файл
            }
            else
            {
                await DisplayAlert(
                    AppConstants.Errors.ERROR,
                    $"{AppConstants.Errors.FILE_NOT_FOUND}: {maskName}",
                    AppConstants.Messages.OK);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                AppConstants.Errors.ERROR, 
                $"{AppConstants.Errors.ERROR_OCCURRED}: {ex.Message}",
                AppConstants.Messages.OK);
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
        PanelButtons2.IsVisible = false;
    }

#if WINDOWS
    private void CheckAvailableMasksWindows()
    {
        try
        {
            string picturesPath = Path.Combine("C:", "Users", "Public", "Pictures");
            string closedJacketMaskPath = Path.Combine(picturesPath, "closed_jacket_mask.png");
            string openJacketMaskPath = Path.Combine(picturesPath, "open_jacket_mask.png");

            // Проверка за closed_jacket_mask.png
            if (File.Exists(closedJacketMaskPath))
            {
                ClosedJacketImageButton.IsEnabled = true;
                ClosedJacketImageButton.IsVisible = true;
            }

            // Проверка за open_jacket_mask.png
            if (File.Exists(openJacketMaskPath))
            {
                OpenJacketImageButton.IsEnabled = true;
                OpenJacketImageButton.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            // В случай на грешка при достъп до файловата система
            Console.WriteLine($"Error checking masks: {ex.Message}");
        }
    }

#elif ANDROID
    private async void CheckAvailableMasksAndroid(bool toSet)
    {
        //премахваме стари записи
        Jackets.Clear();

        // Проверка за разрешения
        App.Current?.Handler.MauiContext?.Services.GetService<CheckForAndroidPermissions>()?.CheckStorage();

        try
        {
            var fileChecker = App.Current?.Handler.MauiContext?.Services.GetService<IFileChecker>();

            var fileExists = await fileChecker.CheckFileExistsAsync(AppConstants.ImagesConstants.DRESS_MASK);
            if (fileExists) Jackets.Add( new JacketModel { ImagePath = "Macros/icons_dress.png", Data = "dress" });

            fileExists = await fileChecker.CheckFileExistsAsync(AppConstants.ImagesConstants.DRESS_FULL_MASK);
            if (fileExists) Jackets.Add( new JacketModel { ImagePath = "Macros/icons_dress_full.png", Data = "dress_full" });

            fileExists = await fileChecker.CheckFileExistsAsync(AppConstants.ImagesConstants.JACKET_MASK);
            if (fileExists) Jackets.Add( new JacketModel { ImagePath = "Macros/icons_jacket.png", Data = "jacket" });

            fileExists = await fileChecker.CheckFileExistsAsync(AppConstants.ImagesConstants.CLOSED_JACKET_MASK);
            if (fileExists) Jackets.Add( new JacketModel { ImagePath = "Macros/icons_jacket_closed.png", Data = "jacket_closed"  });

            fileExists = await fileChecker.CheckFileExistsAsync(AppConstants.ImagesConstants.OPEN_JACKET_MASK);
            if (fileExists) Jackets.Add( new JacketModel { ImagePath = "Macros/icons_jacket_open.png", Data = "jacket_open" });

            fileExists = await fileChecker.CheckFileExistsAsync(AppConstants.ImagesConstants.NO_SET_MASK );
            if (fileExists) Jackets.Add( new JacketModel { ImagePath = "Macros/icons_no_set.png", Data = "no_set" });

            fileExists = await fileChecker.CheckFileExistsAsync(AppConstants.ImagesConstants.PANTS_MASK );
            if (fileExists) Jackets.Add( new JacketModel { ImagePath = "Macros/icons_pants.png", Data = "pants" });

            fileExists = await fileChecker.CheckFileExistsAsync(AppConstants.ImagesConstants.PANTS_SHORT_MASK );
            if (fileExists) Jackets.Add( new JacketModel { ImagePath = "Macros/icons_pants_short.png", Data = "pants_short" });

            fileExists = await fileChecker.CheckFileExistsAsync(AppConstants.ImagesConstants.RAINCOAT_MASK );
            if (fileExists) Jackets.Add( new JacketModel { ImagePath = "Macros/icons_raincoat.png", Data = "raincoat" });

            fileExists = await fileChecker.CheckFileExistsAsync(AppConstants.ImagesConstants.SHIRT_MASK );
            if (fileExists) Jackets.Add( new JacketModel { ImagePath = "Macros/icons_shirt.png", Data = "shirt" });

            fileExists = await fileChecker.CheckFileExistsAsync(AppConstants.ImagesConstants.SKIRT_MASK );
            if (fileExists) Jackets.Add( new JacketModel { ImagePath = "Macros/icons_skirt.png", Data = "skirt" });

            fileExists = await fileChecker.CheckFileExistsAsync(AppConstants.ImagesConstants.SKIRT_LONG_MASK );
            if (fileExists) Jackets.Add( new JacketModel { ImagePath = "Macros/icons_skirt_long.png", Data = "skirt_long" });

            fileExists = await fileChecker.CheckFileExistsAsync(AppConstants.ImagesConstants.TANK_TOP_MASK );
            if (fileExists) Jackets.Add( new JacketModel { ImagePath = "Macros/icons_tank_top.png", Data = "tank_top" });
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

        await CopyFileToCacheAsync(imagePath);

        await singleImageLoader.LoadSingleImageAsync(imagePath);  // Зареждане на изображението асинхронно
    }
#endif

    public async Task CopyFileToCacheAsync(string sourcePath)//string imagePath)
    {
        //try
        //{
        //    var cacheDir = FileSystem.CacheDirectory;
        //    var fileName = Path.GetFileName(imagePath);
        //    var newFilePath = Path.Combine(cacheDir, fileName);
        //    if (!File.Exists(newFilePath))
        //    {
        //        using var sourceStream = File.OpenRead(imagePath);
        //        using var destinationStream = File.Create(newFilePath);
        //        await sourceStream.CopyToAsync(destinationStream);
        //    }
        //}
        //catch (Exception ex)
        //{
        //    Console.WriteLine($"{AppConstants.Errors.ERROR_COPY_FILE}: {ex.Message}");
        //}

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

    private void SetActiveButton(ImageButton activeButton)
    {
        // Списък с всички бутони, които трябва да се управляват
        var allButtons = new List<ImageButton>
        {
            ClosedJacketImageButton,
            OpenJacketImageButton
            // Тук можете да добавите нови бутони в бъдеще
        };

        // Премахваме ефектите от всички бутони
        foreach (var button in allButtons)
        {
            button.BackgroundColor = Colors.Transparent;
            button.BorderColor = Colors.Black;
            button.BorderWidth = 3;
            button.Scale = 1.0;
        }

        // Прилагаме ефектите само на активния бутон
        if (activeButton != null)
        {
            activeButton.BackgroundColor = Color.FromArgb("#E6F3FF");
            activeButton.BorderColor = Color.FromArgb("#0066CC");
            activeButton.BorderWidth = 4;
            activeButton.Scale = 1.1;
        }
    }
    private void HideMenus() => Menu1.IsVisible = !Menu1.IsVisible; 
}
