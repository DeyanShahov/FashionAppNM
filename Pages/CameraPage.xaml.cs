using FashionApp.core;
using FashionApp.core.services;
using FashionApp.Data.Constants;

namespace FashionApp.Pages;

public partial class CameraPage : ContentPage
{
    private CameraService _cameraService;
    public Image _imageThobeSet;
    public string _imagePathThobeSet;

    public CameraPage(Image imageThobeSet, ref string imagePathThobeSet)
	{
		InitializeComponent();

        _imageThobeSet = imageThobeSet;
        _imagePathThobeSet = imagePathThobeSet;


        _cameraService = new CameraService(MyCameraView);
        _cameraService.ImageCaptured += OnImageCaptured;
        _cameraService.StopCamera();
    }

    private void Capture_Clicked(object sender, EventArgs e)
	{
        _cameraService.CaptureClicked();
    }

    private async void Close_Clicked(object sender, EventArgs e) => await CloseCamera();


    private async Task CloseCamera()
    {
        _cameraService.StopCamera();
        await Navigation.PopModalAsync();
    }


    private async void OnImageCaptured(Stream imageStream)
    {
        try
        {
            await ProcessSelectedImage(imageStream);
        }
        catch (Exception ex)
        {
            await DisplayAlert(AppConstants.Errors.ERROR, $"{ex?.Message}\n{ex?.StackTrace}", AppConstants.Messages.OK);
        }

    }

    private async Task ProcessSelectedImage(Stream? stream)
    {
        var resizedImageResult = await ImageStreamResize.ResizeImageStream(stream, 500, 700); // Преоразмеряване на изображението
        var resultPath = await SetCameraImageRealPathForSelectedClothImage(resizedImageResult.ResizedStream);

        _imagePathThobeSet = resultPath;
        _imageThobeSet.Source = ImageSource.FromFile(resultPath);

        await CloseCamera();
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
}