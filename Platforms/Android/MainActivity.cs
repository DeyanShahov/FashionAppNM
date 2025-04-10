using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
//using Android.Gms.Ads;
using Android.OS;
using Android.Runtime;
using AndroidX.Core.App;
using AndroidX.Core.Content;

namespace FashionApp.Platforms.Android
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        //const int RequestStorageId = 1;
        public MainActivity()
        {
            AndroidEnvironment.UnhandledExceptionRaiser += (sender, args) =>
            {
                LogException("Android Unhandled Exception", args.Exception);
                // Ако искате, може да зададете args.Handled = true;
            };

        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            //MobileAds.Initialize(this);

            RequestStoragePermission();
        }

        private void LogException(string source, Exception ex)
        {
            // Пример – записване във файл, конзолата или изпращане към сървър
            System.Diagnostics.Debug.WriteLine($"[{source}] {ex?.Message}\n{ex?.StackTrace}");
            // Можете да добавите допълнителна логика за съхранение/изпращане на грешката
        }


        void RequestStoragePermission()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu) // Android 13+
            {
                if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadMediaImages) != Permission.Granted)
                {
                    //ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.ReadMediaImages }, RequestStorageId);
                    ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.ReadMediaImages }, (int)Permission.Granted);
                }
            }
            else if (Build.VERSION.SdkInt >= BuildVersionCodes.Q) // Android 10+
            {
                // Scoped Storage - не трябва допълнителни permissions за файлове, създадени от приложението
            }
            else
            {
                if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadExternalStorage) != Permission.Granted)
                {
                    //ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.ReadExternalStorage }, RequestStorageId);
                    ActivityCompat.RequestPermissions(this, new string[] { Manifest.Permission.ReadExternalStorage }, (int)Permission.Granted);
                }
            }
        }

    }
}
