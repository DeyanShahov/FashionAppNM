using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using IImage = Microsoft.Maui.Graphics.IImage;
using Microsoft.Maui.Controls;
using CommunityToolkit.Maui.Views;
using FashionApp.core;
using SkiaSharp;
using System.IO;
using static Microsoft.Maui.ApplicationModel.Permissions;
using FashionApp.core.services;
using Microsoft.Maui.Controls.PlatformConfiguration;
using FashionApp.Data.Constants;
//using static Java.Util.Jar.Attributes;



#if __ANDROID__
using AndroidX.Emoji2.Text.FlatBuffer;
using Android.Content;
using Android.Database;
using Android.Net;
using Android.Provider;
using Android.OS;
#endif

namespace FashionApp;

public partial class MaskEditor : ContentPage
{
    private List<DrawingLine> _lines = new();
    private DrawingLine _currentLine;
    private IDrawable _drawable;
    //private HashSet<(int x, int y)> _markedPixels = new();
    private string imageFileName = $"masked_image_{DateTime.Now:yyyyMMdd_HHmmss}.png";
    private CameraService _cameraService;

    public MaskEditor()
    {
        InitializeComponent();
        //_drawable = new DrawingViewDrawable(_lines, _markedPixels);
        _drawable = new DrawingViewDrawable(_lines);
        DrawingView.Drawable = _drawable;

        _cameraService = new CameraService(MyCameraView, CameraPanel);
        _cameraService.ImageCaptured += OnImageCaptured;
        _cameraService.StopCamera();
        //MyCameraView.StopCameraPreview();
    }

    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnSelectImageClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Images,
                PickerTitle = "Pick an image"
            });

            if (result != null)
            {
                if (result.FileName.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
                    result.FileName.EndsWith("png", StringComparison.OrdinalIgnoreCase))
                {
                    var stream = await result.OpenReadAsync();
                    await ProcessSelectedImage(stream);
                }
                else
                {
                    await DisplayAlert("Error", "Please select a valid image file (jpg or png)", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
    }

    private async void OnImageCaptured(Stream imageStream)
    {
        await ProcessSelectedImage(imageStream);
        _cameraService.StopCamera();
        HideMenus();
    }

    private async Task ProcessSelectedImage(Stream? stream)
    {
        // Преоразмеряване на изображението
        var resizedImageResult = await ImageStreamResize.ResizeImageStream(stream, 500, 700); // Преоразмеряване на изображението

        // Конвертиране на изображението, за да включва алфа канал
        var imageWithAlpha = await AddAlphaChanel(resizedImageResult.ResizedStream);

        // Задаване на източника на изображението
        SelectedImage.Source = ImageSource.FromStream(() => imageWithAlpha);
        
        // Настройка на ширината и височината на изображението
        SelectedImage.WidthRequest = Application.Current.MainPage.Width; // Задаване на ширината на екрана
        SelectedImage.HeightRequest = Application.Current.MainPage.Height; // Задаване на височината на екрана

        // Центриране на изображението
        SelectedImage.HorizontalOptions = LayoutOptions.Center;
        SelectedImage.VerticalOptions = LayoutOptions.Center;

        // Показване на елементите
        SelectedImage.IsVisible = true;
        DrawingView.IsVisible = true;
        DrawingTools.IsVisible = true;
        DrawingBottons.IsVisible = true;

        //CameraPanel.IsVisible = false;
        //CameraPanel.IsEnabled = false;
    }

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

    void OnEndInteraction(object sender, TouchEventArgs e)
    {
        _currentLine = null;
    }

    void OnClearClicked(object sender, EventArgs e)
    {
        _lines.Clear();
        //_markedPixels.Clear(); // Изчистваме и маркираните пиксели
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

    private async void OnSaveImageClicked(object sender, EventArgs e)
    {
        if (SelectedImage.Source == null) return;

        // Деактивиране на бутона
        SaveMaskImageButton.IsEnabled = false;
        DrawingBottons.IsVisible = false;

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
            SaveImageToAndroid.Save(imageFileName, resultStream, AppConstants.IMAGES_MASKS_DIRECTORY);                       
#endif
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to save image: {ex.Message}", "OK");
        }
        finally
        {
            // Включване на бутона отново
            SaveMaskImageButton.IsEnabled = true;
            DrawingBottons.IsVisible = true;
        }
    }

    private async void ClosedJacketImageButton_Clicked(object sender, EventArgs e)
    {
        await MacroMechanics(sender, AppConstants.CLOSED_JACKET_MASK);
    }


    private async void OpenJacketImageButton_Clicked(object sender, EventArgs e)
    {
        await MacroMechanics(sender, AppConstants.OPEN_JACKET_MASK);
    }

    private void PanelButton_Clicked(object sender, EventArgs e)
    {
        HideMenus();

        _cameraService.StartCamera();
    }

    private void HideMenus()
    {
        Menu1.IsVisible = !Menu1.IsVisible;
        ImageContainer.IsVisible = !ImageContainer.IsVisible;
        Menu2.IsVisible = !Menu2.IsVisible;
        Menu3.IsVisible = !Menu3.IsVisible;
    }

    private void HidePanelCommand(object sender, EventArgs e)
    {
       
        _cameraService.StopCamera();
        HideMenus();
    }

    //private async void MyCameraView_MediaCaptured(object sender, CommunityToolkit.Maui.Views.MediaCapturedEventArgs e)
    //{
       
    //}

    private async void OnCaptureClicked(object sender, EventArgs e)
    {
        //await MyCameraView.CaptureImage(CancellationToken.None);
        _cameraService.CaptureClicked();
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
            await DisplayAlert("Error checking masks", ex.Message, "Ok");
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
            bool confirmation = await DisplayAlert("Replace Confirmation", $"Are you sure you want to replace the {macroImageFullName.Replace('_', ' ').Remove(macroImageFullName.Length - 4)}?", "Yes", "Cancel");
            return confirmation;
        }
        else
        {
            // Цъобщение че ще се запази като маска към незаето Макро
            await DisplayAlert("Set Confirmation", "You will save a new mask image. ", "OK");
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
            // Тук можете да добавите нови бутони в бъдеще
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
        if (stream == null)
            return null;

        using (MemoryStream memoryStream = new MemoryStream())
        {
            await stream.CopyToAsync(memoryStream); // Асинхронно копиране
            return memoryStream.ToArray();
        }
    }
}


// ----------------------------------------------- CLASS ---------------------------------------------------------------------
public class DrawingViewDrawable : IDrawable
{
    private readonly List<DrawingLine> _lines;
    //private readonly HashSet<(int x, int y)> _markedPixels;

    public DrawingViewDrawable(List<DrawingLine> lines)
    {
        _lines = lines;
        //_markedPixels = markedPixels;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        foreach (var line in _lines)
        {
            if (line.Points.Count > 1)
            {
                canvas.StrokeColor = line.Color;
                canvas.StrokeSize = line.Thickness;
                canvas.StrokeLineCap = LineCap.Round;
                canvas.StrokeLineJoin = LineJoin.Round;

                var path = new PathF();
                path.MoveTo(line.Points[0]);

                for (int i = 1; i < line.Points.Count; i++)
                {
                    path.LineTo(line.Points[i]);
                }

                canvas.DrawPath(path);
            }
        }
    }
}


