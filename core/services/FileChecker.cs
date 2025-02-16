namespace FashionApp.core.services
{
    internal class FileChecker : IFileChecker
    {
        public async Task<bool> CheckFileExistsAsync(string fileName)
        {
            // Проверка на разрешение
            var status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            if(status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.StorageRead>();
                if(status != PermissionStatus.Granted) return false;
            }
#if ANDROID
            // Път до Picture директорията
            var picturesDirectory = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures)?.AbsolutePath;
            if(string.IsNullOrWhiteSpace(picturesDirectory)) return false;

            // Път до файла
            var targetPath = Path.Combine(picturesDirectory, "FashionApp", "MasksImages");
            var filePath = Path.Combine(targetPath,fileName);

            return Directory.Exists(targetPath) && File.Exists(filePath);
#elif WINDOWS
            return false;
#endif
            return false;
        }
    }
}
