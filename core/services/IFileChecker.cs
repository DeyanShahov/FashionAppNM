namespace FashionApp.core.services
{
    internal interface IFileChecker
    {
        Task<bool> CheckFileExistsAsync(string fileName);
    }
}
