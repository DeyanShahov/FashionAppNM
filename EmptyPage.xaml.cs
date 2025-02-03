using Microsoft.Maui.Graphics;
using Microsoft.Maui.Graphics.Platform;
using IImage = Microsoft.Maui.Graphics.IImage;
using Microsoft.Maui.Controls;

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
                    SelectedImage.Source = ImageSource.FromStream(() => stream);
                    SelectedImage.IsVisible = true;
                    DrawingView.IsVisible = true;
                    DrawingTools.IsVisible = true;
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

    private async Task<byte[]> ConvertImageToByteArray(IImage image)
    {
        using var stream = new MemoryStream();
        await image.SaveAsync(stream, ImageFormat.Png);
        return stream.ToArray();
    }

    private async void OnSaveImageClicked(object sender, EventArgs e)
    {
        if (SelectedImage.Source == null) return;

        try
        {
            var image = await DrawingView.CaptureAsync();


            var fileName = $"masked_image_{DateTime.Now:yyyyMMdd_HHmmss}.png";

            var imageStream = await image.OpenReadAsync();
            imageStream.Position = 0;
            //var bitmap = Android.Graphics.BitmapFactory.DecodeStream(imageStream);


#if WINDOWS
            var result = await ConvertImageToByteArray(image as IImage);
            await File.WriteAllBytesAsync($"C:\\Users\\Public\\Pictures\\{fileName}", result);
            await DisplayAlert("Success", $"Image saved to C:\\Users\\Public\\Pictures", "OK");

#elif ANDROID
            var context = Platform.CurrentActivity;

            if (OperatingSystem.IsAndroidVersionAtLeast(29))
            {
                //var contentValues = new Android.Content.ContentValues();
                //contentValues.Put(Android.Provider.MediaStore.IMediaColumns.DisplayName, fileName);
                //contentValues.Put(Android.Provider.MediaStore.IMediaColumns.MimeType, "image/png");
                //contentValues.Put(Android.Provider.MediaStore.IMediaColumns.RelativePath, 
                //                Android.OS.Environment.DirectoryPictures);

                //var resolver = context.ContentResolver;
                //var uri = resolver.Insert(Android.Provider.MediaStore.Images.Media.ExternalContentUri, 
                //                        contentValues);

                //using var outputStream = resolver.OpenOutputStream(uri);
                //using var ms = new MemoryStream();
                //await image.CopyToAsync(ms);
                //await outputStream.WriteAsync(ms.ToArray());


                
                Android.Content.ContentResolver resolver = context.ContentResolver;
                Android.Content.ContentValues contentValues = new();
                contentValues.Put(Android.Provider.MediaStore.IMediaColumns.DisplayName, fileName);
                contentValues.Put(Android.Provider.MediaStore.IMediaColumns.MimeType, "image/png");
                contentValues.Put(Android.Provider.MediaStore.IMediaColumns.RelativePath, "DCIM/" + "FashionApp");
                Android.Net.Uri imageUri = resolver.Insert(Android.Provider.MediaStore.Images.Media.ExternalContentUri, contentValues);
                var os = resolver.OpenOutputStream(imageUri);
                Android.Graphics.BitmapFactory.Options options = new();
                options.InJustDecodeBounds = true;
                var bitmap = Android.Graphics.BitmapFactory.DecodeStream(imageStream);
                //var bitmap = _imageData;
                bitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, os);
                os.Flush();
                os.Close();   
                
                await DisplayAlert("Success", "Image saved on DCIM / FashionApp!", "OK");
            }
            else
            {
                //Java.IO.File storagePath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures);
                //string path = System.IO.Path.Combine(storagePath.ToString(), fileName);
                //System.IO.File.WriteAllBytes(path, imageStream.ToArray());
                ////System.IO.File.WriteAllBytes(path, _imageData);
                //var mediaScanIntent = new Android.Content.Intent(Android.Content.Intent.ActionMediaScannerScanFile);
                //mediaScanIntent.SetData(Android.Net.Uri.FromFile(new Java.IO.File(path)));
                //context.SendBroadcast(mediaScanIntent);
            }
#endif
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to save image: {ex.Message}", "OK");
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
} 