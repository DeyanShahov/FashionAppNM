using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace FashionApp.core
{
    internal class AddMaskToImage
    {
        public static async Task<Stream> AddMaskToImageMetadata(Microsoft.Maui.Controls.Image originalImageSource, GraphicsView drawingView)
        {
            // Взимаме оригиналното изображение
            var originalImage = await originalImageSource.CaptureAsync();
            var originalImageStream = await originalImage.OpenReadAsync();

            // Взимаме маската
            var maskImage = await drawingView.CaptureAsync();
            var maskImageStream = await maskImage.OpenReadAsync();

            // Зареждаме оригиналното изображение и маската
            using var sourceImage = SixLabors.ImageSharp.Image.Load<Rgba32>(originalImageStream);
            using var maskImageSharp = SixLabors.ImageSharp.Image.Load<Rgba32>(maskImageStream);
            
            // Преоразмеряваме маската до размера на оригиналното изображение
            maskImageSharp.Mutate(x => x.Resize(sourceImage.Width, sourceImage.Height));
            
            // Създаваме ново изображение с прозрачен фон
            using var resultImage = new Image<Rgba32>(sourceImage.Width, sourceImage.Height);
            
            for (int y = 0; y < sourceImage.Height; y++)
            {
                for (int x = 0; x < sourceImage.Width; x++)
                {             
                    var sourcePixel = sourceImage[x, y];
                    var maskPixel = maskImageSharp[x, y];
                    
                    if (maskPixel.R != 0 || maskPixel.G != 0 || maskPixel.B != 0)
                    {
                        // Напълно прозрачен пиксел където има маска
                        resultImage[x, y] = new Rgba32(0, 0, 0, 0);
                    }
                    else
                    {
                        // Копираме оригиналния пиксел WHERE няма маска
                        resultImage[x, y] = sourcePixel;
                    }
                }
            }

            // Връщаме резултата като поток
            var resultStream = new MemoryStream();
            await resultImage.SaveAsPngAsync(resultStream);
            //var resizedImageResult = await ImageStreamResize.ResizeImageStream(resultStream, 500, 700);
            resultStream.Position = 0;
            return resultStream;
            //return resizedImageResult.ResizedStream;
        }
    }
}
