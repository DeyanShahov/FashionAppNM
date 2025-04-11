using CommunityToolkit.Maui.Views;
using FashionApp.Data.Constants;

namespace FashionApp.core.services
{
    public class CameraService
    {
        private readonly CameraView _cameraView;
        private readonly Grid _cameraFrame;

        // Декларация на делегат за предаване на потока
        public event Action<Stream> ImageCaptured;

        public CameraService(CameraView cameraView, Grid cameraFrame)
        {
            _cameraView = cameraView;
            _cameraView.MediaCaptured += OnMediaCaptured;
            _cameraFrame = cameraFrame;
        }

        public CameraService(CameraView cameraView)
        {
            _cameraView = cameraView;
            _cameraView.MediaCaptured += OnMediaCaptured;
        }

        public async void StartCamera()
        {
            if (_cameraView != null)
            {
                _cameraView.StopCameraPreview();
                await _cameraView.StartCameraPreview(CancellationToken.None);
                //_cameraView.IsVisible = true;
                //_cameraFrame.IsVisible = true;
                //_cameraFrame.IsEnabled = true;
            }
        }

        public void StopCamera()
        {
            _cameraView.StopCameraPreview();
            //_cameraView.IsVisible = false;
            //_cameraFrame.IsVisible = false;
            //_cameraFrame.IsEnabled = false;
        }

        public async void CaptureClicked()
        {
            await _cameraView.CaptureImage(CancellationToken.None);
        }

        private async void OnMediaCaptured(object sender, MediaCapturedEventArgs e)
        {
            if (e?.Media == null) return;  // Логвайте или обработете грешката, ако няма наличен медия поток.

            try
            { 
                // Вашият код за обработка на изображението
                byte[] imageBuffer;  // Копиране на съдържанието на входния поток в буфер, за да избегнем затварянето му.
                using (var tempStream = new MemoryStream())
                {
                    await e.Media.CopyToAsync(tempStream);
                    imageBuffer = tempStream.ToArray();
                }

                async Task ProcessImageAsync() // Функция за обработка на изображението с новосъздаден MemoryStream.
                {
                    using (var ms = new MemoryStream(imageBuffer))
                    {
                        ms.Position = 0;

                        // Копиране на потока в нов MemoryStream, за да се избегне затварянето на оригиналния поток
                        var resultStream = new MemoryStream();
                        await ms.CopyToAsync(resultStream);
                        resultStream.Position = 0;

                        // Извикване на делегата в основния поток
                        MainThread.BeginInvokeOnMainThread(() => ImageCaptured?.Invoke(resultStream));
                    }
                }

                // Ако Dispatcher изисква превключване, използваме Dispatch.
                var dispatcher = Dispatcher.GetForCurrentThread();
                if (dispatcher != null && dispatcher.IsDispatchRequired) 
                    dispatcher.Dispatch(async () => await ProcessImageAsync());
                else 
                    await ProcessImageAsync();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    AppConstants.Errors.ERROR,
                    $"{AppConstants.Errors.ERROR_PROCESSING_MEDIA}: {ex.Message}",
                    AppConstants.Messages.OK);
            }
        }
    }
} 