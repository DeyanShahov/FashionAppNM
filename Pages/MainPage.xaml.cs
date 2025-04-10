using FashionApp.core.services;
using FashionApp.Data.Constants;

namespace FashionApp.Pages
{
    public partial class MainPage : ContentPage
    {
        //private readonly HttpClient _client = new HttpClient { Timeout = TimeSpan.FromSeconds(300) };
        //private const string ApiUrl = "https://eminently-verified-walleye.ngrok-free.app";
        //private const string ApiUrl = "http://192.168.0.101:80";

        //private readonly ExecutionGuardService _executionGuardService;


        //public MainPage(ExecutionGuardService executionGuard)
        public MainPage()
        {
            InitializeComponent();
            //_executionGuardService = executionGuard;

            BindingContext = this;
            
            SetContentLabel();
        }


        private void SetContentLabel()
        {

#if ANDROID
            // Проверка и отпечатване на екрана на текущата Андроид версия и съответната API версия
            ContentLabel.Text = $"{AppConstants.Messages.CREATED_BY}\n Android: {Android.OS.Build.VERSION.Release}  API: {(int)(int)Android.OS.Build.VERSION.SdkInt}";
#endif
        }

        private async void OnEditorButtonClicked(object sender, EventArgs e) 
        {
            bool pageAlreadyExists = Navigation.NavigationStack.Any(p => p is MaskEditor);
            if (pageAlreadyExists) return;

            //string taskKey = AppConstants.Pages.EDITOR;
            //if (!_executionGuardService.TryStartTask(taskKey)) return;

            try
            {
                var page = MauiProgram.ServiceProvider.GetRequiredService<MaskEditor>();
                await Navigation.PushAsync(page);
            }
            finally
            {
                //_executionGuardService.FinishTask(taskKey);
            }         
        }

        private async void PartnersPageButton_Clicked(object sender, EventArgs e)
        {
            bool pageAlreadyExists = Navigation.NavigationStack.Any(p => p is PartnersPage);
            if (pageAlreadyExists) return;

            //string taskKey = AppConstants.Pages.PARTNERS;
            //if (!_executionGuardService.TryStartTask(taskKey)) return;

            try
            {
                var page = MauiProgram.ServiceProvider.GetRequiredService<PartnersPage>();
                await Navigation.PushAsync(page);
            }
            finally
            {
                //_executionGuardService.FinishTask(taskKey);
            }
        }

        private async void OnNavigateClickedToWeb(object sender, EventArgs e)
        {
            bool pageAlreadyExists = Navigation.NavigationStack.Any(p => p is WebViewPage);
            if (pageAlreadyExists) return;

            //string taskKey = AppConstants.Pages.WEB_VIEW;
            //if (!_executionGuardService.TryStartTask(taskKey)) return;

            try
            {
                var page = MauiProgram.ServiceProvider.GetRequiredService<WebViewPage>();
                await Navigation.PushAsync(page);
            }
            finally
            {
                //_executionGuardService.FinishTask(taskKey);
            }
        }

        private async void GalleryButton_Clicked(object sender, EventArgs e)
        {
            bool pageAlreadyExists = Navigation.NavigationStack.Any(p => p is BaseGallery);
            if (pageAlreadyExists) return;

            //string taskKey = AppConstants.Pages.RESULTS_GALLERY;
            //if (!_executionGuardService.TryStartTask(taskKey)) return;

            try
            {         
                var page = MauiProgram.ServiceProvider.GetRequiredService<BaseGallery>();
                page.ImagesPath = $"{AppConstants.Parameters.APP_BASE_PATH}/{AppConstants.Parameters.APP_NAME}/{AppConstants.Parameters.APP_FOLDER_IMAGES}%";
                page.Title = AppConstants.Parameters.APP_FOLDER_IMAGES;               
                await Navigation.PushAsync(page);
            }
            finally
            {
               // _executionGuardService.FinishTask(taskKey);
            }
        }

        private async void MaskGalleryButton_Clicked(object sender, EventArgs e)
        {
            bool pageAlreadyExists = Navigation.NavigationStack.Any(p => p is BaseGallery);
            if (pageAlreadyExists) return;

            //string taskKey = AppConstants.Pages.MASKS_GALLERY;
            //if (!_executionGuardService.TryStartTask(taskKey)) return;

            try
            {               
                var page = MauiProgram.ServiceProvider.GetRequiredService<BaseGallery>();
                page.ImagesPath = $"{AppConstants.Parameters.APP_BASE_PATH}/{AppConstants.Parameters.APP_NAME}/{AppConstants.Parameters.APP_FOLDER_MASK}%";
                page.Title = AppConstants.Parameters.APP_FOLDER_MASK;
                await Navigation.PushAsync(page);
            }
            finally
            {
                //_executionGuardService.FinishTask(taskKey);
            }
        }

        private async void WebGalleryButton_Clicked(object sender, EventArgs e)
        {
            bool pageAlreadyExists = Navigation.NavigationStack.Any(p => p is BaseGallery);
            if (pageAlreadyExists) return;

            //string taskKey = AppConstants.Pages.WEB_GALLERY;
            //if (!_executionGuardService.TryStartTask(taskKey)) return;
            try
            {         
                var page = MauiProgram.ServiceProvider.GetRequiredService<BaseGallery>();
                page.ImagesPath = $"{AppConstants.Parameters.APP_BASE_PATH}/{AppConstants.Parameters.APP_NAME}/{AppConstants.Parameters.APP_FOLDER_SCREEN}%";
                page.Title = AppConstants.Pages.WEB_GALLERY;
                await Navigation.PushAsync(page);
            }
            finally
            {
                //_executionGuardService.FinishTask(taskKey);
            }
        }
      

        private async void CombineImagesButton_Clicked(object sender, EventArgs e)
        {
            bool pageAlreadyExists = Navigation.NavigationStack.Any(p => p is CombineImages);
            if (pageAlreadyExists) return;

            //string taskKey = AppConstants.Pages.COMBINE_IMAGES;
            //if (!_executionGuardService.TryStartTask(taskKey)) return;

            try
            {
                //await Navigation.PushAsync(new CombineImages(isAdmin));
                var page = MauiProgram.ServiceProvider.GetRequiredService<CombineImages>();
                await Navigation.PushAsync(page);
            }
            catch (Exception ex)
            {
                await DisplayAlert(AppConstants.Errors.ERROR, $"{ex?.Message}\n{ex?.StackTrace}", AppConstants.Messages.OK);
            }
            finally
            {
                //_executionGuardService.FinishTask(taskKey);
            }
        }
    }
}
