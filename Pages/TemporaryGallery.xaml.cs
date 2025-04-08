using FashionApp.core.services;

namespace FashionApp.Pages;

public partial class TemporaryGallery : ContentPage
{
    // Това свойство ще служи за връщане на избраното изображение към основната страница.
    public TaskCompletionSource<string> ImageSelectedTask { get; set; } = new TaskCompletionSource<string>();

    public static readonly BindableProperty ImagesListProperty =
            BindableProperty.Create(nameof(ImagesList), typeof(List<string>), typeof(MainPage), new List<string>());

    public List<string> ImagesList
    {
        get => (List<string>)GetValue(ImagesListProperty);
        set => SetValue(ImagesListProperty, value);
    }

    private string ErrorMessage = String.Empty;
    private bool IsReadyToSet = true;


    public TemporaryGallery(bool isInAppGallery = true, string galleryPath = "TestGallery")
	{
		InitializeComponent();

        BindingContext = this;

        if (isInAppGallery) ImagesList = new Gallery(galleryPath).Images;
        else SetSpecificGallery(galleryPath);       
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        IsReadyToSet = true;
    }

    private async void SetSpecificGallery(string galleryPath)
    {
        var imagesLoader = new ImagesLoader(
          setErrorMessage: (msg) => ErrorMessage = msg,
          setBusy: (busy) => IsBusy = busy,
          setImagesList: (images) => ImagesList = images
        );

        await imagesLoader.LoadImagesAsync(galleryPath);
    }


    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        if(!IsReadyToSet) return;

        IsReadyToSet = false;

        // Предполагаме, че BindingContext-а на всяко изображение е неговият път (string).
        if (sender is Image image && image.Source is FileImageSource fileImage)
        {
            string selectedImageName = fileImage.File;
            // Задаваме избора на изображение като резултат
            ImageSelectedTask.TrySetResult(selectedImageName);
        }
        else if (sender is Image img && img.Source != null)
        {
            // Ако използвате друг тип ImageSource, може да използвате ToString() или да промените логиката.
            ImageSelectedTask.TrySetResult(img.Source.ToString());
        }
        // Затваряме модалния прозорец
        await Navigation.PopModalAsync();
    }
}



