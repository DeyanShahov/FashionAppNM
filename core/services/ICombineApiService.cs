namespace FashionApp.core.services
{
    public interface ICombineApiService
    {
        Task<CombineApiResult> CombineImagesAsync(string clothImagePath, string bodyImagePath, bool useManualMask, List<string>? aiArgs = null);
    }
}
