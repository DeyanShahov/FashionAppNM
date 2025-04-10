using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FashionApp.Data.Constants;


#if ANDROID
//using FashionApp.Droid.Services; // Трябва да се създаде или премести SaveImageToAndroid тук
#endif

namespace FashionApp.core.services
{
    public class ImageSavingService : IImageSavingService
    {
        private readonly Func<string, string, string, Task> _displayAlert;

        public ImageSavingService(Func<string, string, string, Task> displayAlert)
        {
            _displayAlert = displayAlert;
        }

        public async Task<bool> SaveImageAsync(byte[] imageData, string desiredFileNameFormat = "image_{0:yyyyMMdd_HHmmss}.png")
        {
            if (imageData == null || imageData.Length == 0) return false;

            try
            {
                var fileName = string.Format(desiredFileNameFormat, DateTime.Now);
                using var stream = new MemoryStream(imageData);
                stream.Position = 0; // Винаги нулираме потока

#if WINDOWS
                // Използваме Pictures библиотеката за по-добър подход под Windows
                string picturesPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                string savePath = Path.Combine(picturesPath, AppConstants.Parameters.APP_NAME); // Папка на приложението
                Directory.CreateDirectory(savePath); // Уверяваме се, че папката съществува
                string fullPath = Path.Combine(savePath, fileName);
                await File.WriteAllBytesAsync(fullPath, imageData);
                await _displayAlert("Success", $"Image saved to {fullPath}", AppConstants.Messages.OK);
                return true;

#elif ANDROID
                // Препоръчително: Преместете SaveImageToAndroid в неутрално пространство
                // или използвайте платформено-специфична имплементация, инжектирана чрез интерфейс.
                // Засега оставяме директното извикване:
                bool saved = await SaveImageToAndroid.Save(fileName, stream, AppConstants.ImagesConstants.IMAGES_CREATED_IMAGES);
                if (saved)
                {
                    // Може би няма нужда от DisplayAlert тук, зависи от Save метода
                    await _displayAlert("Success", $"Image saved to gallery folder: {AppConstants.ImagesConstants.IMAGES_CREATED_IMAGES}", AppConstants.Messages.OK);
                }
                else
                {
                    await _displayAlert(AppConstants.Errors.ERROR, AppConstants.Errors.FAILED_TO_SAVE_IMAGE, AppConstants.Messages.OK);
                }

                return saved; // Връщаме резултата от Save метода

#elif IOS
                // TODO: Имплементирайте запазване за iOS (MediaLibrary или Files)
                await _displayAlert("Not Implemented", "Image saving is not yet implemented for iOS.", AppConstants.Messages.OK);
                return false;
#else
                await _displayAlert("Unsupported Platform", "Image saving is not supported on this platform.", AppConstants.Messages.OK);
                return false;
#endif
            }
            catch (Exception ex)
            {
                await _displayAlert(AppConstants.Errors.ERROR, $"{AppConstants.Errors.FAILED_TO_SAVE_IMAGE}: {ex.Message}", AppConstants.Messages.OK);
                return false;
            }
        }
    }
}
