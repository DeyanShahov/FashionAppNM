using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using Microsoft.Maui.Controls;
using CommunityToolkit.Maui.Views;

namespace FashionApp.core
{
    //internal class AddMaskToImage
    //{
    //    public static async Task<Stream> AddMaskToImageMetadata(Image originalImageSource, GraphicsView drawingView)
    //    {
    //        // Конвертираме ImageSource в Stream
    //        //var originalImageStream = await ((StreamImageSource)originalImageSource).Stream(CancellationToken.None);
    //        var originalImage = await originalImageSource.CaptureAsync();
    //        var originalImageStream = await originalImage.OpenReadAsync();

    //        // Взимаме маската от DrawingView
    //        var maskImage = await drawingView.CaptureAsync();
    //        var maskImageStream = await maskImage.OpenReadAsync();

    //        // Зареди оригиналното изображение
    //        using var image = await SixLabors.ImageSharp.Image.LoadAsync(originalImageStream);
    //        //using var image = await SixLabors.ImageSharp.Image.LoadAsync(originalImageSource.CaptureAsync());
    //        //using var image = originalImageSource.CaptureAsync();

    //        // Конвертирай маската в byte[]
    //        using var maskMemoryStream = new MemoryStream();
    //        await maskImageStream.CopyToAsync(maskMemoryStream);
    //        byte[] maskBytes = maskMemoryStream.ToArray();

    //        // Добави маската в EXIF метаданните
    //        var exif = image.Metadata.ExifProfile ?? new ExifProfile();
    //        exif.SetValue(ExifTag.MakerNote, maskBytes); // Използване на MakerNote за двоични данни
    //        image.Metadata.ExifProfile = exif;

    //        // Върни резултата като поток
    //        var resultStream = new MemoryStream();
    //        await image.SaveAsync(resultStream, new SixLabors.ImageSharp.Formats.Png.PngEncoder());
    //        resultStream.Position = 0;
    //        return resultStream;
    //    }
    //}
}
