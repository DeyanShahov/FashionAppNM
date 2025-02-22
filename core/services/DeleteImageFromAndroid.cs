using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#if __ANDROID__
using Android.Content;
using Android.Provider;
#endif

namespace FashionApp.core.services
{
    internal static class DeleteImageFromAndroid
    {
#if ANDROID
        public static async Task Delete(ContentResolver resolver, string imageFileName, string directoryPath)
        {
            string[] projection = { IBaseColumns.Id, MediaStore.IMediaColumns.DisplayName };
            Android.Net.Uri collection = MediaStore.Images.Media.ExternalContentUri;
            
            // Добавяне на условие за търсене по fileName
            string selection = $"{MediaStore.IMediaColumns.DisplayName} = ?";
            string[] selectionArgs = { imageFileName };
            
            var cursor = resolver.Query(collection, projection, selection, selectionArgs, null);
            if (cursor != null && cursor.MoveToFirst())
            {
                int idColumn = cursor.GetColumnIndex(IBaseColumns.Id);
                long imageId = cursor.GetLong(idColumn); // Уникален ID на изображението
                Android.Net.Uri deleteUri = ContentUris.WithAppendedId(MediaStore.Images.Media.ExternalContentUri, imageId);
                int deletedRows = resolver.Delete(deleteUri, null, null);
                if (deletedRows > 0)
                    await Application.Current.MainPage.DisplayAlert("Success", $"{imageFileName} delete successful.", "OK");
                else
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to delete image. It might not be owned by the app.", "OK");
            
            }
            cursor?.Close();

            //if (OperatingSystem.IsAndroidVersionAtLeast(29))
            //{
               
            //}
            //else
            //{
            //    // Android 9 или по-старо - използвай директно File API
            //    string filePath = Path.Combine(Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures)?.AbsolutePath ?? "", directoryPath, imageFileName);
            //    Java.IO.File file = new Java.IO.File(filePath);
        
            //    if (file.Exists())
            //    {
            //        bool deleted = file.Delete();
            //        Console.WriteLine(deleted ? $"Файлът {imageFileName} беше изтрит." : $"Неуспешно изтриване на {imageFileName}.");
            //    }
            //}
        }
#endif
    }
}
