using Microsoft.Maui.Controls.PlatformConfiguration;
using System.Text.RegularExpressions;
#if __ANDROID__
using Android.Content;
using Android.Database;
using Android.Net;
using Android.Provider;
#endif

namespace FashionApp.Pages;

public partial class ImageDetailPage : ContentPage
{
    private readonly string _imageUri;
    public ImageDetailPage(string imageUri)
	{
        InitializeComponent();
        _imageUri = imageUri;
        // �������� �������� �� �������������
        DetailImage.Source = ImageSource.FromUri(new System.Uri(_imageUri));
#if ANDROID
        ImageName.Text = Path.GetFileName(GetRealPathFromUri(Android.App.Application.Context.ContentResolver, _imageUri));
#endif
    }

    private async void CloseButton_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private async void DeleteButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            bool conifirmation = await Application.Current.MainPage.DisplayAlert("Delete Confirmation", "Are you sure you want to delete?", "Yes", "Cancel");

            if (conifirmation)
            {

                //��� ��� ������� ������������ �� ��������� .. ���� ���� �� �� �������
            }
#if __ANDROID__
                // �������� �� ����������
                if (!await CheckAndRequestStoragePermission())
                {
                    await DisplayAlert("Error", "Storage permission is required to delete the image.", "OK");
                    return;
                }

                string realPath = GetRealPathFromUri(Android.App.Application.Context.ContentResolver, _imageUri);

                LoadAndroidModernImages(realPath);

                //if (realPath != null && File.Exists(realPath))
                //{
                //    File.Delete(realPath);
                //    await DisplayAlert("Deleted", "Image deleted successfully.", "OK");
                //    await Navigation.PopModalAsync();
                //}
                //else
                //{
                //    await DisplayAlert("Error", "File does not exist.", "OK");
                //}
#endif

            // ����������� ���� _imageUri � ������� ���� (� ������� "file://")
            //if (_imageUri.StartsWith("file://"))
            //{
            //    string filePath = _imageUri.Substring("file://".Length);
            //    if (File.Exists(filePath))
            //    {
            //        File.Delete(filePath);
            //        await DisplayAlert("Deleted", "Image deleted successfully.", "OK");
            //        await Navigation.PopModalAsync();
            //    }
            //    else
            //    {
            //        await DisplayAlert("Error", "File does not exist.", "OK");
            //    }
            //}
            //else
            //{
            //    await DisplayAlert("Error", "Cannot delete remote images.", "OK");
            //}
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to delete image: {ex.Message}", "OK");
        }
    }

    public static string ConvertContentUriToFileUri(string contentUri)
    {
        // ��������� ����� �� ������ �� "content://" � "file://"
        string pattern = @"^content://";
        string replacement = "file://";

        // ���������� �� ��������
        string fileUri = Regex.Replace(contentUri, pattern, replacement);

        return fileUri;
    }

#if __ANDROID__
        public string GetRealPathFromUri(ContentResolver contentResolver, string imageUri)
        {
            // �������� ���� URI-�� ������� � "content://"
            if (imageUri.StartsWith("content://"))
            {
                var uri = Android.Net.Uri.Parse(imageUri);
                string filePath = null;

                // ��������� �� ���� �� ����� �� MediaStore
                using (var cursor = contentResolver.Query(uri, null, null, null, null))
                {
                    if (cursor != null && cursor.MoveToFirst())
                    {
                        int columnIndex = cursor.GetColumnIndex(MediaStore.MediaColumns.Data);
                        filePath = cursor.GetString(columnIndex);
                    }
                }

                // �������� ���� ������ ����������
                if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                {
                    return filePath; // ����� ������ ��� �� �����
                }
            }

            return null; // ����� null, ��� �� � ������� ���
        }
#endif

    private async Task<bool> CheckAndRequestStoragePermission()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
        if (status != PermissionStatus.Granted)
        {
            // ������ �� ����������
            status = await Permissions.RequestAsync<Permissions.StorageRead>();
        }

        return status == PermissionStatus.Granted;
    }

#if __ANDROID__
        private void LoadAndroidModernImages(string pathToFile)
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

                using var cursor = Android.App.Application.Context.ContentResolver.Query(
                    Android.Provider.MediaStore.Images.Media.ExternalContentUri,
                    projection,
                    selection,
                    selectionArgs,
                    sortOrder);

                if (cursor == null)
                {
                    //setErrorMessage("Error: Cursor is null. Query failed.");
                    return;
                }
                if (cursor.MoveToFirst())
                {
                    int idColumn = cursor.GetColumnIndex(Android.Provider.IBaseColumns.Id);
                    string id = cursor.GetString(idColumn);
                    Android.Net.Uri contentUri = Android.Net.Uri.WithAppendedPath(
                        Android.Provider.MediaStore.Images.Media.ExternalContentUri,
                        id);

                    // ��������� �� ������������� �� MediaStore
                    Android.App.Application.Context.ContentResolver.Delete(contentUri, null, null);
                }
                else
                {
                    //setErrorMessage($"Error: File not found in MediaStore.");
                }
            }
            else
            {
                // �� Android ����� ������ (legacy) ���������� �������� ������ �� ��������� �������
                string directory = "/storage/emulated/0/Pictures/FashionApp/MasksImages";
                var filePath = pathToFile; //Path.Combine(directory, fileName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                else
                {
                    //setErrorMessage($"Error: File not found in directory: {directory}");
                }
            }
        }
#endif
}