using FashionApp.core.services;
using Microsoft.Maui.Controls.PlatformConfiguration;

#if __ANDROID__
using Android.Content;
using Android.Database;
using Android.Net;
using Android.Provider;
#endif

namespace FashionApp.Pages;

public partial class BaseGallery : ContentPage
{

    private string selectedImageUri;
    private readonly string imagesPath;

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

    private readonly ImagesLoader imagesLoader;
    private readonly SingleImageLoader singleImageLoader;

    public BaseGallery(string galleryName, string galleryPath)
	{
		InitializeComponent();

        Title = galleryName; // ������� ����� �� ��������� 
        imagesPath = galleryPath; // ������� ���� �� ���������

        BindingContext = this;
        imagesLoader = new ImagesLoader(
            setErrorMessage: (msg) => ErrorMessage = msg,
            setBusy: (busy) => IsBusy = busy,
            setImagesList: (images) => ImagesList = images
        );
        singleImageLoader = new SingleImageLoader(
            setErrorMessage: (msg) => ErrorMessage = msg,
            setImage: (uri) => SelectedBodyImage.Source = Microsoft.Maui.Controls.ImageSource.FromUri(new System.Uri(uri))
        );
    }


    protected override void OnAppearing()
    {
        base.OnAppearing();

        LoadAllImagesForGallery();
#if ANDROID
        LoadLargeImage();
#endif
    }

    //private async void LoadAllImagesForGallery() => await imagesLoader.LoadImagesAsync("/storage/emulated/0/Pictures/FashionApp/Images%");
    private async void LoadAllImagesForGallery() => await imagesLoader.LoadImagesAsync(imagesPath);

#if ANDROID
    private async void LoadLargeImage()
    {
        // �������� ���� �������� � ����������� �� � ������
        if (ImagesList.Count == 0) return;

        // ��������� �� ���� �� ������� ����������� � �������
        //var imagePath = Path.GetFileName(GetRealPathFromUri(Android.App.Application.Context.ContentResolver, ImagesList.FirstOrDefault()));
        var imagePath = GetRealPathFromUri(Android.App.Application.Context.ContentResolver, ImagesList.FirstOrDefault());

        // ��������� �� ������������� ����������
        await singleImageLoader.LoadSingleImageAsync(imagePath);
        selectedImageUri = ImagesList.FirstOrDefault();
    }
#endif


    private async void SelectedBodyImage_Tapped(object sender, TappedEventArgs e)
    {
        if (selectedImageUri != null)
        {
            await Navigation.PushModalAsync(new ImageDetailPage(selectedImageUri));
        }
    }


    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        if (sender is Image tappedImage && tappedImage.BindingContext is string imageUri)
        {
#if ANDROID
            var contentResolver = Android.App.Application.Context.ContentResolver;
            if (contentResolver != null)
            {
                selectedImageUri = imageUri;
                var getImageName = GetRealPathFromUri(contentResolver, imageUri);
                await singleImageLoader.LoadSingleImageAsync(getImageName);
            }
#else
            selectedImageUri = imageUri;
            await singleImageLoader.LoadSingleImageAsync(imageUri);
#endif
        }
    }

#if ANDROID
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
                    int columnIndex = cursor.GetColumnIndex(MediaStore.IMediaColumns.Data);
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
}