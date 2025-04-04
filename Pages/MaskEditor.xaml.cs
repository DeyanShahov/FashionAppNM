using FashionApp.core;
using FashionApp.core.draw;
using FashionApp.core.services;
using FashionApp.Data.Constants;
using SkiaSharp;
using System.Text.RegularExpressions;

namespace FashionApp.Pages;

public partial class MaskEditor : ContentPage
{
    //public string DynamicParameter { get; set; }
    public bool _isAdmin { get; set; } = false;
    private readonly ExecutionGuardService _executionGuardService;

    private List<DrawingLine> _lines = new();
    private DrawingLine _currentLine = new();
    private readonly IDrawable _drawable;
    private string imageFileName = $"masked_image_{DateTime.Now:yyyyMMdd_HHmmss}.png";
    private readonly CameraService _cameraService;

    public MaskEditor(ExecutionGuardService executionGuard)
    {
        InitializeComponent();
        _executionGuardService = executionGuard;

        SetParameters(_isAdmin);

        _drawable = new DrawingViewDrawable(_lines);
        DrawingView.Drawable = _drawable;

        _cameraService = new CameraService(MyCameraView, CameraPanel);
        _cameraService.ImageCaptured += OnImageCaptured;
        _cameraService.StopCamera();
    }

    private void SetParameters(bool isAdmin)
    {
        if (isAdmin)
        {
            //SelectImageButton.IsVisible = isAdmin;
            TestGalleryPanel.IsVisible = isAdmin;
            MacroPanel.IsVisible = isAdmin;
        }
    }

    //private async void OnBackButtonClicked(object sender, EventArgs e) => await Navigation.PopAsync();
    private async void OnSelectImageClicked(object sender, EventArgs e)
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
                bool pageAlreadyExists = Navigation.ModalStack.Any(p => p is ImageEditPage);
                if (pageAlreadyExists) return;

                string taskKey = AppConstants.Pages.IMAGE_EDIT;
                if (!_executionGuardService.TryStartTask(taskKey)) return;

                try
                {
                    //await Navigation.PushModalAsync(new ImageEditPage(result.FullPath));
                    var page = MauiProgram.ServiceProvider.GetRequiredService<ImageEditPage>();
                    page.ImageUri = result.FullPath;
                    await Navigation.PushModalAsync(page);
                }
                finally
                {
                    _executionGuardService.FinishTask(taskKey);
                }
            }
            else
                await DisplayAlert(AppConstants.Errors.ERROR, AppConstants.Errors.SELECT_A_VALID_IMAGE, AppConstants.Messages.OK);

            //if (result != null)
            //{
            //    if (result.FileName.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
            //        result.FileName.EndsWith("png", StringComparison.OrdinalIgnoreCase))
            //    {
            //        var stream = await result.OpenReadAsync();
            //        await ProcessSelectedImage(stream);
            //    }
            //    else
            //        await DisplayAlert(AppConstants.Errors.ERROR, AppConstants.Errors.SELECT_A_VALID_IMAGE, AppConstants.Messages.OK);
            //}
        }
        catch (Exception ex)
        {
            await DisplayAlert(AppConstants.Errors.ERROR, $"{AppConstants.Errors.ERROR}: {ex.Message}", AppConstants.Messages.OK);
        }
    }
    //private async void OpenImageEditor_Clicked(object sender, EventArgs e)
    //{
    //    var file = await FilePicker.PickAsync(new PickOptions
    //    {
    //        FileTypes = FilePickerFileType.Images,
    //        PickerTitle = AppConstants.Messages.PICK_AN_IMAGE
    //    });

    //    // Отваряне на модалния прозорец с пътя на изображението
    //    if (file != null) await Navigation.PushModalAsync(new ImageEditPage(file.FullPath));
    //}

    private async void TestGalleryButton_Clicked(object sender, EventArgs e)
    {
        bool pageAlreadyExists1 = Navigation.ModalStack.Any(p => p is TemporaryGallery);
        if (pageAlreadyExists1) return;

        var tempGallery = new TemporaryGallery();
        await Navigation.PushModalAsync(tempGallery);
        string selectedImageName = await tempGallery.ImageSelectedTask.Task;




        //string taskKey2 = AppConstants.Pages.TEMP_GALLERY;
        //if (!_executionGuardService.TryStartTask(taskKey2)) return;

        //try
        //{
        //    //var tempGallery = new TemporaryGallery();
        //    //await Navigation.PushModalAsync(tempGallery);
        //    //string selectedImageName = await tempGallery.ImageSelectedTask.Task;
        //    var page = MauiProgram.ServiceProvider.GetRequiredService<TemporaryGallery>();
        //    await Navigation.PushModalAsync(page);
        //    string selectedImageName = await page.ImageSelectedTask.Task;
        //}
        //finally
        //{
        //    _executionGuardService.FinishTask(taskKey2);
        //}



        await Task.Delay(1);

        // Ако има избрано изображение, отваряме ImageEditPage
        if (!string.IsNullOrEmpty(selectedImageName))
        {
            // await Navigation.PushModalAsync(new ImageEditPage(selectedImageName));

            bool pageAlreadyExists = Navigation.ModalStack.Any(p => p is ImageEditPage);
            if (pageAlreadyExists) return;

            string taskKey = AppConstants.Pages.IMAGE_EDIT;
            if (!_executionGuardService.TryStartTask(taskKey)) return;

            try
            {
                // await Navigation.PushModalAsync(new ImageEditPage(selectedImageName));
                var page = MauiProgram.ServiceProvider.GetRequiredService<ImageEditPage>();
                page.ImageUri = selectedImageName;
                await Navigation.PushModalAsync(page);
            }
            finally
            {
                _executionGuardService.FinishTask(taskKey);
            }
        }
        else
        {
            // Обработка на ситуацията, когато не е избрано изображение
            await DisplayAlert("Грешка", "Не е избрано изображение.", "OK");
        }

        //await ProcessSelectedImage(selectedImageName);
    }

    public async Task ProcessAndCleanupImageAsync(string fileName)
    {
        // Името на изображението, както е в Resources/Images (или като вградени ресурси)
        //string fileName = "example.png";

        // Пълният път, където ще запишем файла в AppDataDirectory
        string destinationPath = Path.Combine(FileSystem.AppDataDirectory, fileName);

        // Копиране на изображението от пакетните ресурси в AppDataDirectory
        // FileSystem.OpenAppPackageFileAsync търси файла в асемблито, където е включен като ресурс
        using (Stream sourceStream = await FileSystem.OpenAppPackageFileAsync(fileName))
        using (FileStream destinationStream = File.Create(destinationPath))
        {
            await sourceStream.CopyToAsync(destinationStream);
        }

        // Отваряне на файла от AppDataDirectory като поток за обработка
        using (FileStream processingStream = File.OpenRead(destinationPath))
        {
            // Тук поставете логиката за обработка на изображението
            // Например: зареждане в ImageSource, преобразуване или анализ
            await ProcessSelectedImage(processingStream);
        }

        // Изтриване на файла от AppDataDirectory, за да не се трупа
        if (File.Exists(destinationPath))
        {
            File.Delete(destinationPath);
        }
    }

    private async void OnImageCaptured(Stream imageStream)
    {
        //Capture the image
        if (ContourOverlay.IsVisible)
        {
            // Merge the overlay with the captured image
            imageStream = await MergeOverlayWithImage(imageStream);
        }


        await ProcessSelectedImage(imageStream);
        _cameraService.StopCamera();
        HideMenus();

        CameraButtonsPanel.IsEnabled = true;
    }

    private async Task<Stream> MergeOverlayWithImage(Stream imageStream)
    {
        //var assembly = typeof(MaskEditor).GetTypeInfo().Assembly;
        //var overlayStream = assembly.GetManifestResourceStream("FashionApp.Resources.Images.Camera.top_body.png"); //Correct path?
        var overlayStream = await FileSystem.OpenAppPackageFileAsync("Camera/top_body.png");

        if (overlayStream == null)
        {
            await DisplayAlert("Error", "Overlay image not found.", "OK");
            return imageStream;
        }

        using (var imageBitmap = SKBitmap.Decode(imageStream))
        using (var overlayBitmap = SKBitmap.Decode(overlayStream))
        using (var resizedOverlay = overlayBitmap.Resize(imageBitmap.Width, imageBitmap.Height, SKFilterQuality.High)) // Resize the overlay
        using (var mergedBitmap = new SKBitmap(imageBitmap.Width, imageBitmap.Height, true))
        using (var canvas = new SKCanvas(mergedBitmap))
        {
            canvas.DrawBitmap(imageBitmap, 0, 0);
            canvas.DrawBitmap(resizedOverlay, 0, 0, resizedOverlay.ToColorMatrix(SKColors.White));

            var mergedStream = new MemoryStream();
            mergedBitmap.Encode(mergedStream, SKEncodedImageFormat.Png, 100);
            mergedStream.Position = 0;
            return mergedStream;
        }
    }

    private async void OnSaveImageClicked(object sender, EventArgs e)
    {
        if (SelectedImage.Source == null) return;

        // Деактивиране на бутона
        ChangeVisibilityOnSaveButton(false);

        try
        {
            var resultStream = await AddMaskToImage.AddMaskToImageMetadata(SelectedImage, DrawingView);
#if WINDOWS
            string picturesPath = Path.Combine("C:", "Users", "Public", "Pictures");

            bool shouldGenerateRandomName = String.IsNullOrEmpty(imageFileName) || 
              (imageFileName == "closed_jacket_mask.png" && File.Exists(Path.Combine(picturesPath, "closed_jacket_mask.png"))) ||
              (imageFileName == "open_jacket_mask.png" && File.Exists(Path.Combine(picturesPath, "open_jacket_mask.png")));

            if (shouldGenerateRandomName)
            {
                imageFileName = $"masked_image_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            }

            byte[] imageBytes = await ConvertStreamToByteArrayAsync(resultStream);         

            await File.WriteAllBytesAsync($"C:\\Users\\Public\\Pictures\\{imageFileName}", imageBytes);
            await DisplayAlert("Success", $"Image saved to C:\\Users\\Public\\Pictures as {imageFileName}", "OK");

            imageFileName = string.Empty; // Reset value of paramether

#elif ANDROID
            SaveImageToAndroid.Save(imageFileName, resultStream, AppConstants.ImagesConstants.IMAGES_MASKS_DIRECTORY);                       
#endif
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                AppConstants.Errors.ERROR,
                $"{AppConstants.Errors.FAILED_TO_SAVE_IMAGE}: {ex.Message}",
                AppConstants.Messages.OK);
        }
        finally
        {
            // Включване на бутона отново
            ChangeVisibilityOnSaveButton(true);
        }
    }
    private async void ClosedJacketImageButton_Clicked(object sender, EventArgs e)
        => await MacroMechanics(sender, AppConstants.ImagesConstants.CLOSED_JACKET_MASK);
    private async void OpenJacketImageButton_Clicked(object sender, EventArgs e)
        => await MacroMechanics(sender, AppConstants.ImagesConstants.OPEN_JACKET_MASK);
    private void PanelButton_Clicked(object sender, EventArgs e)
    {
        _cameraService.StartCamera();
        HideMenus();
    }
    private void HidePanelCommand(object sender, EventArgs e)
    {     
        _cameraService.StopCamera();

        HideMenus();
    }
    private void OnCaptureClicked(object sender, EventArgs e)
    {
        CameraButtonsPanel.IsEnabled = false;
        _cameraService.CaptureClicked();
    }

    private void ContourSwitch_Clicked(object sender, EventArgs e)
    {
        ContourOverlay.IsVisible = !ContourOverlay.IsVisible;
    }

    // ------------------------------------------- SUPORT METHODS ----------------------------------------------------------------
    private async Task<bool> CheckAvailableMasksAndroidAsync(string fileName)
    {
        try
        {
            var fileChecker = App.Current?.Handler.MauiContext?.Services.GetService<IFileChecker>();
            var fileExists = await fileChecker.CheckFileExistsAsync(fileName);
            return fileExists;
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                AppConstants.Errors.ERROR,
                $"{AppConstants.Errors.ERROR_CKECK_MASKS}: {ex.Message}",
                AppConstants.Messages.OK);
            return false;
        }
    }
    private async Task MacroMechanics(object sender, string appConstants)
    {
        if (imageFileName != appConstants)
        {
            bool resultFromMessage = await MessageForMacroChangesAsync(appConstants);

            if (!resultFromMessage) return;

            SetActiveButton(sender as ImageButton);
            imageFileName = appConstants;
        }
        else
        {
            SetActiveButton(new ImageButton());
            imageFileName = $"masked_image_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        }
    }
    private async Task<bool> MessageForMacroChangesAsync(string macroImageFullName)
    {
        // Проверка за съществуваща вече маска към даденото Макро
        var isMaskExist = await CheckAvailableMasksAndroidAsync(macroImageFullName);
        if (isMaskExist)
        {
            // Питане за потвърждение дали да подмени маската или не с нова
            bool confirmation = await DisplayAlert(
                AppConstants.Messages.REPLACE_CONFIRMATION, 
                $"{AppConstants.Messages.MESSAGE_FOR_REPLACE} {macroImageFullName.Replace('_', ' ').Remove(macroImageFullName.Length - 4)}?",
                AppConstants.Messages.YES,
                AppConstants.Messages.CANCEL);
            return confirmation;
        }
        else
        {
            // Цъобщение че ще се запази като маска към незаето Макро
            await DisplayAlert(AppConstants.Messages.SET_CONFIRMATION,
                               AppConstants.Messages.MESSAGE_FOR_SAVE_MASK,
                               AppConstants.Messages.OK);
            return true;
        }         
    }
    private void SetActiveButton(ImageButton activeButton)
    {
        // Списък с всички бутони, които трябва да се управляват
        var allButtons = new List<ImageButton>
        {
            ClosedJacketImageButton,
            OpenJacketImageButton
        };

        // Премахваме ефектите от всички бутони
        foreach (var button in allButtons)
        {
            button.BackgroundColor = Colors.Transparent;
            button.BorderColor = Colors.Black;
            button.BorderWidth = 4;
            button.Scale = 1.0;
        }

        // Прилагаме ефектите само на активния бутон
        if (activeButton != null && allButtons.Contains(activeButton))
        {
            activeButton.BackgroundColor = Color.FromArgb("#E6F3FF");
            activeButton.BorderColor = Color.FromArgb("#0066CC");
            activeButton.BorderWidth = 6;
            activeButton.Scale = 1.3;
        }
    }
    public async Task<byte[]> ConvertStreamToByteArrayAsync(Stream stream)
    {
        if (stream == null) return Array.Empty<byte>();

        using (MemoryStream memoryStream = new MemoryStream())
        {
            await stream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }
    private void HideMenus()
    {
        Menu1.IsVisible = !Menu1.IsVisible;
        ImageContainer.IsVisible = !ImageContainer.IsVisible;
        Menu2.IsVisible = !Menu2.IsVisible;
        Menu3.IsVisible = !Menu3.IsVisible;
    }
    private void ChangeVisibilityOnSaveButton(bool isSet)
    {
        SaveMaskImageButton.IsEnabled = isSet;
        DrawingBottons.IsVisible = isSet;
    }
    private async Task ProcessSelectedImage(Stream? stream)
    {             
        bool pageAlreadyExists = Navigation.ModalStack.Any(p => p is ImageEditPage);
        if (pageAlreadyExists) return;

        string taskKey = AppConstants.Pages.IMAGE_EDIT;
        if (!_executionGuardService.TryStartTask(taskKey)) return;

        try
        {
            // Преоразмеряване на изображението
            var resizedImageResult = await ImageStreamResize.ResizeImageStream(stream, 500, 700); // Преоразмеряване на изображението

            // Конвертиране на изображението, за да включва алфа канал
            var imageWithAlpha = await AddAlphaChanel(resizedImageResult.ResizedStream);

            //  await Navigation.PushModalAsync(new ImageEditPage(imageWithAlpha));
            var page = MauiProgram.ServiceProvider.GetRequiredService<ImageEditPage>();
            page.ImageStream = imageWithAlpha;
            await Navigation.PushModalAsync(page);
        }
        finally
        {
            _executionGuardService.FinishTask(taskKey);
        }
    }

    //private async Task ProcessSelectedImage(string fileName)
    //{
    //    //Stream stream = new MemoryStream();
    //    //var imageWithAlpha = await AddAlphaChanel(stream);
    //    // Задаване на източника на изображението
    //    //SelectedImage.Source = ImageSource.FromStream(() => imageWithAlpha);
    //    SelectedImage.Source = fileName;

    //    // Настройка на ширината и височината на изображението
    //    SelectedImage.WidthRequest = Application.Current.MainPage.Width;
    //    SelectedImage.HeightRequest = Application.Current.MainPage.Height;

    //    // Центриране на изображението
    //    SelectedImage.HorizontalOptions = LayoutOptions.Center;
    //    SelectedImage.VerticalOptions = LayoutOptions.Center;

    //    // Показване на елементите
    //    SelectedImage.IsVisible = true;
    //    DrawingView.IsVisible = true;
    //    DrawingTools.IsVisible = true;
    //    DrawingBottons.IsVisible = true;
    //}
    private async Task<Stream> AddAlphaChanel(Stream imageStream)
    {
        using var originalBitmap = SKBitmap.Decode(imageStream);
        var bitmapWithAlpha = new SKBitmap(originalBitmap.Width, originalBitmap.Height, true);

        using (var canvas = new SKCanvas(bitmapWithAlpha))
        {
            canvas.Clear(SKColors.Transparent);
            canvas.DrawBitmap(originalBitmap, 0, 0);
        }

        var imageWithAlphaStream = new MemoryStream();
        bitmapWithAlpha.Encode(imageWithAlphaStream, SKEncodedImageFormat.Png, 80);

        imageWithAlphaStream.Position = 0;
        return imageWithAlphaStream;
    }
    private void OnImageSizeChanged(object sender, EventArgs e)
    {
        if (SelectedImage.Width > 0 && SelectedImage.Height > 0)
        {
            DrawingView.WidthRequest = SelectedImage.Width;
            DrawingView.HeightRequest = SelectedImage.Height;

            DrawingView.TranslationX = SelectedImage.X;
            DrawingView.TranslationY = SelectedImage.Y;
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

    //--------------------------------------------- DRAW METHODS ------------------------------------------------------------
    void OnStartInteraction(object sender, TouchEventArgs e)
    {
        var touch = e.Touches[0];

        if (touch.X >= 0 && touch.X <= DrawingView.Width &&
            touch.Y >= 0 && touch.Y <= DrawingView.Height)
        {
            _currentLine = new DrawingLine
            {
                Color = Colors.Gray,
                Thickness = (float)BrushSlider.Value
            };
            _currentLine.Points.Add(touch);
            _lines.Add(_currentLine);
            DrawingView.Invalidate();
        }
    }
    void OnDragInteraction(object sender, TouchEventArgs e)
    {
        if (_currentLine != null)
        {
            var touch = e.Touches[0];

            // Проверяваме дали докосването е в рамките на DrawingView
            if (touch.X >= 0 && touch.X <= DrawingView.Width &&
                touch.Y >= 0 && touch.Y <= DrawingView.Height)
            {
                _currentLine.Points.Add(touch);
                DrawingView.Invalidate();
            }
        }
    }
    void OnEndInteraction(object sender, TouchEventArgs e) => _currentLine = null;
    void OnClearClicked(object sender, EventArgs e)
    {
        _lines.Clear();
        DrawingView.Invalidate();
    }
    void OnUndoClicked(object sender, EventArgs e)
    {
        if (_lines.Count > 0)
        {
            _lines.RemoveAt(_lines.Count - 1);
            DrawingView.Invalidate();
        }
    }
}

public static class SKBitmapExtensions
{
    public static SKPaint ToColorMatrix(this SKBitmap bitmap, SKColor color)
    {
        var paint = new SKPaint();
        paint.Color = color;
        paint.Shader = SKShader.CreateColor(color);
        return paint;
    }

    public static SKBitmap Resize(this SKBitmap bitmap, int maxWidth, int maxHeight, SKFilterQuality filterQuality = SKFilterQuality.High)
    {
        float widthScale = (float)maxWidth / bitmap.Width;
        float heightScale = (float)maxHeight / bitmap.Height;
        float scale = Math.Min(widthScale, heightScale);

        int newWidth = (int)(bitmap.Width * scale);
        int newHeight = (int)(bitmap.Height * scale);

        using (var scaledBitmap = new SKBitmap(newWidth, newHeight))
        using (var canvas = new SKCanvas(scaledBitmap))
        {
            canvas.Scale(scale); // Scale the canvas
            canvas.DrawBitmap(bitmap, 0, 0);  // Draw the original bitmap onto the scaled canvas
            return scaledBitmap.Copy(); //return a copy to prevent disposing
        }
    }
}


