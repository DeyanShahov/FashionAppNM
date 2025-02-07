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

    public async Task<string> UploadImageAsync(string filePath, string uploadType = "input")
    {
        // Проверка дали файлът съществува
        if (!File.Exists(filePath))
        {
            return $"Error: File not found: {filePath}";
        }

        // Създаваме URL за качване
        //var url = $"{_serverUrl}/upload/image";
        var url = $"{_serverUrl}/upload_image";

        try
        {
            using var form = new MultipartFormDataContent();

            // Отваряме файла в бинарен режим
            using var fileStream = File.OpenRead(filePath);
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/png"); // Променете, ако имате различен формат

            // Добавяме файла към формата (ключ "image")
            form.Add(streamContent, "image", Path.GetFileName(filePath));

            // Добавяме и текстовото поле "type" (например "input" или "temp")
            form.Add(new StringContent(uploadType), "type");

            // Изпращаме POST заявката
            var response = await _httpClient.PostAsync(url, form);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return $"Success: {responseContent}";
            }
            else
            {
                return $"Upload error. Status code: {response.StatusCode}";
            }
        }
        catch (Exception ex)
        {
            return $"Upload exception: {ex.Message}";
        }
    }
}
