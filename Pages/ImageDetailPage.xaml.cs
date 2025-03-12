using FashionApp.core.services;
using FashionApp.Data.Constants;
using Microsoft.Maui.Storage;
using Microsoft.Maui.ApplicationModel;
using Newtonsoft.Json;

namespace FashionApp.Pages;

public partial class ImageDetailPage : ContentPage
{
    private readonly string _imageUri;
    private readonly string fileName;
    public ImageDetailPage(string imageUri)
	{
        InitializeComponent();
        _imageUri = imageUri;
        // �������� �������� �� �������������
        DetailImage.Source = ImageSource.FromUri(new System.Uri(_imageUri));
#if ANDROID
        fileName = Path.GetFileName(GetRealPathFromUriService.Get(Android.App.Application.Context.ContentResolver, _imageUri));
        ImageName.Text = fileName;
        if (HasImageSource(fileName)) Source.IsVisible = true;
#endif
    }

    private async void CloseButton_Clicked(object sender, EventArgs e) => await Navigation.PopModalAsync();

    private async void SourceButton_Clicked(object sender, EventArgs e)
    {
        string jsonData = Preferences.Get("screenshot_urls", string.Empty);
        if (string.IsNullOrEmpty(jsonData))
        {
            await DisplayAlert("Информация", "Няма запазени URL адреси.", "OK");
            return;
        }

        // Десериализирай JSON-а в речник
        Dictionary<string, string> urlMapping = null;
        try
        {
            urlMapping = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonData);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Грешка", $"Неуспешно зареждане на данните: {ex.Message}", "OK");
            return;
        }

        // Провери дали има запис с ключ равен на името на избраната снимка
        if (urlMapping != null && urlMapping.TryGetValue(fileName, out string url))
        {
            // Опитай да отвориш URL-а в браузъра
            try
            {
                //await Launcher.OpenAsync(new Uri(url));
                await CustomWebPage(url);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Грешка", $"Не може да се отвори URL: {ex.Message}", "OK");
            }
        }
        else
        {
            await DisplayAlert("Информация", "За избраната снимка няма зададен URL.", "OK");
        }
    }

    private async void DeleteButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            bool confirmation = await Application.Current.MainPage.DisplayAlert(
                AppConstants.Messages.DELETE_CONFIRMATION,
                AppConstants.Messages.MESSAGE_FOR_DELETE,
                AppConstants.Messages.YES, AppConstants.Messages.CANCEL);

            if (!confirmation) return;

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
            DeleteImageMapping(fileName);
        }
        catch (Exception ex)
        {
            await DisplayAlert(AppConstants.Errors.ERROR, $"{AppConstants.Errors.FAILED_DELETE_IMAGE}: {ex.Message}", AppConstants.Messages.OK);
        }
    }


    // Метод, който проверява дали за даден файл съществува запис в Preferences
    private bool HasImageSource(string fileName)
    {
        // Вземане на запазените данни (JSON низ)
        string jsonData = Preferences.Get("screenshot_urls", string.Empty);

        if (string.IsNullOrEmpty(jsonData))
        {
            // Няма запазени данни
            return false;
        }

        try
        {
            // Десериализиране на JSON-а в речник
            var urlMapping = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonData);

            // Ако речникът съдържа ключа и стойността не е празна
            if (urlMapping != null && urlMapping.TryGetValue(fileName, out string url))
            {
                return !string.IsNullOrEmpty(url);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Грешка при десериализация на данните: {ex.Message}");
        }

        return false;
    }

    private void DeleteImageMapping(string fileName)
    {
        // Зареждане на JSON данните от Preferences
        string jsonData = Preferences.Get("screenshot_urls", string.Empty);
        if (string.IsNullOrEmpty(jsonData))
        {
            // Няма записани данни – нищо за изтриване
            return;
        }

        try
        {
            // Десериализиране на JSON-а в речник
            var urlMapping = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonData);
            if (urlMapping != null && urlMapping.ContainsKey(fileName))
            {
                // Премахване на записа за дадената снимка
                urlMapping.Remove(fileName);

                // Сериализиране на обновения речник и запис във Preferences
                string newJsonData = JsonConvert.SerializeObject(urlMapping);
                Preferences.Set("screenshot_urls", newJsonData);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Грешка при изтриването на записа: {ex.Message}");
        }
    }

    //------------------------------------------------ METHODS ----------------------------------------------
    private async Task CustomWebPage(string url)
    {
        try
        {
            await Navigation.PushModalAsync(new WebViewPage(true, url));
        }
        catch (Exception ex)
        {
            CustomErrorMessage(ex.Message);
        }
    }
    private async void CustomErrorMessage(string message)
    {
        await DisplayAlert(AppConstants.Errors.ERROR, message, AppConstants.Messages.OK);
    }
}