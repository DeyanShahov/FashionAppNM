#if __ANDROID__
using Android.Content;
using Android.Database;
using Android.Net;
using Android.Provider;
#endif

using CommunityToolkit.Maui.Views;
using FashionApp.core;
using FashionApp.core.draw;

namespace FashionApp.Pages;

public partial class ImageEditPage : ContentPage
{
    private List<DrawingLine> _lines = new();
    private DrawingLine _currentLine;
    private IDrawable _drawable;


    public ImageEditPage(string imageUri)
	{
		InitializeComponent();
		LoadImage(imageUri);

        _drawable = new DrawingViewDrawable(_lines);
        DrawingView.Drawable = _drawable;
    }

	private async void LoadImage(string imagePath)
	{
        ImageForEdit.Source = ImageSource.FromFile(imagePath);
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

    private void OnSaveImageClicked(object sender, EventArgs e)
    {
        
    }

}