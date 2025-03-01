using SkiaSharp;

namespace FashionApp.core
{
    internal class ImageStreamResize
    {
        public static async Task<ResizedImageResult> ResizeImageStream(Stream imageStream, int maxWidth, int maxHeight)
        {
            // Зареждане на изображението
            imageStream.Position = 0; // Нулиране на позицията
            using var original = SKBitmap.Decode(imageStream);

            // Проверка дали е необходимо преоразмеряване
            if (original.Width <= maxWidth && original.Height <= maxHeight)
            {
                //imageStream.Position = 0;
                return new ResizedImageResult
                {
                    ResizedStream = imageStream,
                    Width = original.Width,
                    Height = original.Height
                };
            }

            // Изчисляване на новите размери с запазване на пропорциите
            float ratio = Math.Min((float)maxWidth / original.Width, (float)maxHeight / original.Height);
            int newWidth = (int)(original.Width * ratio);
            int newHeight = (int)(original.Height * ratio);

            // Преоразмеряване
            var samplingOptions = new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.None);
            using var resized = original.Resize(new SKImageInfo(newWidth, newHeight), samplingOptions);
            using var image = SKImage.FromBitmap(resized);
            using var data = image.Encode(SKEncodedImageFormat.Png, 80); // Png с качество 80%

            // Записване в нов поток
            var outputStream = new MemoryStream();
            data.SaveTo(outputStream);
            outputStream.Position = 0;
            return new ResizedImageResult
            {
                ResizedStream = outputStream,
                Width = newWidth,
                Height = newHeight
            };
        }
    }
    internal class ResizedImageResult
    {
        public Stream? ResizedStream { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}
