namespace FashionApp.core.services
{
    public class CombineApiResult
    {
        public bool Success { get; set; }
        public byte[]? ImageData { get; set; }
        public string? ErrorMessage { get; set; }
        public bool IsImageResult { get; set; } // Указва дали резултатът е изображение или текст
    }
}
