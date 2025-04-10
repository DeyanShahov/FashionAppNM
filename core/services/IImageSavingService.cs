namespace FashionApp.core.services
{
    public interface IImageSavingService
    {
        Task<bool> SaveImageAsync(byte[] imageData, string desiredFileNameFormat = "image_{0:yyyyMMdd_HHmmss}.png");
    }
}
