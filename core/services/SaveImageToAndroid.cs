﻿#if __ANDROID__
using Android.Content;
using Android.Provider;
using FashionApp.Data.Constants;
#endif

namespace FashionApp.core.services
{
    internal static class SaveImageToAndroid
    {
#if ANDROID
        public static async void Save(string imageFileName, Stream stream, string directory)
        {
            var context = Platform.CurrentActivity;
            string directoryPath = Path.Combine(Android.OS.Environment.DirectoryPictures, directory);

            if (OperatingSystem.IsAndroidVersionAtLeast(29))
            {
                ContentResolver resolver = context.ContentResolver;
                ContentValues contentValues = new();

                // Изтриване на предишната Макро снимка ако системата е андроид 11+
                //if (OperatingSystem.IsAndroidVersionAtLeast(30)) await DeleteExistingImageAsync(resolver, imageFileName, directoryPath);
                if (OperatingSystem.IsAndroidVersionAtLeast(30)) await DeleteImageFromAndroid.Delete(resolver, imageFileName, directoryPath);  
               

                // Създаване на нов запис
                contentValues.Put(MediaStore.IMediaColumns.DisplayName, imageFileName);
                contentValues.Put(MediaStore.IMediaColumns.MimeType, "image/png");
                contentValues.Put(MediaStore.IMediaColumns.RelativePath, directoryPath);

                Android.Net.Uri? imageUri = resolver.Insert(MediaStore.Images.Media.ExternalContentUri, contentValues);
                if (imageUri == null)
                {
                    await  Application.Current.MainPage.DisplayAlert(
                        AppConstants.Errors.ERROR, 
                        AppConstants.Errors.FAILED_TO_SAVE_IMAGE, 
                        AppConstants.Messages.OK);
                    return;
                }

                using (var os = resolver.OpenOutputStream(imageUri))
                {
                    if (os != null)
                    {
                        var bitmap = Android.Graphics.BitmapFactory.DecodeStream(stream);
                        bitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, os);
                        os.Flush();
                    }
                }

                await Application.Current.MainPage.DisplayAlert(
                    AppConstants.Messages.SUCCESS,
                    $"{AppConstants.Messages.IMAGE_SAVED_ON} {directoryPath} {AppConstants.Messages.AS} {imageFileName}",
                    AppConstants.Messages.OK);

                imageFileName = string.Empty; // Reset value of paramether
            }
            else
            {
                // Handle older Android versions if needed
                await Application.Current.MainPage.DisplayAlert(AppConstants.Errors.ERROR, "OS is invalid!", AppConstants.Messages.OK);

                //Java.IO.File storagePath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures);
                //string path = System.IO.Path.Combine(storagePath.ToString(), imageFileName);
                //System.IO.File.WriteAllBytes(path, stream.ToArray());
                ////System.IO.File.WriteAllBytes(path, _imageData);
                //var mediaScanIntent = new Android.Content.Intent(Android.Content.Intent.ActionMediaScannerScanFile);
                //mediaScanIntent.SetData(Android.Net.Uri.FromFile(new Java.IO.File(path)));
                //context.SendBroadcast(mediaScanIntent);
            }
        }
#endif
    }
}
