#if ANDROID
using Android.Provider;
using Android.Content;
#endif

namespace FashionApp.core.services
{
    public static class GetRealPathFromUriService
    {
#if ANDROID
        public static string Get(ContentResolver contentResolver, string imageUri)
        {
            if (imageUri.StartsWith("content://"))
            {
                var uri = Android.Net.Uri.Parse(imageUri);
                string filePath = null;

                using (var cursor = contentResolver.Query(uri, null, null, null, null))
                {
                    if (cursor != null && cursor.MoveToFirst())
                    {
                        int columnIndex = cursor.GetColumnIndex(MediaStore.MediaColumns.Data);
                        filePath = cursor.GetString(columnIndex);
                    }
                }
 
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    return filePath;    
                }
            }
            return null; //  null,     
        }
#endif
    }
}