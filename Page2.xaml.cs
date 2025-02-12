using System.Text.Json;
using System.Text;
using System.IO;
using System.Collections.Generic;
using SkiaSharp;
using Microsoft.Maui.Controls.PlatformConfiguration;
//using static AndroidX.Concurrent.Futures.CallbackToFutureAdapter;
using Microsoft.Maui.Controls;

namespace FashionApp;

public partial class Page2 : ContentPage
{
    private readonly HttpClient _client = new HttpClient { Timeout = TimeSpan.FromSeconds(300) };
    private const string ApiUrl = "https://eminently-verified-walleye.ngrok-free.app";
    //private const string ApiUrl = "http://192.168.0.101:80";
    //private const string ApiUrl = "http://127.0.0.1:80";
    private byte[] _imageData;
    private string _clothImagePath;
    private string _bodyImagePath;
    private bool isClosedJacketActive = true;

    public Page2()
    {
        InitializeComponent();
        //SetActiveButton(true); // Set initial state

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

    private async void CombineImagesClicked(object sender, EventArgs e)
    {
        if (SelectedClothImage.Source == null || SelectedBodyImage.Source == null)
        {
            await DisplayAlert("Error", "Please select both images first", "OK");
            return;
        }

        try
        {
            //var uploader = new ComfyUIUploader(ApiUrl);
            //if (!string.IsNullOrEmpty(_clothImagePath) && !string.IsNullOrEmpty(_bodyImagePath))
            //{
            //    await uploader.UploadImageAsync(_clothImagePath, "input"); // Качваме първата картинка
            //    await uploader.UploadImageAsync(_bodyImagePath, "input"); // Качваме втората картинка
            //}
            //else
            //{
            //    await DisplayAlert("Error", "Please select both images first", "OK");
            //}

            ToggleLoading(true);
            ResponseImage.IsVisible = false;
            ResponseText.IsVisible = false;
            SaveButton.IsVisible = false;
            SaveButton.IsEnabled = false;

            UploadImages(); // Качваме двата файла

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

    private async void UploadImages()
    {
        var uploader = new ComfyUIUploader(ApiUrl);
        if (!string.IsNullOrEmpty(_clothImagePath) && !string.IsNullOrEmpty(_bodyImagePath))
        {
            await uploader.UploadImageAsync(_clothImagePath, "input"); // Качваме първата картинка
            await uploader.UploadImageAsync(_bodyImagePath, "input"); // Качваме втората картинка
        }
        else
        {
            await DisplayAlert("Error", "Please select both images first", "OK");
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (_imageData == null)
            return;

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
                contentValues.Put(Android.Provider.MediaStore.IMediaColumns.RelativePath, "DCIM/" + "FashionApp");
                Android.Net.Uri imageUri = resolver.Insert(Android.Provider.MediaStore.Images.Media.ExternalContentUri, contentValues);
                var os = resolver.OpenOutputStream(imageUri);
                Android.Graphics.BitmapFactory.Options options = new();
                options.InJustDecodeBounds = true;
                var bitmap = Android.Graphics.BitmapFactory.DecodeStream(stream);
                //var bitmap = _imageData;
                bitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, os);
                os.Flush();
                os.Close();

                await DisplayAlert("Success", $"Image saved on DCIM / FashionApp!", "OK");
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
        CombineImages.IsEnabled = !isLoading;
        LoadingIndicator.IsRunning = isLoading;
        LoadingIndicator.IsVisible = isLoading;
    }


    private async void OpenJacketImageButton_Clicked(object sender, EventArgs e)
    {
        SetActiveButton(sender as ImageButton);
#if WINDOWS
        await LoadCorrectImageMaskWindows("open_jacket_mask.png");
#elif ANDROID
        await LoadCorrectImageMaskAndroid("open_jacket_mask.png");
#endif
    }

    private async void ClosedJacketImageButton_Clicked(object sender, EventArgs e)
    {
        SetActiveButton(sender as ImageButton);
#if WINDOWS
        await LoadCorrectImageMaskWindows("closed_jacket_mask.png");
#elif ANDROID
        await LoadCorrectImageMaskAndroid("closed_jacket_mask.png");
#endif
    }


#if ANDROID
    private async Task LoadCorrectImageMaskAndroid(string fileName)
    {
        try
        {
            // Проверка за permissions
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
                var selection = $"{Android.Provider.MediaStore.IMediaColumns.DisplayName} = ? AND " +
                              $"{Android.Provider.MediaStore.IMediaColumns.RelativePath} LIKE ?";
                var selectionArgs = new string[] { fileName, "%DCIM/FashionApp/MasksImages%" };
                
                var cursor = resolver.Query(imageUri, null, selection, selectionArgs, null);
                
                if (cursor != null && cursor.MoveToFirst())
                {
                    var columnIndex = cursor.GetColumnIndex(Android.Provider.MediaStore.IMediaColumns.Data);
                    if (columnIndex != -1)
                    {
                        var imagePath = cursor.GetString(columnIndex);
                        using var inputStream = resolver.OpenInputStream(Android.Net.Uri.Parse("file://" + imagePath));
                        if (inputStream != null)
                        {
                            var memoryStream = new MemoryStream();
                            await inputStream.CopyToAsync(memoryStream);
                            memoryStream.Position = 0;
                            
                            SelectedBodyImage.Source = ImageSource.FromStream(() => new MemoryStream(memoryStream.ToArray()));
                            _bodyImagePath = imagePath;
                        }
                    }
                    cursor.Close();
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
    
    private void CheckAvailableMasksAndroid()
    {
        try 
        {
            var context = Platform.CurrentActivity;
            var resolver = context?.ContentResolver;
            
            if (resolver != null)
            {
                // Проверка за closed_jacket_mask.png
                var closedJacketUri = Android.Provider.MediaStore.Images.Media.ExternalContentUri;
                var closedJacketSelection = $"{Android.Provider.MediaStore.IMediaColumns.DisplayName} = ? AND " +
                                          $"{Android.Provider.MediaStore.IMediaColumns.RelativePath} LIKE ?";
                var closedJacketSelectionArgs = new string[] { "closed_jacket_mask.png", "%DCIM/FashionApp/MasksImages%" };
                
                var closedJacketCursor = resolver.Query(closedJacketUri, null, closedJacketSelection, 
                                                       closedJacketSelectionArgs, null);
                
                if (closedJacketCursor != null && closedJacketCursor.Count > 0)
                {
                    ClosedJacketImageButton.IsEnabled = true;
                    ClosedJacketImageButton.IsVisible = true;
                }
                closedJacketCursor?.Close();
                
                // Проверка за open_jacket_mask.png
                var openJacketUri = Android.Provider.MediaStore.Images.Media.ExternalContentUri;
                var openJacketSelection = $"{Android.Provider.MediaStore.IMediaColumns.DisplayName} = ? AND " +
                                        $"{Android.Provider.MediaStore.IMediaColumns.RelativePath} LIKE ?";
                var openJacketSelectionArgs = new string[] { "open_jacket_mask.png", "%DCIM/FashionApp/MasksImages%" };
                
                var openJacketCursor = resolver.Query(openJacketUri, null, openJacketSelection, 
                                                       openJacketSelectionArgs, null);
                
                if (openJacketCursor != null && openJacketCursor.Count > 0)
                {
                    OpenJacketImageButton.IsEnabled = true;
                    OpenJacketImageButton.IsVisible = true;
                }
                openJacketCursor?.Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error checking masks: {ex.Message}");
        }
    }


#endif


}
