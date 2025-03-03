using System.Text.Json;
using System.Text;
using FashionApp.core.services;
using FashionApp.Data.Constants;
using FashionApp.core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FashionApp.Pages;

public partial class CombineImages : ContentPage
{
    private readonly HttpClient _client = new HttpClient { Timeout = TimeSpan.FromSeconds(300) };
    private const string ApiUrl = "https://eminently-verified-walleye.ngrok-free.app";
    //private const string ApiUrl = "http://192.168.0.101:80";
    //private const string ApiUrl = "http://127.0.0.1:80";
    private byte[] _imageData = [];
    private string _clothImagePath = String.Empty;
    private string _bodyImagePath = String.Empty;

    private readonly SingleImageLoader singleImageLoader;
    private CameraService _cameraService;

    private bool isCustomMask = true;

    private List<string> selectedItems = new List<string>();
    private List<string> selectedOptionsForZoneToMarcFromAI = new List<string>(); // Колекция с избрани стойности

    public CombineImages()
    {
        InitializeComponent();
        _cameraService = new CameraService(MyCameraView, CameraPanel);
        _cameraService.ImageCaptured += OnImageCaptured;
        _cameraService.StopCamera();

        singleImageLoader = new SingleImageLoader(
            setErrorMessage: (msg) => ResponseText.Text = msg,
            setImage: (uri) => SelectedBodyImage.Source = Microsoft.Maui.Controls.ImageSource.FromUri(new System.Uri(uri))
        );


#if WINDOWS
        CheckAvailableMasksWindows();
#elif ANDROID
        CheckAvailableMasksAndroid(true);
#endif
    }

    //---------------------------------------- BUTONS ACTIONS --------------------------------------------------------------
    private async void OnNavigateClicked(object sender, EventArgs e) => await Navigation.PopAsync();
    private void OnSelectClothImageClicked(object sender, EventArgs e) => SetAnImageAsSourceAsync(SelectedClothImage, nameof(_clothImagePath));
    private void OnSelectBodyImageClicked(object sender, EventArgs e) => SetAnImageAsSourceAsync(SelectedBodyImage, nameof(_bodyImagePath));
    private void OnCaptureClicked(object sender, EventArgs e) => _cameraService.CaptureClicked();
    private void MaskTypeSwitch_Toggled(object sender, ToggledEventArgs e)
    {
        ButtonPanel.IsVisible = e.Value;
        isCustomMask = !e.Value;
        SelectBodyImageButton.Text = "Select Body Image - " + (e.Value ? "AI Mask"  : "Custom Mask");

#if ANDROID
        CheckAvailableMasksAndroid(!e.Value);
#endif
    }
    private void OptionButton_Clicked(object sender, EventArgs e)
    {
        Button clickedButton = (Button)sender;
        string value = clickedButton.CommandParameter as string;

        if (string.IsNullOrEmpty(value)) return; // Предпазване от грешки

        if (selectedOptionsForZoneToMarcFromAI.Contains(value))
        {
            // Премахване от списъка
            selectedOptionsForZoneToMarcFromAI.Remove(value);
            clickedButton.BackgroundColor = Colors.Black;
            clickedButton.TextColor = Colors.Red;
        }
        else
        {
            // Добавяне в списъка
            selectedOptionsForZoneToMarcFromAI.Add(value);
            clickedButton.BackgroundColor = Colors.White;
            clickedButton.TextColor = Colors.Black;
        }
    }

    private async void OnCombineImages_Clicked(object sender, EventArgs e)
    {
        if (SelectedClothImage.Source == null || SelectedBodyImage.Source == null)
        {
            await DisplayAlert(AppConstants.Errors.ERROR, AppConstants.Errors.SELECT_BOTH_IMAGES, AppConstants.Messages.OK);
            return;
        }

        try
        {
            ToggleLoading(true);
            SetVisibilityForResult(false);
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
                is_custom_mask = isCustomMask, // Задаване типа на маската: ръчна ( true ) / АI ( false )
                args = selectedOptionsForZoneToMarcFromAI // Списъка с евентуалните зони за маркиране от АЙ-то
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
                SetVisibilityForResult(true);
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
    private void PanelButton_Clicked(object sender, EventArgs e)
    {
        HideMenus();
        _cameraService.StartCamera();
    }
    private void HidePanelCommand(object sender, EventArgs e)
    {
        HideMenus();
        _cameraService.StopCamera();
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
            await DisplayAlert(
                AppConstants.Errors.ERROR,
                $"{AppConstants.Errors.FAILED_TO_SAVE_IMAGE}: {ex.Message}",
                AppConstants.Messages.OK);
        }
    }
    private async void OpenJacketImageButton_Clicked(object sender, EventArgs e)
    {
        SetActiveButton(sender as ImageButton);
#if WINDOWS
        await LoadCorrectImageMaskWindows(AppConstants.ImagesConstants.OPEN_JACKET_MASK);
#elif ANDROID
        LoadLargeImage(AppConstants.ImagesConstants.OPEN_JACKET_MASK);
#endif
    }
    private async void ClosedJacketImageButton_Clicked(object sender, EventArgs e)
    {
        SetActiveButton(sender as ImageButton);
#if WINDOWS
        await LoadCorrectImageMaskWindows(AppConstants.ImagesConstants.CLOSED_JACKET_MASK);
#elif ANDROID
        LoadLargeImage(AppConstants.ImagesConstants.CLOSED_JACKET_MASK);
#endif
    }
    
    private void SelectClothFromApp_Clicked(object sender, EventArgs e)
    {

    }

    private void SelectBodyFromApp_Clicked(object sender, EventArgs e)
    {

    }

    //-------------------------------------------- FUNCTIONS --------------------------------------------------------- 
    private async Task<bool> UploadImages()
    {
        var uploader = new ComfyUIUploader(ApiUrl);
        if (!string.IsNullOrEmpty(_clothImagePath) && !string.IsNullOrEmpty(_bodyImagePath))
        {
            await uploader.UploadImageAsync(_clothImagePath, "input"); // Качваме първата картинка            
            await uploader.UploadImageAsync(_bodyImagePath, "input"); // Качваме втората картинка

            DeleteTemporaryImage();
            return true;
        }
        else
        {
            await DisplayAlert(AppConstants.Errors.ERROR, AppConstants.Errors.MISSING_PATH_TO_FILES, AppConstants.Messages.OK);
            return false;
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
        _cameraService.StopCamera();
        HideMenus();
    }
    private async Task ProcessSelectedImage(Stream? stream)
    {
        var resizedImageResult = await ImageStreamResize.ResizeImageStream(stream, 500, 700); // Преоразмеряване на изображението
        var resultPath = await SetCameraImageRealPathForSelectedClothImage(resizedImageResult.ResizedStream);
        _clothImagePath = resultPath;

        // Задаване на източника на изображението
        SelectedClothImage.Source = ImageSource.FromFile(resultPath);

        // Настройка на ширината и височината на изображението
        SelectedClothImage.WidthRequest = Application.Current.MainPage.Width;
        SelectedClothImage.HeightRequest = Application.Current.MainPage.Height;

        // Центриране на изображението
        SelectedClothImage.HorizontalOptions = LayoutOptions.Center;
        SelectedClothImage.VerticalOptions = LayoutOptions.Center;
        // Показване на елементите
        SelectedClothImage.IsVisible = true;
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
        // Проверка за разрешения
        App.Current?.Handler.MauiContext?.Services.GetService<CheckForAndroidPermissions>()?.CheckStorage();

        try
        {
            var fileChecker = App.Current?.Handler.MauiContext?.Services.GetService<IFileChecker>();

            var fileExists = await fileChecker.CheckFileExistsAsync(AppConstants.ImagesConstants.CLOSED_JACKET_MASK);
            if (fileExists)
            {
                ClosedJacketImageButton.IsEnabled = toSet;
                ClosedJacketImageButton.IsVisible = toSet;
            }

            fileExists = await fileChecker.CheckFileExistsAsync(AppConstants.ImagesConstants.OPEN_JACKET_MASK);
            if (fileExists)
            {
                OpenJacketImageButton.IsEnabled = toSet;
                OpenJacketImageButton.IsVisible = toSet;
            }
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
        await singleImageLoader.LoadSingleImageAsync(imagePath);  // Зареждане на изображението асинхронно
    }
#endif
    //------------------------------------------ VISUAL EFECTS--------------------------------------------------------
    private void ToggleLoading(bool isLoading)
    {      
        CombineImagesButton.IsEnabled = !isLoading;
        LoadingIndicator.IsRunning = isLoading;
        LoadingIndicator.IsVisible = isLoading;
    }
    private void SetVisibilityForResult(bool toSet)
    {
        ResponseImage.IsVisible = toSet;
        SaveButton.IsVisible = toSet;
        SaveButton.IsEnabled = toSet;
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
