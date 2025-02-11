using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using IImage = Microsoft.Maui.Graphics.IImage;
using Microsoft.Maui.Controls;
using CommunityToolkit.Maui.Views;
using FashionApp.core;
using SkiaSharp;


namespace FashionApp;

public partial class EmptyPage : ContentPage
{
    private List<DrawingLine> _lines = new();
    private DrawingLine _currentLine;
    private IDrawable _drawable;
    private HashSet<(int x, int y)> _markedPixels = new();

    public EmptyPage()
    {
        InitializeComponent();
        _drawable = new DrawingViewDrawable(_lines, _markedPixels);
        DrawingView.Drawable = _drawable;
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
                    var resizedImageResult = await ImageStreamResize.ResizeImageStream(stream, 500, 700); // Преоразмеряване на изображението

                    // Convert the image to include an alpha channel
                    var imageWithAlpha = await AddAlphaChanel(resizedImageResult.ResizedStream);

                    //SelectedImage.Source = ImageSource.FromStream(() => resizedImageResult.ResizedStream);
                    SelectedImage.Source = ImageSource.FromStream(() => imageWithAlpha);
                    SelectedImage.WidthRequest = resizedImageResult.Width;
                    SelectedImage.HeightRequest = resizedImageResult.Height;

                    //DrawingView.HeightRequest = SelectedImage.Height;
                    //DrawingView.WidthRequest = SelectedImage.Width;

                    //DrawingView.TranslationX = SelectedImage.X;
                    //DrawingView.TranslationY = SelectedImage.Y;

                    SelectedImage.IsVisible = true;
                    DrawingView.IsVisible = true;
                    DrawingTools.IsVisible = true;
                    DrawingBottons.IsVisible = true;
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

    private async Task<Stream> AddAlphaChanel(Stream imageStream)
    {
        //// Load the image
        //using var image = SKBitmap.Decode(imageStream);
        //// Create a new image with an alpha channel
        //using var imageWithAlpha = new SKBitmap(image.Width, image.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
        //// Copy the original image to the new image
        //using var canvas = new SKCanvas(imageWithAlpha);
        //canvas.DrawBitmap(image, 0, 0);
        //// Save the new image to a stream
        //var stream = new MemoryStream();
        //imageWithAlpha.Encode(SKEncodedImageFormat.Png, 100).SaveTo(stream);
        //stream.Position = 0;
        //return stream;

        using var originalBitmap = SKBitmap.Decode(imageStream);
        var bitmapWithAlpha = new SKBitmap(originalBitmap.Width, originalBitmap.Height, true);

        using (var canvas = new SKCanvas(bitmapWithAlpha))
        {
            canvas.Clear(SKColors.Transparent);
            canvas.DrawBitmap(originalBitmap, 0, 0);
        }

        var imageWithAlphaStream = new MemoryStream();
        bitmapWithAlpha.Encode(imageWithAlphaStream, SKEncodedImageFormat.Png, 100);

        imageWithAlphaStream.Position = 0;
        return imageWithAlphaStream;
    }

    private void OnImageSizeChanged(object sender, EventArgs e)
    {
        //if (SelectedImage.Width > 0 && SelectedImage.Height > 0)
        //{
           
        //}

        DrawingView.WidthRequest = SelectedImage.Width;
        DrawingView.HeightRequest = SelectedImage.Height;

        DrawingView.TranslationX = SelectedImage.X;
        DrawingView.TranslationY = SelectedImage.Y;
    }

    void OnStartInteraction(object sender, TouchEventArgs e)
    {
        var touch = e.Touches[0];

        if (touch.X >= 0 && touch.X <= DrawingView.Width &&
            touch.Y >= 0 && touch.Y <= DrawingView.Height)
        {
            _currentLine = new DrawingLine
            {
                //Color = Colors.Gray.WithAlpha(0.8f),
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
        _markedPixels.Clear(); // Изчистваме и маркираните пиксели
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

        try
        {
            //var imageOriginal = await SelectedImage.CaptureAsync();
            //var resultStream = await AddMaskToImage.AddMaskToImageMetadata(SelectedImage.Source, DrawingView);
            var resultStream = await AddMaskToImage.AddMaskToImageMetadata(SelectedImage, DrawingView);

            var fileName = $"masked_image_{DateTime.Now:yyyyMMdd_HHmmss}.png";

#if WINDOWS
            byte[] imageBytes = await ConvertStreamToByteArrayAsync(resultStream);
            
            //using ( var imageStream = await resultStream.OpenReadAsync())
            //{
            //    using (var memoryStream = new MemoryStream())
            //    {
            //        memoryStream.Position = 0;
            //        await imageStream.CopyToAsync(memoryStream);
            //        imageBytes = memoryStream.ToArray();
            //    }
            //}

            await File.WriteAllBytesAsync($"C:\\Users\\Public\\Pictures\\{fileName}", imageBytes);
            await DisplayAlert("Success", $"Image saved to C:\\Users\\Public\\Pictures", "OK");

#elif ANDROID
            //var imageStream2 = await resultStream.OpenReadAsync();
            //imageStream2.Position = 0;
            var context = Platform.CurrentActivity;

            if (OperatingSystem.IsAndroidVersionAtLeast(29))
            {
                Android.Content.ContentResolver resolver = context.ContentResolver;
                Android.Content.ContentValues contentValues = new();
                contentValues.Put(Android.Provider.MediaStore.IMediaColumns.DisplayName, fileName);
                contentValues.Put(Android.Provider.MediaStore.IMediaColumns.MimeType, "image/png");
                contentValues.Put(Android.Provider.MediaStore.IMediaColumns.RelativePath, "DCIM/" + "FashionApp");
                Android.Net.Uri imageUri = resolver.Insert(Android.Provider.MediaStore.Images.Media.ExternalContentUri, contentValues);
                var os = resolver.OpenOutputStream(imageUri);
                Android.Graphics.BitmapFactory.Options options = new();
                options.InJustDecodeBounds = true;
                var bitmap = Android.Graphics.BitmapFactory.DecodeStream(resultStream);
                bitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, os);
                os.Flush();
                os.Close();   

                await DisplayAlert("Success", "Image saved on DCIM / FashionApp!", "OK");
            }
            else
            {
            }
#endif
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to save image: {ex.Message}", "OK");
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


public class DrawingViewDrawable : IDrawable
{
    private readonly List<DrawingLine> _lines;
    private readonly HashSet<(int x, int y)> _markedPixels;

    public DrawingViewDrawable(List<DrawingLine> lines, HashSet<(int x, int y)> markedPixels)
    {
        _lines = lines;
        _markedPixels = markedPixels;
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

    //public void DrawToCanvas(SKCanvas canvas, SKRect rect)
    //{
    //    foreach (var line in _lines)
    //    {
    //        if (line.Points.Count > 1)
    //        {
    //            using var paint = new SKPaint
    //            {


    //            // Replace the problematic line with the following
    //                Color = line.Color.ToSKColor(),
    //                StrokeWidth = line.Thickness,
    //                StrokeCap = SKStrokeCap.Round,
    //                StrokeJoin = SKStrokeJoin.Round,
    //                Style = SKPaintStyle.Stroke
    //            };

    //            var path = new SKPath();
    //            path.MoveTo(line.Points[0].X, line.Points[0].Y);

    //            for (int i = 1; i < line.Points.Count; i++)
    //            {
    //                path.LineTo(line.Points[i].X, line.Points[i].Y);
    //            }
    //            canvas.DrawPath(path, paint);
    //        }
    //    }
    //}
}

