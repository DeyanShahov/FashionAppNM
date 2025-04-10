using FashionApp.core.services;
using FashionApp.Data.Constants;

namespace FashionApp.core.Services
{
    public class ImageSelectionService : IImageSelectionService
    {
        // Може да се инжектира DisplayAlert функционалност чрез интерфейс
        private readonly Func<string, string, string, Task> _displayAlert;

        public ImageSelectionService(Func<string, string, string, Task> displayAlert)
        {
            _displayAlert = displayAlert;
        }

        public async Task SelectImageFromFilePickerAsync(Action<ImageSource, string> updateAction)
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
                    if (result.FileName.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
                        result.FileName.EndsWith("png", StringComparison.OrdinalIgnoreCase))
                    {
                        using var stream = await result.OpenReadAsync();
                        using var memoryStream = new MemoryStream();
                        await stream.CopyToAsync(memoryStream);
                        memoryStream.Position = 0;

                        var imageSource = ImageSource.FromStream(() => new MemoryStream(memoryStream.ToArray()));
                        updateAction(imageSource, result.FullPath);
                    }
                    else
                    {
                        await _displayAlert(AppConstants.Errors.ERROR, AppConstants.Errors.SELECT_A_VALID_IMAGE, AppConstants.Messages.OK);
                    }
                }
            }
            catch (Exception ex)
            {
                await _displayAlert(AppConstants.Errors.ERROR, $"{AppConstants.Errors.ERROR_OCCURRED}: {ex.Message}", AppConstants.Messages.OK);
            }
        }

        // Този метод ще се извиква от CombineImages след като получи резултата от TemporaryGallery
        public async Task SelectImageFromGalleryAsync(Func<Task<string>> gallerySelector, Action<ImageSource, string> updateAction)
        {
            try
            {
                string selectedImageName = await gallerySelector();
                if (!string.IsNullOrEmpty(selectedImageName))
                {
                    // Приемаме, че галерията връща име, което може да се използва като ImageSource директно
                    // и пътят се конструира по определен начин
                    string imagePath = Path.Combine("Gallery", $"{selectedImageName}.jpg"); // Примерна логика за път
                    updateAction(ImageSource.FromFile(selectedImageName), imagePath); // Актуализираме UI и пътя
                }
            }
            catch (Exception ex)
            {
                await _displayAlert(AppConstants.Errors.ERROR, $"{"AppConstants.Errors.ERROR_PROCESSING_GALLERY_SELECTION"}: {ex.Message}", AppConstants.Messages.OK);
            }
        }

        public async Task ProcessCapturedImageAsync(Stream imageStream, Action<ImageSource, string> updateAction)
        {
            try
            {
                var tempPath = await PrepareImageForUpload(imageStream);
                if (!string.IsNullOrEmpty(tempPath))
                {
                    updateAction(ImageSource.FromFile(tempPath), tempPath);
                }
            }
            catch (Exception ex)
            {
                await _displayAlert(AppConstants.Errors.ERROR, $"{"AppConstants.Errors.ERROR_PROCESSING_CAPTURED_IMAGE"}: {ex.Message}", AppConstants.Messages.OK);
            }
        }

        // Преоразмерява и записва във временен файл, връща пътя
        public async Task<string> PrepareImageForUpload(Stream imageStream)
        {
            DeleteTemporaryImages(); // Изтриваме стари временни файлове
            if (!imageStream.CanRead) return string.Empty;

            try
            {
                // Преоразмеряване (логиката е изнесена в ImageStreamResize)
                var resizedImageResult = await ImageStreamResize.ResizeImageStream(imageStream, 500, 700);
                //if (!resizedImageResult.Success || resizedImageResult.ResizedStream == null)
                if (resizedImageResult.ResizedStream == null)
                {
                    await _displayAlert(AppConstants.Errors.ERROR," AppConstants.Errors.ERROR_RESIZING_IMAGE", AppConstants.Messages.OK);
                    return string.Empty;
                }

                using var resizedStream = resizedImageResult.ResizedStream;
                if (resizedStream.CanSeek) resizedStream.Position = 0;

                string tempPath = Path.Combine(FileSystem.CacheDirectory, $"processedImage_{DateTime.Now.Ticks}.png");

                using (var fileStream = File.Create(tempPath))
                {
                    await resizedStream.CopyToAsync(fileStream);
                }
                return tempPath;
            }
            catch (Exception ex)
            {
                await _displayAlert(AppConstants.Errors.ERROR, $"{"AppConstants.Errors.ERROR_PREPARING_IMAGE"}: {ex.Message}", AppConstants.Messages.OK);
                return string.Empty;
            }
        }


        public void DeleteTemporaryImages()
        {
            try
            {
                string cacheDir = FileSystem.CacheDirectory;
                var oldFiles = Directory.GetFiles(cacheDir, "processedImage_*.png"); // Обновяваме шаблона
                foreach (var oldFile in oldFiles)
                {
                    try
                    {
                        File.Delete(oldFile);
                    }
                    catch (IOException ex) // По-специфично изключение
                    {
                        // Логване на грешката е по-добре от DisplayAlert тук
                        System.Diagnostics.Debug.WriteLine($"{AppConstants.Errors.ERROR_DELETE_FILE} {oldFile}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex) // За грешки при достъп до директорията
            {
                System.Diagnostics.Debug.WriteLine($"{"AppConstants.Errors.ERROR_ACCESSING_CACHE_DIR"}: {ex.Message}");
            }
        }
    }
}