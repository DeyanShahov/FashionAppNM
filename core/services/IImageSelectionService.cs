namespace FashionApp.core.services
{
    public interface IImageSelectionService
    {
        Task SelectImageFromFilePickerAsync(Action<ImageSource, string> updateAction);
        Task SelectImageFromGalleryAsync(Func<Task<string>> gallerySelector, Action<ImageSource, string> updateAction);
        Task ProcessCapturedImageAsync(Stream imageStream, Action<ImageSource, string> updateAction);
        Task<string> PrepareImageForUpload(Stream imageStream);
        void DeleteTemporaryImages();
    }
}
