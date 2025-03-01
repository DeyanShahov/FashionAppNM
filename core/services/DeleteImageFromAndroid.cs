#if __ANDROID__
using Android.Content;
using Android.Provider;
using FashionApp.Data.Constants;
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
                    await Application.Current.MainPage.DisplayAlert(
                        AppConstants.Messages.SUCCESS,
                        $"{imageFileName} {AppConstants.Messages.DELETE_SUCCESS}.",
                        AppConstants.Messages.OK);
                else
                    await Application.Current.MainPage.DisplayAlert(
                        AppConstants.Errors.ERROR, 
                        AppConstants.Errors.FAILED_DELETE_IMAGE,
                        AppConstants.Messages.OK);
            
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

        public static bool DeleteAndroidModernImage(string pathToFile)
        {
            if (OperatingSystem.IsAndroidVersionAtLeast(29))
            {
                // ���������� �� MediaStore ������ �� Android ������� ������
                string[] projection = {
                    Android.Provider.IBaseColumns.Id,
                    Android.Provider.MediaStore.IMediaColumns.Data
                };
                string selection = $"{Android.Provider.MediaStore.IMediaColumns.Data} = ?";
                string[] selectionArgs = new[] { pathToFile };
                string? sortOrder = null;

                using var cursor = Android.App.Application.Context.ContentResolver?.Query(
                    Android.Provider.MediaStore.Images.Media.ExternalContentUri,
                    projection,
                    selection,
                    selectionArgs,
                    sortOrder);

                if (cursor == null)
                {
                    //setErrorMessage("Error: Cursor is null. Query failed.");
                    return false;
                }
                if (cursor.MoveToFirst())
                {
                    int idColumn = cursor.GetColumnIndex(Android.Provider.IBaseColumns.Id);
                    string id = cursor.GetString(idColumn);
                    Android.Net.Uri? contentUri = Android.Net.Uri.WithAppendedPath(
                        Android.Provider.MediaStore.Images.Media.ExternalContentUri,
                        id);

                    // ��������� �� ������������� �� MediaStore
                    var result = Android.App.Application.Context.ContentResolver?.Delete(contentUri, null, null);
                    return result != 0;
                }
                else
                {
                    return false;
                    //setErrorMessage($"Error: File not found in MediaStore.");
                }
            }
            else
            {
                // �� Android ����� ������ (legacy) ���������� �������� ������ �� ��������� �������
                string directory = $"{AppConstants.Parameters.APP_BASE_PATH}/{AppConstants.Parameters.APP_NAME}/{AppConstants.Parameters.APP_FOLDER_MASK}%";
                var filePath = pathToFile; 
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }
                else
                {
                    return false;
                }             
            }
        }
#endif
    }
}
