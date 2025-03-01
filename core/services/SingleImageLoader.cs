using FashionApp.Data.Constants;

namespace FashionApp.core.services
{
    internal class SingleImageLoader
    {
        private readonly Action<string> setErrorMessage;
        private readonly Action<string> setImage;

        public SingleImageLoader(Action<string> setErrorMessage, Action<string> setImage)
        {
            this.setErrorMessage = setErrorMessage;
            this.setImage = setImage;
        }

        public async Task LoadSingleImageAsync(string fileNameFullPath)
        {
            try
            {
#if WINDOWS
            // Използваме същата директория, както в ImageLoader.cs за Windows
            var imageDirectory = @"C:\Users\redfo\Downloads\AI Girls\AI Daenerys Targaryen";
            var filePath = Path.Combine(imageDirectory, fileNameFullPath);

            if (File.Exists(fileNameFullPath)) setImage($"file://{filePath}");
            else setErrorMessage($"{AppConstants.Errors.ERROR}: {AppConstants.Errors.FILE_NOT_FOUND}");

#elif __ANDROID__
                if (OperatingSystem.IsAndroidVersionAtLeast(29))
                {
                    // Използване на MediaStore заявка за Android модерни версии
                    string[] projection = {
                    Android.Provider.IBaseColumns.Id,
                    Android.Provider.MediaStore.IMediaColumns.Data
                };
                    string filePath = fileNameFullPath;
                    string selection = $"{Android.Provider.MediaStore.IMediaColumns.Data} = ?";
                    string[] selectionArgs = new[] { filePath };
                    string? sortOrder = null;

                    using var cursor = Android.App.Application.Context.ContentResolver.Query(
                        Android.Provider.MediaStore.Images.Media.ExternalContentUri,
                        projection,
                        selection,
                        selectionArgs,
                        sortOrder);

                    if (cursor == null)
                    {
                        setErrorMessage($"{AppConstants.Errors.ERROR}: {AppConstants.Errors.CURSOR_ERROR}");
                        return;
                    }
                    if (cursor.MoveToFirst())
                    {
                        int idColumn = cursor.GetColumnIndex(Android.Provider.IBaseColumns.Id);
                        string? id = cursor.GetString(idColumn);
                        Android.Net.Uri? contentUri = Android.Net.Uri.WithAppendedPath(
                            Android.Provider.MediaStore.Images.Media.ExternalContentUri,
                            id);
                        setImage(contentUri.ToString());
                    }
                    else
                    {
                        setErrorMessage($"{AppConstants.Errors.ERROR}: {AppConstants.Errors.FILE_NOT_FOUND}.");
                    }
                }
                else
                {
                    // За Android стари версии (legacy) използваме директен достъп до файловата система
                    string directory = $"{AppConstants.Parameters.APP_BASE_PATH}/{AppConstants.Parameters.APP_NAME}/{AppConstants.Parameters.APP_FOLDER_MASK}%";
                    var filePath = Path.Combine(directory, fileNameFullPath);
                    if (File.Exists(filePath))
                    {
                        setImage($"file://{filePath}");
                    }
                    else
                    {
                        setErrorMessage($"{AppConstants.Errors.ERROR}: {AppConstants.Errors.FILE_NOT_FOUND}.");
                    }
                }
#endif
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                await App.Current.MainPage.DisplayAlert(
                    AppConstants.Errors.ERROR, 
                    $"{AppConstants.Errors.ERROR_OCCURRED}: {ex.Message}\nStack: {ex.StackTrace}",
                    AppConstants.Messages.OK);
            }
        }
    }
}
