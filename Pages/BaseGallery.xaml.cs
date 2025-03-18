using FashionApp.core.services;

#if __ANDROID__
using Android.Content;
using Android.Provider;
#endif

namespace FashionApp.Pages;

public partial class BaseGallery : ContentPage
{
    private bool isInUse = false;
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

    //public double ImageWidth = 120;
    //public double ImageHeight = 120;
    //public bool isBaseVariant = true;
    //public int RequiredTab = 1;

    public BaseGallery(string galleryName, string galleryPath, bool singleImageInclude = true)
	{
		InitializeComponent();

        Title = galleryName; // Сетване името на глаерията 
        imagesPath = galleryPath; // Сетване пътя до галерията
        //isBaseVariant = singleImageInclude;

        BindingContext = this;

        //LargeImageFrame.IsVisible = singleImageInclude; // Сетване дали да се вижда единична голяма снимка в зависимост мода
        //GridItemList.Span = singleImageInclude ? 3 : 2;
        //ImageWidth = singleImageInclude ? 120 : 250;
        //ImageHeight = singleImageInclude ? 120 : 300;
        //RequiredTab = singleImageInclude ? 1 : 2;

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

    private async void LoadAllImagesForGallery() => await imagesLoader.LoadImagesAsync(imagesPath);

#if ANDROID
    private async void LoadLargeImage()
    {
        // Проверка дали списъкът с изображения не е празен
        if (ImagesList.Count == 0) return;

        // Извличане на пътя на първото изображение в списъка
        var imagePath = GetRealPathFromUri(Android.App.Application.Context.ContentResolver, ImagesList.FirstOrDefault());

        // Зареждане на изображението асинхронно
        await singleImageLoader.LoadSingleImageAsync(imagePath);
        selectedImageUri = ImagesList.FirstOrDefault();
    }
#endif


    private async void SelectedBodyImage_Tapped(object sender, TappedEventArgs e)
    {
        if (isInUse) return;

        isInUse = true;

        var page = new ImageDetailPage(selectedImageUri);
        page.Disappearing += Page_Disappearing;
        if (selectedImageUri != null) await Navigation.PushModalAsync(page);

    }

    private void Page_Disappearing(object sender, EventArgs e)
    {
        isInUse = false; // Reset isInUse when the page disappears
        ((ImageDetailPage)sender).Disappearing -= Page_Disappearing; // Unsubscribe
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
                    int columnIndex = cursor.GetColumnIndex(MediaStore.IMediaColumns.Data);
                    filePath = cursor.GetString(columnIndex);
                }
            }

            // Проверка дали файлът съществува
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath)) return filePath;
        }

        return null; // Връща null, ако не е намерен път
    }
#endif
}