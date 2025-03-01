using FashionApp.Data.Constants;

namespace FashionApp.core.services
{
    internal class CheckForAndroidPermissions
    {
        public async void Check()
        {
            // Проверка за permissions
            var testPermission = PermissionStatus.Unknown;
            testPermission = await Permissions.CheckStatusAsync<Permissions.Photos>();
            //if(testPermission == PermissionStatus.Granted) return;
            if (Permissions.ShouldShowRationale<Permissions.Photos>())
            {
                await App.Current.MainPage.DisplayAlert(AppConstants.Errors.ERROR, AppConstants.Errors.STORAGE_PERMISSION_REQUIRED, AppConstants.Messages.OK);
            }
            testPermission = await Permissions.RequestAsync<Permissions.Photos>();
            if (testPermission != PermissionStatus.Granted)
            {
                await App.Current.MainPage.DisplayAlert(AppConstants.Errors.ERROR, AppConstants.Errors.STORAGE_PERMISSION_REQUIRED, AppConstants.Messages.OK);
                return;
            }

            var test2 = await Permissions.RequestAsync<Permissions.Media>();

            var status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.StorageRead>();
                if (status != PermissionStatus.Granted)
                {
                    await Application.Current.MainPage.DisplayAlert(AppConstants.Errors.ERROR, AppConstants.Errors.STORAGE_PERMISSION_REQUIRED, AppConstants.Messages.OK);
                    return;
                }
            }
        }

        public async void CheckStorage()
        {
            var testPermission = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            if (Permissions.ShouldShowRationale<Permissions.StorageRead>())
            {
                await App.Current.MainPage.DisplayAlert(AppConstants.Errors.ERROR, AppConstants.Errors.STORAGE_PERMISSION_REQUIRED, AppConstants.Messages.OK);
            }
            testPermission = await Permissions.RequestAsync<Permissions.StorageRead>();
            if (testPermission != PermissionStatus.Granted)
            {
                await Application.Current.MainPage.DisplayAlert(AppConstants.Errors.ERROR, AppConstants.Errors.STORAGE_PERMISSION_REQUIRED, AppConstants.Messages.OK);
                return;
            }
        }

        public async Task<bool> CheckAndRequestStoragePermission()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            if (status != PermissionStatus.Granted)
            {
                // Искане на разрешение
                status = await Permissions.RequestAsync<Permissions.StorageRead>();
            }

            return status == PermissionStatus.Granted;
        }
    }
}
