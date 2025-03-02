#if __ANDROID__
using Android.Content;
using Android.Database;
using Android.Net;
using Android.Provider;
#endif

using FashionApp.core.draw;

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
    private double _xOffset = 0;
    private double _yOffset = 0;
    private double _startX;
    private double _startY;

    public ImageEditPage(string imageUri)
	{
		InitializeComponent();
        // Използваме SizeChanged, за да сме сигурни, че страницата и елементите са с правилния размер
        this.SizeChanged += OnSizeChanged;

        LoadImage(imageUri);
     
        _drawable = new DrawingViewDrawable(_lines);
        DrawingView.Drawable = _drawable;
    }

	private void LoadImage(string imagePath)
	{
        ImageForEdit.Source = ImageSource.FromFile(imagePath);
        ZoomSlider_ValueChanged(ZoomSlider, new ValueChangedEventArgs(ZoomSlider.Value, ZoomSlider.Value));
    }

	private async void CloseButton_Clicked(object sender, EventArgs e)
	{
		await Navigation.PopModalAsync();
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
                Thickness = (float)20 //(float)BrushSlider.Value
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
        
    }

    //private void OnZoomClicked(object sender, EventArgs e)
    //{
    //    ZoomSliderPanel.IsVisible = !ZoomSliderPanel.IsVisible;
    //    ZoomSliderPanel.IsEnabled = !ZoomSliderPanel.IsEnabled;
    //}

    private void OnZoomClicked(object sender, EventArgs e)
    {
        // Toggle visibility and enabled state
        ZoomSliderPanel.IsVisible = !ZoomSliderPanel.IsVisible;
        ZoomSliderPanel.IsEnabled = !ZoomSliderPanel.IsEnabled;

        if (ZoomSliderPanel.IsVisible)
        {
            ZoomButton.BackgroundColor = Color.FromRgba(255, 255, 255, 1);
        }
        else
        {
            ZoomButton.BackgroundColor = Color.FromRgba(0, 0, 0, 0);
        }
        // Reset the zoom value
        //ZoomSlider.Value = 1;
        //_currentScale = 1;
        //ImageForEdit.Scale = _currentScale;
        //ImageForEdit.TranslationX = 0;
        //ImageForEdit.TranslationY = 0;
        //_xOffset = 0;
        //_yOffset = 0;
    }

    private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
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

            _xOffset = newX;
            _yOffset = newY;
        }
    }

    // Обработчик за промените в слайдъра
    //private async void ZoomSlider_ValueChanged(object sender, ValueChangedEventArgs e)
    //{
    //    // Новата желана скала се взема от стойността на слайдъра
    //    double newScale = e.NewValue;
    //    // Анимираме плавно промяната на мащаба
    //    await ImageForEdit.ScaleTo(newScale, 250, Easing.CubicInOut);
    //}

    private void ZoomSlider_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        _currentScale = e.NewValue;

        // Scale the image
        ImageForEdit.Scale = _currentScale;
        //Center the image
        //ImageForEdit.TranslationX = 0;
        //ImageForEdit.TranslationY = 0;
        //_xOffset = 0;
        //_yOffset = 0;
        // Reset the slider to the start
        ZoomSlider.Value = _currentScale;
        // Reset the zoomButton to the original color
        ZoomButton.BackgroundColor = Color.FromRgba(0, 0, 0, 0);

    }


    private async void OnSizeChanged(object sender, EventArgs e)
    {
        if (!alreadyZoomed)
        {
            alreadyZoomed = true;
            // Анимирайте промяната на скалата към центъра на изображението.
            await ImageForEdit.ScaleTo(ZoomSlider.Value, 500, Easing.CubicInOut);
        }
    }
}