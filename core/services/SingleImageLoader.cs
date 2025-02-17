using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
#if WINDOWS
            // Използваме същата директория, както в ImageLoader.cs за Windows
            var imageDirectory = @"C:\Users\redfo\Downloads\AI Girls\AI Daenerys Targaryen";
            var filePath = Path.Combine(imageDirectory, fileNameFullPath);
            //if (File.Exists(filePath))
            if (File.Exists(fileNameFullPath))
            {
                setImage($"file://{filePath}");
            }
            else
            {
                setErrorMessage($"Error: File {fileNameFullPath} not found in directory: {imageDirectory}");
            }
#elif __ANDROID__
            if (OperatingSystem.IsAndroidVersionAtLeast(29))
            {
                // Използване на MediaStore заявка за Android модерни версии
                string[] projection = {
                    Android.Provider.IBaseColumns.Id,
                    Android.Provider.MediaStore.IMediaColumns.Data
                };
                // Пълният път на файла
                //string filePath = $"/storage/emulated/0/Pictures/FashionApp/MasksImages/{fileName}";
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
                    setErrorMessage("Error: Cursor is null. Query failed.");
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
                    setErrorMessage($"Error: File {fileNameFullPath} not found in MediaStore.");
                }
            }
            else
            {
                // За Android стари версии (legacy) използваме директен достъп до файловата система
                string directory = "/storage/emulated/0/Pictures/FashionApp/MasksImages";
                var filePath = Path.Combine(directory, fileNameFullPath);
                if (File.Exists(filePath))
                {
                    setImage($"file://{filePath}");
                }
                else
                {
                    setErrorMessage($"Error: File {fileNameFullPath} not found in directory: {directory}");
                }
            }
#endif
            await Task.CompletedTask;
        }

    }
}
