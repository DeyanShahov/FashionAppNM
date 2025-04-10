// Добавяме using за новите услуги
using FashionApp.core;
using FashionApp.core.services;
using FashionApp.Data.Constants;
using System.Collections.ObjectModel;

namespace FashionApp.Pages;

public partial class CombineImages : ContentPage
{
    // Премахваме HttpClient и ApiUrl
    // Премахваме _imageData, ще използваме резултата от API услугата

    private string _clothImagePath = String.Empty;
    private string _bodyImagePath = String.Empty;
    private byte[]? _resultImageData = null; // Съхранява резултата от API

    // Инжектирани услуги
    private readonly IImageSelectionService _imageSelectionService;
    private readonly ICombineApiService _combineApiService; // <- Добавяме
    private readonly IImageSavingService _imageSavingService; // <- Добавяме
    private readonly IMaskManagementService _maskManagementService; // <- Добавяме
    private readonly CameraService _cameraService;
    private readonly ExecutionGuardService _executionGuardService;
    private readonly Settings _appSettings;

    public ObservableCollection<JacketModel> ListAvailableClothesForMacros { get; set; } = new ObservableCollection<JacketModel>();

    private bool isFromClothImage = true;

    public CombineImages(
        ExecutionGuardService executionGuard,
        Settings settings,
        IImageSelectionService imageSelectionService,
        ICombineApiService combineApiService, // <- Инжектираме
        IImageSavingService imageSavingService, // <- Инжектираме
        IMaskManagementService maskManagementService // <- Инжектираме
        )
    {
        InitializeComponent();

        // Запазваме инжектираните инстанции
        _appSettings = settings;
        _executionGuardService = executionGuard;
        _imageSelectionService = imageSelectionService;
        _combineApiService = combineApiService;
        _imageSavingService = imageSavingService;
        _maskManagementService = maskManagementService;

        BindingContext = this;

        _cameraService = new CameraService(MyCameraView, CameraPanel);
       // _cameraService.ImageCaptured += OnImageCaptured;
        _cameraService.StopCamera();

        LoadAvailableMasks(); // Извикваме новия метод за зареждане на маски
    }

    protected override async void OnAppearing() // Направихме го async
    {
        base.OnAppearing();
        LabelToken.Text = $"TOKENS : {_appSettings.Tokens}";
        SetVisibilityOnCombineImagesButtonBasedOnTokens();
        PanelButtons.IsVisible = false;
        PanelButtons2.IsVisible = false;
        _imageSelectionService?.DeleteTemporaryImages();
        await LoadAvailableMasks(); // Презареждаме маските при всяко показване
    }

    // --- Основна логика за комбиниране ---
    private async void OnCombineImages_Clicked(object sender, EventArgs e)
    {
        // Проверка дали са избрани изображения (може да се направи по-надеждно)
        if (string.IsNullOrEmpty(_clothImagePath) || string.IsNullOrEmpty(_bodyImagePath) ||
            SelectedClothImage.Source == null || SelectedBodyImage.Source == null ||
            SelectedClothImage.Source.ToString()?.Contains("blank_image_photo.png") == true ||
            SelectedBodyImage.Source.ToString()?.Contains("blank_image_photo.png") == true)
        {
            await DisplayAlert(AppConstants.Errors.ERROR, AppConstants.Errors.SELECT_BOTH_IMAGES, AppConstants.Messages.OK);
            return;
        }

        if (_appSettings.Tokens <= 0)
        {
            await DisplayAlert(AppConstants.Errors.ERROR, "AppConstants.Errors.NO_TOKENS", AppConstants.Messages.OK);
            return;
        }

        // Показваме резултатния изглед и зареждането
        InputStack.IsVisible = false;
        ResultStack.IsVisible = true;
        ToggleLoading(true);
        SaveButton.IsVisible = false; // Скриваме бутона за запазване
        ResponseText.IsVisible = false; // Скриваме текстовото поле за грешки/отговори
        _resultImageData = null; // Нулираме предишни данни

        try
        {
            // Извикваме API услугата
            // TODO: Трябва да се вземе решение как се управлява useManualMask и aiArgs
            bool useManualMask = true; // Примерно
            List<string>? aiArgs = null; // Примерно

            CombineApiResult result = await _combineApiService.CombineImagesAsync(_clothImagePath, _bodyImagePath, useManualMask, aiArgs);

            if (result.Success)
            {
                if (result.IsImageResult && result.ImageData != null)
                {
                    // Успех, имаме изображение
                    _resultImageData = result.ImageData; // Запазваме данните
                    ResponseImage.Source = ImageSource.FromStream(() => new MemoryStream(_resultImageData));
                    SaveButton.IsVisible = true; // Показваме бутона за запазване
                    SaveButton.IsEnabled = true;
                }
                else
                {
                    // Успех, но API-то е върнало текст (грешка или съобщение)
                    ResponseText.Text = result.ErrorMessage ?? "AppConstants.Errors.UNKNOWN_API_RESPONSE";
                    ResponseText.IsVisible = true;
                    ResponseImage.Source = "Icons/blank_image_photo_2.png"; // Показваме празна икона
                }
            }
            else
            {
                // Грешка при извикване на API
                ResponseText.Text = result.ErrorMessage ?? "AppConstants.Errors.UNKNOWN_API_ERROR";
                ResponseText.IsVisible = true;
                ResponseImage.Source = "Icons/blank_image_photo_2.png";
            }
        }
        catch (Exception ex) // Неочаквана грешка
        {
            ResponseText.Text = $"{"AppConstants.Errors.UNEXPECTED_ERROR"}: {ex.Message}";
            ResponseText.IsVisible = true;
            ResponseImage.Source = "Icons/blank_image_photo_2.png";
        }
        finally
        {
            ToggleLoading(false); // Спираме индикатора за зареждане

            // Намаляваме токените само ако API извикването е било успешно (дори и да е върнало текст вместо изображение)
            // Това зависи от вашата бизнес логика.
            if (_resultImageData != null) // Намаляваме само ако има успешно генерирано изображение
            {
                _appSettings.Tokens -= 1;
            }
            // Ако искате да намалявате токени дори при грешка от API-то, преместете го извън if-а.


            LabelToken.Text = $"TOKENS : {_appSettings.Tokens}";
            GoogleAdsButton.IsEnabled = true; // Винаги активираме бутона за реклама след опит
            GoogleAdsButton.IsVisible = true;
            SetVisibilityOnCombineImagesButtonBasedOnTokens(); // Обновяваме видимостта на бутона за комбиниране
            _imageSelectionService?.DeleteTemporaryImages(); // Изтриваме временните файлове след комбиниране
        }
    }

    // --- Запазване на изображение ---
    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (_resultImageData != null)
        {
            bool saved = await _imageSavingService.SaveImageAsync(_resultImageData);
            if (saved)
            {
                ReturnToCombinePageAfterBackOrSave(); // Връщаме към началния изглед само ако е запазено успешно
            }
            // Ако не е запазено, _displayAlert ще се покаже от услугата
        }
    }

    // --- Зареждане на маски (използва новата услуга) ---
    private async Task LoadAvailableMasks()
    {
        if (_maskManagementService != null)
        {
            await _maskManagementService.LoadAvailableMasksAsync(ListAvailableClothesForMacros);
        }
    }

    // --- Избор на маска (използва новата услуга) ---
    private async void MacroButton_Clicked(object sender, EventArgs e)
    {
        if (sender is not ImageButton clickedButton) return;
        if (clickedButton.CommandParameter is not string maskIdentifier || string.IsNullOrEmpty(maskIdentifier)) return;

        ToggleLoading(true); // Показваме индикатор, докато копираме/зареждаме

        try
        {
#if ANDROID
            // Копираме файла от основната директория в кеша (ако вече не е там)
            await _maskManagementService.CopyMaskToCacheAsync(maskIdentifier);
            // Взимаме пътя от кеша
            var cachedMaskPath = Path.Combine(FileSystem.CacheDirectory, (await _maskManagementService.GetMaskPathAsync(maskIdentifier)) ?? string.Empty);


            if (!string.IsNullOrEmpty(cachedMaskPath) && File.Exists(cachedMaskPath))
            {
                // Зареждаме изображението от кеширания път
                UpdateBodyImage(ImageSource.FromFile(cachedMaskPath), cachedMaskPath);
            }
            else
            {
                await DisplayAlert(AppConstants.Errors.ERROR, "AppConstants.Errors.ERROR_LOADING_MASK", AppConstants.Messages.OK);
            }
#else
            await DisplayAlert("Not Supported", "Mask selection is only supported on Android.", AppConstants.Messages.OK);
#endif
        }
        catch (Exception ex)
        {
            await DisplayAlert(AppConstants.Errors.ERROR, $"{"AppConstants.Errors.ERROR_PROCESSING_MASK_SELECTION"}: {ex.Message}", AppConstants.Messages.OK);
        }
        finally
        {
            ToggleLoading(false); // Скриваме индикатора
        }
    }


    // --- Останалите UI методи остават почти същите ---
    private async void OnNavigateClicked(object sender, EventArgs e) => await Navigation.PopAsync();
    private void OnSelectedClothImageTapped(object sender, TappedEventArgs e) => PanelButtons.IsVisible = !PanelButtons.IsVisible;
    private void OnSelectedBodyImageTapped(object sender, TappedEventArgs e) => PanelButtons2.IsVisible = !PanelButtons2.IsVisible;
    private async void OnCreateRewardedInterstitialClicked(object sender, EventArgs e) {/* ... */ }
    private void OnBackButtonClicked(object sender, EventArgs e) => ReturnToCombinePageAfterBackOrSave();
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
        _resultImageData = null; // Нулираме и данните

        // Нулираме източниците на изображения
        SelectedClothImage.Source = "Icons/blank_image_photo.png";
        SelectedBodyImage.Source = "Icons/blank_image_photo.png";
        ResponseImage.Source = "Icons/blank_image_photo_2.png";

        PanelButtons.IsVisible = false; // Скриваме панелите с бутони
        PanelButtons2.IsVisible = false;

        _imageSelectionService?.DeleteTemporaryImages(); // Изтриваме временните файлове
    }
    private void SetVisibilityOnCombineImagesButtonBasedOnTokens()
    {
        CombineImagesButton.IsVisible = _appSettings.Tokens > 0;
        CombineImagesButton.IsEnabled = _appSettings.Tokens > 0;
    }

    // Тези методи вече са имплементирани по-горе
    // private void PanelButton_Clicked(object sender, EventArgs e) { ... }
    // private void PanelButton6_Clicked(object sender, EventArgs e) { ... }
    // private void HidePanelCommand(object sender, EventArgs e) { ... }
    // private void ShowCamera() { ... }
    // private void HideCamera() { ... }
    // private void UpdateClothImage(ImageSource source, string path) { ... }
    // private void UpdateBodyImage(ImageSource source, string path) { ... }
    // private async void OnImageCaptured(Stream imageStream) { ... }

    // Премахваме остарелите методи, които вече не се използват директно
    // или са заменени от логика в услугите.
}