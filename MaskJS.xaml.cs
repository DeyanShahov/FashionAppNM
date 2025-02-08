using System.IO;
namespace FashionApp;

public partial class MaskJS : ContentPage
{
    public MaskJS()
    {
        InitializeComponent();

        var htmlSource = new HtmlWebViewSource();
        htmlSource.BaseUrl = FileSystem.AppDataDirectory;
        var stream = FileSystem.OpenAppPackageFileAsync("index.html").Result;
        using (var reader = new StreamReader(stream))
        {
            htmlSource.Html = reader.ReadToEnd();
        }
        maskJS.Source = htmlSource;
    }
}
