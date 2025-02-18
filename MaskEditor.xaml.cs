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

#if __ANDROID__
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
    private HashSet<(int x, int y)> _markedPixels = new();
    private bool isClosedJacketActive = true;
    private string imageFileName = "closed_jacket_mask.png";

    public MaskEditor()
    {
        InitializeComponent();
        _drawable = new DrawingViewDrawable(_lines, _markedPixels);
        DrawingView.Drawable = _drawable;
        //SetActiveButton(true); // Задаваме начално състояние
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
            else
            {
                throw new Exception("Touch is outside the DrawingView");
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
            var context = Platform.CurrentActivity;
            string directoryPath = Path.Combine(Android.OS.Environment.DirectoryPictures, "FashionApp", "MasksImages");

            if (OperatingSystem.IsAndroidVersionAtLeast(29))
            {
                Android.Content.ContentResolver resolver = context.ContentResolver;
                Android.Content.ContentValues contentValues = new();

                //await DeleteExistingImageAsync(resolver, imageFileName, directoryPath);

                // Създаване на нов запис
                contentValues.Put(Android.Provider.MediaStore.IMediaColumns.DisplayName, imageFileName);
                contentValues.Put(Android.Provider.MediaStore.IMediaColumns.MimeType, "image/png");
                contentValues.Put(Android.Provider.MediaStore.IMediaColumns.RelativePath, directoryPath);

                Android.Net.Uri? imageUri = resolver.Insert(Android.Provider.MediaStore.Images.Media.ExternalContentUri, contentValues);
                if (imageUri == null)
                {
                    await DisplayAlert("Error", "Failed to create image file.", "OK");
                    return;
                }

                using (var os = resolver.OpenOutputStream(imageUri))
                {
                    if (os != null)
                    {
                        var bitmap = Android.Graphics.BitmapFactory.DecodeStream(resultStream);
                        bitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, os);
                        os.Flush();
                    }
                }
                
                await DisplayAlert("Success", $"Image saved on Pictures / FashionApp / MasksImages as {imageFileName}", "OK");

                imageFileName = string.Empty; // Reset value of paramether
            }
            else
            {
                // Handle older Android versions if needed
            }
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


#if ANDROID
    private async Task DeleteExistingImageAsync(ContentResolver resolver, string imageFileName, string directoryPath)
    {
        string[] projection = { IBaseColumns.Id, MediaStore.MediaColumns.DisplayName };
        Android.Net.Uri collection = MediaStore.Images.Media.ExternalContentUri;
        
        // Добавяне на условие за търсене по fileName
        string selection = $"{MediaStore.MediaColumns.DisplayName} = ?";
        string[] selectionArgs = { imageFileName };
        
        var cursor = resolver.Query(collection, projection, selection, selectionArgs, null);
        if (cursor != null && cursor.MoveToFirst())
        {
            int idColumn = cursor.GetColumnIndex(IBaseColumns.Id);
            long imageId = cursor.GetLong(idColumn); // Уникален ID на изображението
            Android.Net.Uri deleteUri = ContentUris.WithAppendedId(MediaStore.Images.Media.ExternalContentUri, imageId);
            int deletedRows = resolver.Delete(deleteUri, null, null);
            if (deletedRows > 0)
            {
                await DisplayAlert("Succes", $"{imageFileName} dellet succesful.", "OK");
            }
            else
            {
                //Console.WriteLine("Failed to delete image. It might not be owned by the app.");
                await DisplayAlert("Error", "Failed to delete image. It might not be owned by the app.", "OK");
            }
            //Console.WriteLine($"Image ID: {imageId}");
        }
        cursor?.Close();

        if (true)
        {
            // Scoped Storage (Android 10+) - само файлове, създадени от приложението, могат да се изтриват
            //string selection = $"{MediaStore.MediaColumns.DisplayName} = ? AND {MediaStore.MediaColumns.RelativePath} = ?";
            //string[] selectionArgs = new string[] { imageFileName, directoryPath };
    
            //Android.Net.Uri collection = MediaStore.Images.Media.ExternalContentUri;
            //Android.Database.ICursor? cursor = resolver.Query(collection, new string[] { IBaseColumns.Id }, selection, selectionArgs, null);
    
            //if (cursor != null && cursor.MoveToFirst())
            //{
            //    int idColumn = cursor.GetColumnIndex(IBaseColumns.Id);
            //    long id = cursor.GetLong(idColumn);
            //    Android.Net.Uri deleteUri = ContentUris.WithAppendedId(MediaStore.Images.Media.ExternalContentUri, id);
    
            //    try
            //    {
            //        int rowsDeleted = resolver.Delete(deleteUri, null, null);
            //        if (rowsDeleted > 0)
            //        {
            //            await DisplayAlert("Succes", $"{imageFileName} dellet succesful.", "OK");
            //        }
            //        else
            //        {
            //            await DisplayAlert("Error", $"{imageFileName} dont delete", "OK");
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        await DisplayAlert("Error", $"Error on delete: {ex.Message}", "OK");
            //    }
            //}
    
            //cursor?.Close();
        }
        else
        {
            // Android 9 или по-старо - използвай директно File API
            string filePath = Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures)?.AbsolutePath ?? "", directoryPath, imageFileName);
            Java.IO.File file = new Java.IO.File(filePath);
    
            if (file.Exists())
            {
                bool deleted = file.Delete();
                Console.WriteLine(deleted ? $"Файлът {imageFileName} беше изтрит." : $"Неуспешно изтриване на {imageFileName}.");
            }
        }
    }
#endif


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

    private async void ClosedJacketImageButton_Clicked(object sender, EventArgs e)
    {
#if ANDROID
        var result = await CheckAvailableMasksAndroid("closed_jacket_mask.png");

        if(result)
        {
             bool conifirmation = await Application.Current.MainPage.DisplayAlert("Replace Confirmation", "Are you sure you want to replace the mask?", "Yes", "Cancel");

             if(!conifirmation) return;
        }
        else
        {
            await DisplayAlert("Set Confirmation", "You will save a new mask image. ", "OK");
        }
#endif

        SetActiveButton(sender as ImageButton);
        imageFileName = "closed_jacket_mask.png";
    }

    private void OpenJacketImageButton_Clicked(object sender, EventArgs e)
    {
        SetActiveButton(sender as ImageButton);
        imageFileName = "open_jacket_mask.png";
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



#if ANDROID
    private async Task<bool> CheckAvailableMasksAndroid(string fileName)
    {
        try
        {
            var fileChecker = App.Current.Handler.MauiContext.Services.GetService<IFileChecker>();

            var fileExists = await fileChecker.CheckFileExistsAsync(fileName);
            if (fileExists)
            {
                return true;
                //ClosedJacketImageButton.IsEnabled = true;
                //ClosedJacketImageButton.IsVisible = true;
            }
            else
            {
                return false;
            }


            //fileExists = await fileChecker.CheckFileExistsAsync("open_jacket_mask.png");
            //if (fileExists)
            //{
            //    OpenJacketImageButton.IsEnabled = true;
            //    OpenJacketImageButton.IsVisible = true;
            //}
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking masks: {ex.Message}");
            await Application.Current.MainPage.DisplayAlert("Error checking masks", ex.Message, "Ok");
            return false;
        }
    }
#endif
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

//internal class MyPhotosPermissionService : BasePlatformPermission
//{
//   public override (string androidPermission, bool isRuntime)[] RequiredPermissions => 
//        new List<(string androidPermission, bool isRuntime)>
//   {
//        ("android.permission.READ_EXTERNAL_STORAGE", true),
//        ("android.permission.WRITE_EXTERNAL_STORAGE", true)
//       //(Android.Manifest.Permission.ReadExternalStorage, true),
//       //(Android.Manifest.Permission.WriteExternalStorage, true)
//   }.ToArray();
//}

