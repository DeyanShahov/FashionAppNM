using System.Text.Json;
using System.Text;
using System.IO;
using System.Collections.Generic;
using SkiaSharp;
using Microsoft.Maui.Controls.PlatformConfiguration;
//using static AndroidX.Concurrent.Futures.CallbackToFutureAdapter;
using Microsoft.Maui.Controls;
using FashionApp.core.services;
using System;
using FashionApp.Data.Constants;
using CommunityToolkit.Maui.Views;

using FashionApp.core;
using Microsoft.Maui;


#if __ANDROID__
using Android.Content;
using Android.Database;
using Android.Net;
using Android.Provider;
#endif

namespace FashionApp;

public partial class CombineImages : ContentPage
{
    private readonly HttpClient _client = new HttpClient { Timeout = TimeSpan.FromSeconds(300) };
    private const string ApiUrl = "https://eminently-verified-walleye.ngrok-free.app";
    //private const string ApiUrl = "http://192.168.0.101:80";
    //private const string ApiUrl = "http://127.0.0.1:80";
    private byte[] _imageData;
    private string _clothImagePath;
    private string _bodyImagePath;
    private bool isClosedJacketActive = true;

    private readonly SingleImageLoader singleImageLoader;
    private CameraService _cameraService;

    public CombineImages()
    {
        InitializeComponent();
        //SetActiveButton(true); // Set initial state

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
        CheckAvailableMasksAndroid();
#endif
    }

    private async void OnNavigateClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    private async void OnSelectClothImageClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Images,
                PickerTitle = "Pick a cloth image"
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
                    SelectedClothImage.Source = ImageSource.FromStream(() => new MemoryStream(memoryStream.ToArray()));
                    SelectedClothImage.IsVisible = true;
                    _clothImagePath = result.FullPath; // Запазваме пътя на избрания файл
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

    private async void OnSelectBodyImageClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Images,
                PickerTitle = "Pick a body image"
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
                    SelectedBodyImage.Source = ImageSource.FromStream(() => new MemoryStream(memoryStream.ToArray()));
                    SelectedBodyImage.IsVisible = true;
                    _bodyImagePath = result.FullPath; // Запазваме пътя на избрания файл
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

    private async void OnCombineImages_Clicked(object sender, EventArgs e)
    {
        if (SelectedClothImage.Source == null || SelectedBodyImage.Source == null)
        {
            await DisplayAlert("Error", "Please select both images first", "OK");
            return;
        }

        try
        {
            ToggleLoading(true);
            ResponseImage.IsVisible = false;
            ResponseText.IsVisible = false;
            SaveButton.IsVisible = false;
            SaveButton.IsEnabled = false;

            if (!await UploadImages()) return; // Качваме двата файла

            // Изпращаме POST заявка към API
            await Task.Delay(5000); // Изчакване от 5 секунди

            var requestUrl = $"{ApiUrl}/combine_images";
            var requestBody = new
            {
                function_name = "generate_image",
                cloth_image = "input_image_cloth.png", //Path.GetFileName(_clothImagePath),
                body_image = "input_image_body.png" //Path.GetFileName(_bodyImagePath)
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _client.PostAsync(requestUrl, jsonContent);

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"HTTP error: {response.StatusCode}");
            }

            var contentType = response.Content.Headers.ContentType?.MediaType;
            _imageData = await response.Content.ReadAsByteArrayAsync();

            if (contentType != null && contentType.StartsWith("image/"))
            {
                ResponseImage.Source = ImageSource.FromStream(() => new MemoryStream(_imageData));
                ResponseImage.IsVisible = true;
                SaveButton.IsVisible = true;
                SaveButton.IsEnabled = true;
            }
            else
            {
                //var errorMessage = Encoding.UTF8.GetString(_imageData);
                //ResponseText.IsVisible = true;
                //throw new Exception(errorMessage);

                ResponseText.Text = Encoding.UTF8.GetString(_imageData);
                ResponseText.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            ResponseText.Text = $"Error: {ex.Message}";
            ResponseText.IsVisible = true;
        }
        finally
        {
            ToggleLoading(false);
        }
    }

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
            await DisplayAlert("Error", "Missing path to files!", "OK");
            return false;
        }
    }

    private static void DeleteTemporaryImage()
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
                // Логнете грешката, ако се появи (например чрез Debug)
                System.Diagnostics.Debug.WriteLine($"Грешка при изтриване на файл {oldFile}: {ex.Message}");
            }
        }
    }

    private async Task<string> SetCameraImageRealPathForSelectedClothImage(Stream stream)
    {
        //if (SelectedClothImage.Source is StreamImageSource streamImageSource)
        //{
        //    string tempPath = Path.Combine(FileSystem.CacheDirectory, "testImage.png");
        //    //using (Stream stream = await streamImageSource.Stream(CancellationToken.None))
        //    //{
        //    //    using (var fileStream = File.Create(tempPath))
        //    //    {
        //    //        await stream.CopyToAsync(fileStream);
        //    //        _clothImagePath = tempPath; // Запазвате пътя във вашия параметър.
        //    //    }
        //    //}

        //    Stream stream = await streamImageSource.Stream(CancellationToken.None);
        //    using (var fileStream = File.Create(tempPath))
        //    {
        //        await stream.CopyToAsync(fileStream);
        //        _clothImagePath = tempPath; // Запазвате пътя във вашия параметър.
        //    }
        //}

        DeleteTemporaryImage(); // Изтриваме ако е имало предищно неизтрита снимка

        // Ако потокът е seekable, може да се наложи да го ресетнете:
        if (stream.CanSeek)
        {
            stream.Position = 0;
        }

        string tempPath = Path.Combine(FileSystem.CacheDirectory, $"testImage_{DateTime.Now.Ticks}.png");

        using (var fileStream = File.Create(tempPath))
        {
            await stream.CopyToAsync(fileStream);
        }

        return tempPath;
    }


    private void OnCaptureClicked(object sender, EventArgs e)
    {
        _cameraService.CaptureClicked();
    }

    private async void OnImageCaptured(Stream imageStream)
    {
        await ProcessSelectedImage(imageStream);
        _cameraService.StopCamera();
        HideMenus();
    }

    private void PanelButton_Clicked(object sender, EventArgs e)
    {
        HideMenus();

        _cameraService.StartCamera();
    }

    private void HidePanelCommand(object sender, EventArgs e)
    {

        _cameraService.StopCamera();
        HideMenus();
    }

    private void HideMenus()
    {
        Menu1.IsVisible = !Menu1.IsVisible;
        //SelectedClothImage.IsVisible = !SelectedClothImage.IsVisible;
    }


    private async Task ProcessSelectedImage(Stream? stream)
    {
        // Преоразмеряване на изображението
        var resizedImageResult = await ImageStreamResize.ResizeImageStream(stream, 500, 700); // Преоразмеряване на изображението
        var resultPath = await SetCameraImageRealPathForSelectedClothImage(resizedImageResult.ResizedStream);
        _clothImagePath = resultPath;

        // Конвертиране на изображението, за да включва алфа канал
        //var imageWithAlpha = await AddAlphaChanel(resizedImageResult.ResizedStream);

        // Задаване на източника на изображението
        //SelectedClothImage.Source = ImageSource.FromStream(() => resizedImageResult.ResizedStream);
        SelectedClothImage.Source = ImageSource.FromFile(resultPath);


        // Настройка на ширината и височината на изображението
        SelectedClothImage.WidthRequest = Application.Current.MainPage.Width; // Задаване на ширината на екрана
        SelectedClothImage.HeightRequest = Application.Current.MainPage.Height; // Задаване на височината на екрана

        // Центриране на изображението
        SelectedClothImage.HorizontalOptions = LayoutOptions.Center;
        SelectedClothImage.VerticalOptions = LayoutOptions.Center;

        // Показване на елементите
        SelectedClothImage.IsVisible = true;

        //HideMenus();

        //CameraPanel.IsVisible = false;
        //CameraPanel.IsEnabled = false;
    }

   

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (_imageData == null) return;

        try
        {
            var fileName = $"image_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            //var filePath = Path.Combine(FileSystem.AppDataDirectory, fileName);               

            //await File.WriteAllBytesAsync(filePath, _imageData);

            //await DisplayAlert("Success", $"Image saved to {filePath}", "OK");

            using var stream = new MemoryStream(_imageData);
            stream.Position = 0;

#if WINDOWS
                await File.WriteAllBytesAsync($"C:\\Users\\Public\\Pictures\\{fileName}", _imageData);
                await DisplayAlert("Success", $"Image saved to C:\\Users\\Public\\Pictures", "OK");

#elif ANDROID
            var context = Platform.CurrentActivity;

            if (OperatingSystem.IsAndroidVersionAtLeast(29))
            {
                Android.Content.ContentResolver resolver = context.ContentResolver;
                Android.Content.ContentValues contentValues = new();
                contentValues.Put(Android.Provider.MediaStore.IMediaColumns.DisplayName, fileName);
                contentValues.Put(Android.Provider.MediaStore.IMediaColumns.MimeType, "image/png");
                contentValues.Put(Android.Provider.MediaStore.IMediaColumns.RelativePath, AppConstants.IMAGES_DIRECTORY);
                Android.Net.Uri imageUri = resolver.Insert(Android.Provider.MediaStore.Images.Media.ExternalContentUri, contentValues);
                var os = resolver.OpenOutputStream(imageUri);
                Android.Graphics.BitmapFactory.Options options = new();
                options.InJustDecodeBounds = true;
                var bitmap = Android.Graphics.BitmapFactory.DecodeStream(stream);
                //var bitmap = _imageData;
                bitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, os);
                os.Flush();
                os.Close();

                await DisplayAlert("Success", $"Image saved on {AppConstants.IMAGES_DIRECTORY}", "OK");
            }
            else
            {
                Java.IO.File storagePath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures);
                string path = System.IO.Path.Combine(storagePath.ToString(), fileName);
                System.IO.File.WriteAllBytes(path, stream.ToArray());
                //System.IO.File.WriteAllBytes(path, _imageData);
                var mediaScanIntent = new Android.Content.Intent(Android.Content.Intent.ActionMediaScannerScanFile);
                mediaScanIntent.SetData(Android.Net.Uri.FromFile(new Java.IO.File(path)));
                context.SendBroadcast(mediaScanIntent);
            }
#endif
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to save image: {ex.Message}", "OK");
        }
    }

    private void ToggleLoading(bool isLoading)
    {
        CombineImagesButton.IsEnabled = !isLoading;
        LoadingIndicator.IsRunning = isLoading;
        LoadingIndicator.IsVisible = isLoading;
    }


    private async void OpenJacketImageButton_Clicked(object sender, EventArgs e)
    {
        SetActiveButton(sender as ImageButton);
#if WINDOWS
        await LoadCorrectImageMaskWindows(AppConstants.OPEN_JACKET_MASK);
#elif ANDROID
        //await LoadCorrectImageMaskAndroid(AppConstants.OPEN_JACKET_MASK);
        LoadLargeImage(AppConstants.OPEN_JACKET_MASK);
#endif
    }

    private async void ClosedJacketImageButton_Clicked(object sender, EventArgs e)
    {
        SetActiveButton(sender as ImageButton);
#if WINDOWS
        await LoadCorrectImageMaskWindows(AppConstants.CLOSED_JACKET_MASK);
#elif ANDROID
        //await LoadCorrectImageMaskAndroid(AppConstants.CLOSED_JACKET_MASK);
        //LoadLargeImage(_bodyImagePath);
        LoadLargeImage(AppConstants.CLOSED_JACKET_MASK);
#endif
    }


#if ANDROID
    private async void LoadLargeImage(string imageUri)
    {           
        var imagePath = ConvertCachePathToImagePath(imageUri); // Извличане на пътя на първото изображение в списъка    
        _bodyImagePath = imagePath;
        await singleImageLoader.LoadSingleImageAsync(imagePath);  // Зареждане на изображението асинхронно
    }

    private string ConvertCachePathToImagePath(string cachePath)
    {
        // Проверка дали пътят започва с кеш директорията
        string imagesDirectory = "/storage/emulated/0/Pictures/FashionApp/MasksImages/";
    
        if (true)
        {
            // Извличане на името на файла от кеш пътя
            //string fileName = Path.GetFileName(cachePath);
            // Създаване на новия път в директорията с изображения
            string newPath = Path.Combine(imagesDirectory, cachePath);
            return newPath;
        }
    
        // Връща оригиналния път, ако не започва с кеш директорията
        return cachePath;
    }


    public string GetRealPathFromUri(ContentResolver contentResolver, string imageUri)
    {
        // Проверка дали URI-то започва с "content://"
        if (imageUri.StartsWith("content://"))
        {
            var uri = Android.Net.Uri.Parse(imageUri);
            string filePath = null;

            // Извличане на пътя на файла от MediaStore
            using (var cursor = contentResolver.Query(uri, null, null, null, null))
            {
                if (cursor != null && cursor.MoveToFirst())
                {
                    int columnIndex = cursor.GetColumnIndex(MediaStore.MediaColumns.Data);
                    filePath = cursor.GetString(columnIndex);
                }
            }

            // Проверка дали файлът съществува
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                return filePath; // Връща пълния път до файла
            }
        }

        return null; // Връща null, ако не е намерен път
    }

    private async Task LoadCorrectImageMaskAndroid(string fileName)
    {
        try
        {
            // Проверка за permissions
            var testPermission = PermissionStatus.Unknown;
            testPermission = await Permissions.CheckStatusAsync<Permissions.Photos>();
            //if(testPermission == PermissionStatus.Granted) return;
            if (Permissions.ShouldShowRationale<Permissions.Photos>())
            {
                await DisplayAlert("Permission required", "Location permission is required to load images", "OK");
            }
            testPermission = await Permissions.RequestAsync<Permissions.Photos>();
            if(testPermission != PermissionStatus.Granted)
            {
                await DisplayAlert("Permission required", "Location permission is required to load images", "OK");
                return;
            }

            var test2 = await Permissions.RequestAsync<Permissions.Media>();

            var status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.StorageRead>();
                if (status != PermissionStatus.Granted)
                {
                    await DisplayAlert("Permission required", 
                        "Storage permission is required to load images", "OK");
                    return;
                }
            }

            var context = Platform.CurrentActivity;
            var resolver = context?.ContentResolver;
            
            if (resolver != null)
            {
                var imageUri = Android.Provider.MediaStore.Images.Media.ExternalContentUri;
                var selection = Android.Provider.MediaStore.IMediaColumns.RelativePath + "=? AND " +
                              Android.Provider.MediaStore.IMediaColumns.DisplayName + "=?";
                var selectionArgs = new string[] { "Pictures/FashionApp/MasksImages/", fileName };
                
                using var cursor = resolver.Query(imageUri, null, selection, selectionArgs, null);
                
                if (cursor != null && cursor.MoveToFirst())
                {
                    var dataColumn = cursor.GetColumnIndex(Android.Provider.MediaStore.IMediaColumns.Data);
                    if (dataColumn != -1)
                    {
                        var filePath = cursor.GetString(dataColumn);
                        var contentUri = Android.Net.Uri.Parse("file://" + filePath);
                        
                        using var inputStream = resolver.OpenInputStream(contentUri);
                        if (inputStream != null)
                        {
                            var memoryStream = new MemoryStream();
                            await inputStream.CopyToAsync(memoryStream);
                            memoryStream.Position = 0;
                            
                            SelectedBodyImage.Source = ImageSource.FromStream(() => new MemoryStream(memoryStream.ToArray()));
                            _bodyImagePath = filePath;
                        }
                    }
                }
                else
                {
                    await DisplayAlert("Error", $"File not found: {fileName}", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", 
                $"An error occurred: {ex.Message}\nStack: {ex.StackTrace}", "OK");
        }
    }
#endif

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
                await DisplayAlert("Error", "File not found: closed_jacket_mask.png", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
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

            // Ако поне един бутон е активен, активираме първия наличен
            //if (ClosedJacketImageButton.IsEnabled)
            //{
            //    SetActiveButton(true);
            //}
            //else if (OpenJacketImageButton.IsEnabled)
            //{
            //    SetActiveButton(false);
            //}
        }
        catch (Exception ex)
        {
            // В случай на грешка при достъп до файловата система
            Console.WriteLine($"Error checking masks: {ex.Message}");
        }
    }
#elif ANDROID
    private async void CheckAvailableMasksAndroid()
    {
        var testPermission = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
        if (Permissions.ShouldShowRationale<Permissions.StorageRead>())
        {
            await DisplayAlert("Error", "Storage permission is required to access storage.", "OK");
        }
        testPermission = await Permissions.RequestAsync<Permissions.StorageRead>();
        if (testPermission != PermissionStatus.Granted)
        {
            await DisplayAlert("Error", "Storage permission is required to load images.", "OK");
            return;
        }

        try 
        {
            var fileChecker = App.Current.Handler.MauiContext.Services.GetService<IFileChecker>();

            var fileExists = await fileChecker.CheckFileExistsAsync("closed_jacket_mask.png");
            if (fileExists)
            {
                ClosedJacketImageButton.IsEnabled = true;
                ClosedJacketImageButton.IsVisible = true;
            }

            fileExists = await fileChecker.CheckFileExistsAsync("open_jacket_mask.png");
            if (fileExists)
            {
                OpenJacketImageButton.IsEnabled = true;
                OpenJacketImageButton.IsVisible = true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking masks: {ex.Message}");
        }
    }
#endif
}
