using FashionApp.core;
using FashionApp.core.services;
using FashionApp.Data.Constants;
using SkiaSharp;

namespace FashionApp.Pages;

public partial class MaskEditor : ContentPage
{
    private readonly CameraService _cameraService;

    //public MaskEditor(ExecutionGuardService executionGuard)
    public MaskEditor()
    {
        InitializeComponent();

        _cameraService = new CameraService(MyCameraView, CameraPanel);
        _cameraService.ImageCaptured += OnImageCaptured;
        _cameraService.StopCamera();
    }


    //private async void OnBackButtonClicked(object sender, EventArgs e) => await Navigation.PopAsync();
    private async void OnSelectImageClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Images,
                PickerTitle = AppConstants.Messages.PICK_AN_IMAGE
            });

            if (result != null)
            {           
                bool pageAlreadyExists = Navigation.ModalStack.Any(p => p is ImageEditPage);
                if (pageAlreadyExists) return;

                //string taskKey = AppConstants.Pages.IMAGE_EDIT;
                //if (!_executionGuardService.TryStartTask(taskKey)) return;

                try
                {
                    //await Navigation.PushModalAsync(new ImageEditPage(result.FullPath));
                    var page = MauiProgram.ServiceProvider.GetRequiredService<ImageEditPage>();
                    page.ImageUri = result.FullPath;
                    await Navigation.PushModalAsync(page);
                }
                finally
                {
                   // _executionGuardService.FinishTask(taskKey);
                }
            }
            else
                await DisplayAlert(AppConstants.Errors.ERROR, AppConstants.Errors.SELECT_A_VALID_IMAGE, AppConstants.Messages.OK);

        }
        catch (Exception ex)
        {
            await DisplayAlert(AppConstants.Errors.ERROR, $"{AppConstants.Errors.ERROR}: {ex.Message}", AppConstants.Messages.OK);
        }
    }

    private async void OnImageCaptured(Stream imageStream)
    {
        await ProcessSelectedImage(imageStream);
        _cameraService.StopCamera();
        HideMenus();

        CameraButtonsPanel.IsEnabled = true;
    }

    private async void OpenCamera_Clicked(object sender, EventArgs e)
    {
        // 1. Проверка и искане на разрешение ПРЕДИ употреба
        var status = await PermissionService.CheckAndRequestCameraAsync();
        // 2. Проверка на резултата СЛЕД искането
        if (status != PermissionStatus.Granted)
        {
            await DisplayAlert("Отказ", "Не можем да използваме камерата без разрешение.", "OK");
            return; // Прекратяване на действието
        }

        _cameraService.StartCamera();
        HideMenus();
    }
    private void HidePanelCommand(object sender, EventArgs e)
    {     
        _cameraService.StopCamera();
        HideMenus();
    }
    private void OnCaptureClicked(object sender, EventArgs e)
    {
        CameraButtonsPanel.IsEnabled = false;
        _cameraService.CaptureClicked();
    }

    private void HideMenus()
    {
        Menu1.IsVisible = !Menu1.IsVisible;
        CameraPanel.IsVisible = !CameraPanel.IsVisible;
        CameraPanel.IsEnabled = !CameraPanel.IsEnabled;
    }


    private async Task ProcessSelectedImage(Stream? stream)
    {             
        bool pageAlreadyExists = Navigation.ModalStack.Any(p => p is ImageEditPage);
        if (pageAlreadyExists) return;

        //string taskKey = AppConstants.Pages.IMAGE_EDIT;
        //if (!_executionGuardService.TryStartTask(taskKey)) return;

        try
        {
            // Преоразмеряване на изображението
            var resizedImageResult = await ImageStreamResize.ResizeImageStream(stream, 500, 700); // Преоразмеряване на изображението

            // Конвертиране на изображението, за да включва алфа канал
            var imageWithAlpha = await AddAlphaChanel(resizedImageResult.ResizedStream);

            //  await Navigation.PushModalAsync(new ImageEditPage(imageWithAlpha));
            var page = MauiProgram.ServiceProvider.GetRequiredService<ImageEditPage>();
            page.ImageStream = imageWithAlpha;
            await Navigation.PushModalAsync(page);
        }
        finally
        {
            //_executionGuardService.FinishTask(taskKey);
        }
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
}


