using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FashionApp.core.services
{
    internal class ImagesLoader
    {
        private readonly Action<string> setErrorMessage;
        private readonly Action<bool> setBusy;
        private readonly Action<List<string>> setImagesList;

        public ImagesLoader(Action<string> setErrorMessage, Action<bool> setBusy, Action<List<string>> setImagesList)
        {
            this.setErrorMessage = setErrorMessage;
            this.setBusy = setBusy;
            this.setImagesList = setImagesList;
        }

        public async Task LoadImagesAsync(string imagesFolderPath)
        {
            setBusy(true);
            try
            {
#if WINDOWS
                await WindowsLoadImagesFromAssetsAsync();
#elif __ANDROID__
                await AndroidLoadImagesFromAssetsAsync(imagesFolderPath);
#endif
            }
            catch (Exception ex)
            {
                setErrorMessage($"Error: {ex.Message}");
            }
            finally
            {
                setBusy(false);
            }
        }


#if WINDOWS
        private async Task WindowsLoadImagesFromAssetsAsync()
        {
            var imageDirectory = @"C:\Users\redfo\Downloads\AI Girls\AI Daenerys Targaryen";

            if (!Directory.Exists(imageDirectory))
            {
                setErrorMessage($"Error: Directory not found: {imageDirectory}");
                return;
            }

            var supportedExtensions = new HashSet<string> { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            var images = Directory.EnumerateFiles(imageDirectory)
                .Where(f => supportedExtensions.Contains(Path.GetExtension(f).ToLower()))
                .Select(f => $"file://{f}")
                .ToList();

            setImagesList(images);
        }
#elif __ANDROID__
        private async Task AndroidLoadImagesFromAssetsAsync(string imagesFolderPath)
        {
            var testPermission = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            if (Permissions.ShouldShowRationale<Permissions.StorageRead>())
            {
                setErrorMessage("Error: Storage permission is required to access storage.");
            }
            testPermission = await Permissions.RequestAsync<Permissions.StorageRead>();
            if (testPermission != PermissionStatus.Granted)
            {
                setErrorMessage("Error: Storage permission is required to load images.");
                return;
            }
        
            var images = new List<string>();
        
            if (OperatingSystem.IsAndroidVersionAtLeast(29))
            {
                LoadAndroidModernImages(images, imagesFolderPath);
            }
            else
            {
                LoadAndroidLegacyImages(images, imagesFolderPath);
            }
        
            setImagesList(images);      
        }


        private void LoadAndroidModernImages(List<string> images, string imagesFolderPath)
        {
            string[] projection = {
                Android.Provider.IBaseColumns.Id,
                Android.Provider.MediaStore.IMediaColumns.Data,
                Android.Provider.MediaStore.IMediaColumns.RelativePath
            };
        
            
            string selection = $"{Android.Provider.MediaStore.IMediaColumns.Data} LIKE ? AND {Android.Provider.MediaStore.IMediaColumns.MimeType} LIKE ?";
            string[] selectionArgs = new[] { imagesFolderPath, "image/%" };
            string sortOrder = $"{Android.Provider.MediaStore.IMediaColumns.DateAdded} DESC";
        
            using var cursor = Android.App.Application.Context.ContentResolver?.Query(
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
        
            if (!cursor.MoveToFirst())
            {
                setErrorMessage("Info: No images found in the specified directory.");
                return;
            }
        
            int idColumn = cursor.GetColumnIndex(Android.Provider.IBaseColumns.Id);
            int pathColumn = cursor.GetColumnIndex(Android.Provider.MediaStore.IMediaColumns.RelativePath);
        
            do
            {
                string? id = cursor.GetString(idColumn);
                Android.Net.Uri? contentUri = Android.Net.Uri.WithAppendedPath(
                    Android.Provider.MediaStore.Images.Media.ExternalContentUri,
                    id);
                images.Add(contentUri.ToString());
            } while (cursor.MoveToNext());
        }


        private void LoadAndroidLegacyImages(List<string> images, string imagesFolderPath)
        {
            string path = imagesFolderPath;
            if (Directory.Exists(path))
            {
                var files = Directory.EnumerateFiles(path, "*.*")
                    .Where(f => f.EndsWith(".jpg") || f.EndsWith(".jpeg") ||
                               f.EndsWith(".png") || f.EndsWith(".gif"));
        
                foreach (var file in files)
                {
                    images.Add($"file://{file}");
                }
            }
            else
            {
                setErrorMessage("Error: Directory not found.");
            }
        }
#endif
    }
}
