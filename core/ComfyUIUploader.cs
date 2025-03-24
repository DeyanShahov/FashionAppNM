using FashionApp.Data.Constants;
using System.Net.Http.Headers;

public class ComfyUIUploader
{
    private readonly HttpClient _httpClient;
    private readonly string _serverUrl; // Пример: "http://127.0.0.1:8188"

    public ComfyUIUploader(string serverUrl)
    {
        _serverUrl = serverUrl.TrimEnd('/');
        _httpClient = new HttpClient();
    }

    //public async Task<string> UploadImageAsync(string filePath, bool isFromInAppGallery = false, string uploadType = "input")
    public async Task<string> UploadImageAsync(string filePath)
    {
        bool isFromInAppGallery = filePath.Contains("Gallery");

        if (!isFromInAppGallery && !File.Exists(filePath))
        {
            return $"{AppConstants.Errors.ERROR}: {AppConstants.Errors.FILE_NOT_FOUND}: {filePath}";
        }

        // Създаваме URL за качване
        var url = $"{_serverUrl}/upload_image";

        try
        {
            using var form = new MultipartFormDataContent();

            // Отваряме файла в бинарен режим
            using var fileStream = isFromInAppGallery ?  await FileSystem.OpenAppPackageFileAsync(filePath) : File.OpenRead(filePath);
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/png"); // Променете, ако имате различен формат

            // Добавяме файла към формата (ключ "image")
            form.Add(streamContent, "image", Path.GetFileName(filePath));

            // Добавяме и текстовото поле "type" (например "input" или "temp")
            form.Add(new StringContent("input"), "type");

            // Изпращаме POST заявката
            var response = await _httpClient.PostAsync(url, form);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return $"{AppConstants.Messages.SUCCESS}: {responseContent}";
            }
            else
            {
                return $"{AppConstants.Errors.UPLOAD_ERROR}: {response.StatusCode}";
            }
        }
        catch (Exception ex)
        {
            return $"{AppConstants.Errors.UPLOAD_EXCEPTION}: {ex.Message}";
        }
    }
}
