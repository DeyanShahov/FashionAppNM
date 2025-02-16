using FashionApp.core.services;
using Microsoft.Maui.Controls.PlatformConfiguration;

#if __ANDROID__
using Android.Content;
using Android.Database;
using Android.Net;
using Android.Provider;
#endif

namespace FashionApp.Pages;

public partial class GalleryPage : ContentPage
{
    private string selectedImageUri;

    public static readonly BindableProperty ImagesListProperty =
            BindableProperty.Create(nameof(ImagesList), typeof(List<string>), typeof(MainPage), new List<string>());

    public List<string> ImagesList
    {
        get => (List<string>)GetValue(ImagesListProperty);
        set => SetValue(ImagesListProperty, value);
    }

    private bool isBusy;
    public bool IsBusy
    {
        get => isBusy;
        set
        {
            if (isBusy != value)
            {
                isBusy = value;
                OnPropertyChanged(nameof(IsBusy));
            }
        }
    }


    private string errorMessage = string.Empty;
    public string ErrorMessage
    {
        get => errorMessage;
        set
        {
            if (errorMessage != value)
            {
                errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
                ErrorLabel.Text = value;
                ErrorLabel.IsVisible = !string.IsNullOrEmpty(value);
            }
        }
    }

    private readonly ImagesLoader imageLoader;
    private readonly SingleImageLoader singleImageLoader;

    public GalleryPage()
	{
		InitializeComponent();

        BindingContext = this;
        imageLoader = new ImagesLoader(
            setErrorMessage: (msg) => ErrorMessage = msg,
            setBusy: (busy) => IsBusy = busy,
            setImagesList: (images) => ImagesList = images
        );
        singleImageLoader = new SingleImageLoader(
            setErrorMessage: (msg) => ErrorMessage = msg,
            setImage: (uri) => SelectedBodyImage.Source = Microsoft.Maui.Controls.ImageSource.FromUri(new System.Uri(uri))
        );

        LoadAllImagesForGallery();
#if ANDROID
        LoadLargeImage();
#endif
    }

    private async void LoadAllImagesForGallery() => await imageLoader.LoadImagesAsync("/storage/emulated/0/Pictures/FashionApp/Images%");

#if ANDROID
    private async void LoadLargeImage()
    {
        // Проверка дали списъкът с изображения не е празен
        if (ImagesList.Count == 0) return;
        
        // Извличане на пътя на първото изображение в списъка
        //var imagePath = Path.GetFileName(GetRealPathFromUri(Android.App.Application.Context.ContentResolver, ImagesList.FirstOrDefault()));
        var imagePath = GetRealPathFromUri(Android.App.Application.Context.ContentResolver, ImagesList.FirstOrDefault());
        
        // Зареждане на изображението асинхронно
        await singleImageLoader.LoadSingleImageAsync(imagePath);
    }
#endif


    private async void SelectedBodyImage_Tapped(object sender, TappedEventArgs e)
    {
        if(selectedImageUri != null)
        {
            await Navigation.PushModalAsync(new ImageDetailPage(selectedImageUri));
        }
    }

#if ANDROID
    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        if (sender is Image tappedImage && tappedImage.BindingContext is string imageUri)
        {
            selectedImageUri = imageUri;
            //await Navigation.PushModalAsync(new ImageDetailPage(imageUri));
            var getImageName = GetRealPathFromUri(Android.App.Application.Context.ContentResolver, imageUri);
            await singleImageLoader.LoadSingleImageAsync(getImageName);
        }
    }


    public string GetRealPathFromUri(ContentResolver contentResolver, string imageUri)
    {
        // Проверка дали URI-то започва с "content://"
        if (imageUri.StartsWith("content://"))
        {
            var uri = Android.Net.Uri.Parse(imageUri);
            string filePath = null;

            // Извличане на пътя на файла от MediaStore
            using (var cursor = contentResolver.Query(uri, null, null, null, null))
            {
                if (cursor != null && cursor.MoveToFirst())
                {
                    int columnIndex = cursor.GetColumnIndex(MediaStore.MediaColumns.Data);
                    filePath = cursor.GetString(columnIndex);
                }
            }

            // Проверка дали файлът съществува
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                return filePath; // Връща пълния път до файла
            }
        }

        return null; // Връща null, ако не е намерен път
    }
#endif
}