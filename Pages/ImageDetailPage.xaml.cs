using FashionApp.core.services;
using FashionApp.Data.Constants;

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
        ImageName.Text = Path.GetFileName(GetRealPathFromUriService.Get(Android.App.Application.Context.ContentResolver, _imageUri));
#endif
    }

    private async void CloseButton_Clicked(object sender, EventArgs e) => await Navigation.PopModalAsync();

    private async void DeleteButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            bool conifirmation = await Application.Current.MainPage.DisplayAlert(
                AppConstants.Messages.DELETE_CONFIRMATION,
                AppConstants.Messages.MESSAGE_FOR_DELETE,
                AppConstants.Messages.YES, AppConstants.Messages.CANCEL);

            if (!conifirmation) return;

            var permissionResult = App.Current?.Handler.MauiContext?.Services.GetService<CheckForAndroidPermissions>();


#if __ANDROID__
            if(!await permissionResult.CheckAndRequestStoragePermission())
            {
                await DisplayAlert(AppConstants.Errors.ERROR, AppConstants.Errors.STORAGE_PERMISSION_REQUIRED, AppConstants.Messages.OK);
                return;
            }

            string realPath = GetRealPathFromUriService.Get(Android.App.Application.Context.ContentResolver, _imageUri);

            if (realPath == null || !File.Exists(realPath))  await DisplayAlert(AppConstants.Errors.ERROR, AppConstants.Errors.FILE_NOT_FOUND, AppConstants.Messages.OK);

            bool isDeleted = DeleteImageFromAndroid.DeleteAndroidModernImage(realPath);

            if(isDeleted)
            {
                await DisplayAlert(AppConstants.Messages.SUCCESS, AppConstants.Messages.DELETE_PHOTO_SUCCESS, AppConstants.Messages.OK);
                await Navigation.PopModalAsync();
            }
#endif
        }
        catch (Exception ex)
        {
            await DisplayAlert(AppConstants.Errors.ERROR, $"{AppConstants.Errors.FAILED_DELETE_IMAGE}: {ex.Message}", AppConstants.Messages.OK);
        }
    }
}