using FashionApp.core.services;

namespace FashionApp.Pages;

public partial class TemporaryGallery : ContentPage
{
    // ���� �������� �� ����� �� ������� �� ��������� ����������� ��� ��������� ��������.
    public TaskCompletionSource<string> ImageSelectedTask { get; set; } = new TaskCompletionSource<string>();

    public static readonly BindableProperty ImagesListProperty =
            BindableProperty.Create(nameof(ImagesList), typeof(List<string>), typeof(MainPage), new List<string>());

    public List<string> ImagesList
    {
        get => (List<string>)GetValue(ImagesListProperty);
        set => SetValue(ImagesListProperty, value);
    }

    //private readonly ImagesLoader imagesLoader;
    private string ErrorMessage;
    private bool IsBusy = false;


    public TemporaryGallery(bool isInAppGallery = true, string galleryPath = "TestGallery")
	{
		InitializeComponent();

        BindingContext = this;

        if (isInAppGallery) ImagesList = new Gallery(galleryPath).Images;
        else SetSpecificGallery(galleryPath);

       
    }

    private void SetSpecificGallery(string galleryPath)
    {
        ImagesLoader imagesLoader = new ImagesLoader(
          setErrorMessage: (msg) => ErrorMessage = msg,
          setBusy: (busy) => IsBusy = busy,
          setImagesList: (images) => ImagesList = images
        );

        imagesLoader.LoadImagesAsync(galleryPath);
    }

    private void SelectedBodyImage_Tapped(object sender, TappedEventArgs e)
    {

    }

    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        // ������������, �� BindingContext-� �� ����� ����������� � �������� ��� (string).
        if (sender is Image image && image.Source is FileImageSource fileImage)
        {
            string selectedImageName = fileImage.File;
            // �������� ������ �� ����������� ���� ��������
            ImageSelectedTask.TrySetResult(selectedImageName);
        }
        else if (sender is Image img && img.Source != null)
        {
            // ��� ���������� ���� ��� ImageSource, ���� �� ���������� ToString() ��� �� ��������� ��������.
            ImageSelectedTask.TrySetResult(img.Source.ToString());
        }
        // ��������� �������� ��������
        await Navigation.PopModalAsync();
    }
}



