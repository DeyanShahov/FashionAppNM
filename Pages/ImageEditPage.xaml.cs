#if __ANDROID__
using Android.Content;
using Android.Database;
using Android.Net;
using Android.Provider;
#endif

using CommunityToolkit.Maui.Views;
using FashionApp.core;
using FashionApp.core.draw;
using FashionApp.core.services;
using FashionApp.Data.Constants;
using System.Diagnostics;

namespace FashionApp.Pages;

public partial class ImageEditPage : ContentPage
{
    private List<DrawingLine> _lines = new();
    private DrawingLine _currentLine;
    private IDrawable _drawable;

    //double xOffset = 0;
    //double yOffset = 0;
    bool alreadyZoomed = false;

    private double _currentScale = 1;
    private double _startScale = 1;
    private double _currentScaleBrush = 20;
    private double _startScaleBrush = 20;
    private double _xOffset = 0;
    private double _yOffset = 0;
    private double _startX;
    private double _startY;

    private string imageFileName = $"masked_image_{DateTime.Now:yyyyMMdd_HHmmss}.png";

    public string ImageUri { get; set; } = String.Empty;
    public Stream ImageStream { get; set; }

    //private Image _basicImage;
    //private GraphicsView _graphicsView;

    public ImageEditPage()
    {
        InitializeComponent();
        //_basicImage = image;
        //_graphicsView = graphicsView;

        this.SizeChanged += OnSizeChanged;

        _drawable = new DrawingViewDrawable(_lines);
        DrawingView.Drawable = _drawable;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _lines.Clear();
        DrawingView.Invalidate();

        if (ImageUri != String.Empty) LoadImageFromUrl(ImageUri);
        else LoadImageFromStream(ImageStream);
    }


    // Конструктор, който приема imageUri (string)
    //public ImageEditPage(string imageUri)
    //    : this()
    //{
    //    LoadImageFromUrl(imageUri);
    //}

    // Конструктор, който приема Stream
    //public ImageEditPage(Stream imageStream)
    //    : this()
    //{
    //    LoadImageFromStream(imageStream);
    //}

    private void LoadImageFromStream(Stream imageStream)
    {
        ImageForEdit.Source = ImageSource.FromStream(() => imageStream);
        ZoomSlider_ValueChanged(ZoomSlider, new ValueChangedEventArgs(ZoomSlider.Value, ZoomSlider.Value));
    }

    private void LoadImageFromUrl(string imagePath)
	{
        //ImageForEdit.Source = ImageSource.FromFile(imagePath);
        ImageForEdit.Source = imagePath;
        ZoomSlider_ValueChanged(ZoomSlider, new ValueChangedEventArgs(ZoomSlider.Value, ZoomSlider.Value));
    }

	private async void CloseButton_Clicked(object sender, EventArgs e)
	{
		await Navigation.PopModalAsync();
	}
    void OnStartInteraction(object sender, TouchEventArgs e)
    {
        if (ZoomSliderPanel.IsVisible == true) return;

        var touch = e.Touches[0];

        if (touch.X >= 0 && touch.X <= DrawingView.Width &&
            touch.Y >= 0 && touch.Y <= DrawingView.Height)
        {
            _currentLine = new DrawingLine
            {
                Color = Colors.Gray,
                //Thickness = (float)20 
                Thickness = (float)_currentScaleBrush
            };
            _currentLine.Points.Add(touch);
            _lines.Add(_currentLine);
            DrawingView.Invalidate();
        }
    }
    void OnDragInteraction(object sender, TouchEventArgs e)
    {
        //if (ZoomSliderPanel.IsVisible == true) return;

        if (_currentLine != null)
        {
            var touch = e.Touches[0];

            // ����������� ���� ����������� � � ������� �� DrawingView
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
    private void OnSaveImageClicked(object sender, EventArgs e)
    {
        SavePanelOptions.IsVisible = !SavePanelOptions.IsVisible;
    }


    private void OnZoomClicked(object sender, EventArgs e)
    {
        // Toggle visibility and enabled state
        ZoomSliderPanel.IsVisible = !ZoomSliderPanel.IsVisible;
        ZoomSliderPanel.IsEnabled = !ZoomSliderPanel.IsEnabled;
        BrushSliderPanel.IsVisible = !BrushSliderPanel.IsVisible;
        BrushSliderPanel.IsEnabled = !BrushSliderPanel.IsEnabled;
        DrawingView.IsVisible = !DrawingView.IsVisible;

        if (ZoomSliderPanel.IsVisible)
        {
            ZoomButton.BackgroundColor = Color.FromRgba(255, 255, 255, 1);
        }
        else
        {
            ZoomButton.BackgroundColor = Color.FromRgba(0, 0, 0, 0);
        }
    }

    private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        if (!ZoomSliderPanel.IsVisible)
        {
            DrawingView.IsVisible = !DrawingView.IsVisible;
            DrawingView.IsEnabled = !DrawingView.IsEnabled;
            return;
        }

        if (e.StatusType == GestureStatus.Started)
        {
            _startX = _xOffset;
            _startY = _yOffset;
        }
        else if (e.StatusType == GestureStatus.Running)
        {
            double newX = _startX + e.TotalX;
            double newY = _startY + e.TotalY;

            // Calculate allowed boundaries
            double maxOffsetX = (ImageForEdit.Width * _currentScale - ImageContainer.Width) / 2;
            double maxOffsetY = (ImageForEdit.Height * _currentScale - ImageContainer.Height) / 2;

            // Limit panning to keep at least half of the image visible
            if (ImageForEdit.Width * _currentScale > ImageContainer.Width)
            {
                newX = Math.Max(newX, -maxOffsetX); // Limit to the left
                newX = Math.Min(newX, maxOffsetX); // Limit to the right
            }
            else
            {
                newX = 0;
            }
            if (ImageForEdit.Height * _currentScale > ImageContainer.Height)
            {
                newY = Math.Max(newY, -maxOffsetY);
                newY = Math.Min(newY, maxOffsetY);
            }
            else
            {
                newY = 0;
            }

            // Apply translation
            ImageForEdit.TranslationX = newX;
            ImageForEdit.TranslationY = newY;

            DrawingView.TranslationX = newX;
            DrawingView.TranslationY = newY;

            _xOffset = newX;
            _yOffset = newY;
        }
    }

    private void BrushSlider_ValueChanged(object sender, ValueChangedEventArgs e) => _currentScaleBrush = e.NewValue;

    private void ZoomSlider_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        try
        {
            Slider slider = (Slider)sender;
            double zoomValue = slider.Value;
            _currentScale = zoomValue;

            if (ImageForEdit != null) ImageForEdit.Scale = _currentScale;
            if (DrawingView != null) DrawingView.Scale = _currentScale;

            ZoomButton.BackgroundColor = Color.FromRgba(0, 0, 0, 0);
        }
        catch (NullReferenceException ex)
        {
            Debug.WriteLine($"NullReferenceException: {ex.Message}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception: {ex.Message}");
        }
    }


    private async void OnSizeChanged(object sender, EventArgs e)
    {
        if (!alreadyZoomed)
        {
            alreadyZoomed = true;
            await ImageForEdit.ScaleTo(ZoomSlider.Value, 500, Easing.CubicInOut);
        }
    }

    private async void OnJacketSelected(object sender, TappedEventArgs e)
    {
        if (e.Parameter is string selectedCloth)
        {
            selectedCloth = selectedCloth.Remove(selectedCloth.Length - 4).Remove(0, 13).Replace('_', ' ');
            var result = false;

            bool checkForSave = await DisplayAlert("Message",
                              $"{selectedCloth} {"ли искахте да маркирате?"}",
                              AppConstants.Messages.YES,
                              AppConstants.Messages.CANCEL);

            if (!checkForSave) return;

            switch (selectedCloth)
            {
                case "dress":
                    result = await MacroMechanics(sender, "dress_mask.png");
                    break;
                case "dress full":
                    result = await MacroMechanics(sender, "dress_full_mask.png");
                    break;
                case "jacket closed":
                    result = await MacroMechanics(sender, AppConstants.ImagesConstants.CLOSED_JACKET_MASK);
                    break;
                case "jacket open":
                    result = await MacroMechanics(sender, AppConstants.ImagesConstants.OPEN_JACKET_MASK);
                    break;
                case "jacket":
                    result = await MacroMechanics(sender, "jacket_mask.png");             
                    break;
                case "pants":
                    result = await MacroMechanics(sender, "pants_mask.png");
                    break;
                case "pants short":
                    result = await MacroMechanics(sender, "pants_short_mask.png");
                    break;
                case "raincoat":
                    result = await MacroMechanics(sender, "raincoat_mask.png");
                    break;
                case "shirt":
                    result = await MacroMechanics(sender, "shirt_mask.png");
                    break;
                case "skirt":
                    result = await MacroMechanics(sender, "skirt_mask.png");
                    break;
                case "skirt long":
                    result = await MacroMechanics(sender, "skirt_long_mask.png");
                    break;
                case "tank top":
                    result = await MacroMechanics(sender, "tank_top_mask.png");
                    break;
                case "no set":
                    result = true;
                    break;
            }

            string ops = imageFileName;

           

            if (result && checkForSave)
            {
                SaveImage();
                await Navigation.PopModalAsync();
            }
            //else
            //{
            //    await DisplayAlert("Избран елемент", $"Вие избрахте: {selectedCloth}", "OK");
            //}
        }
    }

    private async Task<bool> MacroMechanics(object sender, string appConstants)
    {
        if (imageFileName != appConstants)
        {
            bool resultFromMessage = await MessageForMacroChangesAsync(appConstants);

            if (!resultFromMessage) return false;

            //SetActiveButton(sender as ImageButton);
            imageFileName = appConstants;
            return true;
        }
        else
        {
            await DisplayAlert("Избрано яке", $"Вече е сетнато като: {imageFileName}", "OK");
            return false;
        }
        //else
        //{
        //    //SetActiveButton(new ImageButton());
        //    imageFileName = $"masked_image_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        //}
    }

    private async Task<bool> MessageForMacroChangesAsync(string macroImageFullName)
    {
        // Проверка за съществуваща вече маска към даденото Макро
        var isMaskExist = await CheckAvailableMasksAndroidAsync(macroImageFullName);
        //if (isMaskExist)
        //{
        //    // Питане за потвърждение дали да подмени маската или не с нова
        //    bool confirmation = await DisplayAlert(
        //        AppConstants.Messages.REPLACE_CONFIRMATION,
        //        $"{AppConstants.Messages.MESSAGE_FOR_REPLACE} {macroImageFullName.Replace('_', ' ').Remove(macroImageFullName.Length - 4)}?",
        //        AppConstants.Messages.YES,
        //        AppConstants.Messages.CANCEL);
        //    return confirmation;
        //}
        //else
        //{
        //    // Цъобщение че ще се запази като маска към незаето Макро
        //    return await DisplayAlert(isMaskExist ? AppConstants.Messages.REPLACE_CONFIRMATION : AppConstants.Messages.SET_CONFIRMATION,
        //                       isMaskExist ? 
        //                            $"{AppConstants.Messages.MESSAGE_FOR_REPLACE} {macroImageFullName.Replace('_', ' ').Remove(macroImageFullName.Length - 4)}?" 
        //                            : AppConstants.Messages.MESSAGE_FOR_SAVE_MASK,
        //                       AppConstants.Messages.YES,
        //                       AppConstants.Messages.CANCEL);
        //}

        return await DisplayAlert(isMaskExist ? AppConstants.Messages.REPLACE_CONFIRMATION : AppConstants.Messages.SET_CONFIRMATION,
                               isMaskExist ?
                                    $"{AppConstants.Messages.MESSAGE_FOR_REPLACE} {macroImageFullName.Replace('_', ' ').Remove(macroImageFullName.Length - 4)}?"
                                    : AppConstants.Messages.MESSAGE_FOR_SAVE_MASK,
                               AppConstants.Messages.YES,
                               AppConstants.Messages.CANCEL);
    }

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

    private async void SaveImage()
    {
        if (ImageForEdit.Source == null) return;

        // Деактивиране на бутона
        //ChangeVisibilityOnSaveButton(false);

        try
        {
            var resultStream = await AddMaskToImage.AddMaskToImageMetadata(ImageForEdit, DrawingView);
#if ANDROID
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
            //ChangeVisibilityOnSaveButton(true);
        }
    }
}