using FashionApp.Pages;

namespace FashionApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                // Запишете лог за e.ExceptionObject
                var ex = e.ExceptionObject as Exception;
                LogException("AppDomain Unhandled Exception", ex);
                // Допълнително действие, ако е необходимо (например, изпращане на лог към сървър)
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                LogException("TaskScheduler Unobserved Exception", e.Exception);
                e.SetObserved(); // Предотвратява прекратяването на приложението
            };

            // Регистриране на други случки, ако е необходимо       

            //MainPage = new AppShell();
            MainPage = new NavigationPage(new LoginPage());
        }

        private void LogException(string source, Exception ex)
        {
            // Пример – записване във файл, конзолата или изпращане към сървър
            System.Diagnostics.Debug.WriteLine($"[{source}] {ex?.Message}\n{ex?.StackTrace}");
            // Можете да добавите допълнителна логика за съхранение/изпращане на грешката
        }

    }
}
