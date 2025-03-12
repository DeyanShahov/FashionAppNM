using FashionApp.core.services;
using FashionApp.Data.Constants;
using System.Reflection;


#if ANDROID
using Android.Content;
using Android.Provider;
#endif

namespace FashionApp.Pages;

public partial class TestGallery : ContentPage
{
    // ���� �������� �� ����� �� ������� �� ��������� ����������� ��� ��������� ��������.
    public TaskCompletionSource<string> ImageSelectedTask { get; set; } = new TaskCompletionSource<string>();
    //public TaskCompletionSource<Image> ImageTest {  get; set; } = new TaskCompletionSource<Image>();

    //public static readonly BindableProperty ImagesListProperty =
    //        BindableProperty.Create(nameof(ImagesList), typeof(List<string>), typeof(MainPage), new List<string>());
    public static readonly BindableProperty ImagesListProperty =
           BindableProperty.Create(nameof(ImagesList), typeof(List<string>), typeof(MainPage), new List<string>());

    //public List<string> ImagesList
    //{
    //    get => (List<string>)GetValue(ImagesListProperty);
    //    set => SetValue(ImagesListProperty, value);
    //}

    public List<string> ImagesList
    {
        get => (List<string>)GetValue(ImagesListProperty);
        set => SetValue(ImagesListProperty, value);
    }

    public TestGallery(string galleryName, string galleryPath)
	{
		InitializeComponent();

        Title = galleryName; // ������� ����� �� ��������� 
        BindingContext = this;

        RunManiq(galleryPath);      
    }

    public void RunManiq(string galleryPath)
    {
         var gallery = new Gallery(galleryPath);//.LoadImages();
         ImagesList = gallery.LoadImages();
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


public class Gallery
{
    public string Path { get; set; }
    public List<string> Images { get; set; }

    public Gallery(string path)
    {
        Path = path;
        Images = LoadImages();
    }

    //        .
    //      ,   Resources/Images/TestGallery.
    //private List<string> LoadImages()
    //{
    //    List<string> imagesNameList = new List<string>();

    //    if (Path == "TestGallery")
    //    {
    //        for (int i = 2; i <= 37; i++)
    //        {
    //            imagesNameList.Add('a' + i.ToString());
    //        }
    //    }
    //    return imagesNameList;
    //}

    public List<string> LoadImages()
    {
        List<string> imagesNameList = new List<string>();

        if (true)
        {
            for (int i = 2; i <= 37; i++)
            {
                imagesNameList.Add('a' + i.ToString());
            }
        }
        return imagesNameList;
    }

    //public List<string> LoadImages()
    //{
    //    List<string> imagesNameList = new List<string>();

    //    if (Path == "TestGallery")
    //    {
    //        for (int i = 2; i <= 37; i++)
    //        {
    //            imagesNameList.Add('a' + i.ToString());
    //        }
    //    }
    //    return imagesNameList;
    //}

    public async Task<byte[]> LoadEmbeddedImageAsync(string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourcePath = $"FashionApp.Resources.Images.TestGallery.{fileName}.jpg"; //     namespace-!

        using Stream stream = assembly.GetManifestResourceStream(resourcePath);
        if (stream == null)
            throw new FileNotFoundException($"Embedded resource {fileName} not found.");

        using MemoryStream memoryStream = new();
        await stream.CopyToAsync(memoryStream);
        return memoryStream.ToArray();
    }
}