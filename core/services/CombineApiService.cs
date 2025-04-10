using FashionApp.core.services;
using FashionApp.Data.Constants; // За AppConstants
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FashionApp.core.Services
{
    public class CombineApiService : ICombineApiService
    {
        private readonly HttpClient _httpClient;
        private const string BaseApiUrl = "http://77.77.134.134:82"; // Преместваме базовия URL тук

        // Инжектираме HttpClientFactory за по-добра практика
        public CombineApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("CombineApiClient"); // Създаваме именуван клиент
            _httpClient.Timeout = TimeSpan.FromSeconds(300); // Задаваме таймаут
        }

        // Метод за качване на едно изображение
        private async Task<bool> UploadImageAsync(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath) || !File.Exists(imagePath))
            {
                System.Diagnostics.Debug.WriteLine($"Upload Error: File not found or path is empty - {imagePath}");
                return false;
            }

            try
            {
                using var fileStream = File.OpenRead(imagePath);
                using var content = new MultipartFormDataContent();
                var streamContent = new StreamContent(fileStream);
                streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/png"); // Или друг подходящ тип
                // Използваме константа за името на файла при качване
                string uploadFileName = Path.GetFileName(imagePath) == AppConstants.Parameters.INPUT_IMAGE_CLOTH ? AppConstants.Parameters.INPUT_IMAGE_CLOTH : AppConstants.Parameters.INPUT_IMAGE_BODY;
                content.Add(streamContent, "image", uploadFileName); // API-то очаква поле 'image'

                string uploadUrl = $"{BaseApiUrl}/upload_image"; // Ендпойнт за качване
                var response = await _httpClient.PostAsync(uploadUrl, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Upload Failed for {imagePath}: {response.StatusCode}, Content: {errorContent}");
                    return false;
                }
                // Може да се добави парсване на отговора, ако е необходимо
                System.Diagnostics.Debug.WriteLine($"Upload Successful for {imagePath}");
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Upload Exception for {imagePath}: {ex.Message}");
                return false;
            }
        }

        public async Task<CombineApiResult> CombineImagesAsync(string clothImagePath, string bodyImagePath, bool useManualMask, List<string>? aiArgs = null)
        {
            // 1. Качване на изображенията
            bool clothUploaded = await UploadImageAsync(clothImagePath);
            // Използваме името на файла от пътя, за да зададем правилното име при второто качване
            bool bodyUploaded = await UploadImageAsync(bodyImagePath);


            if (!clothUploaded || !bodyUploaded)
            {
                return new CombineApiResult { Success = false, ErrorMessage = "AppConstants.Errors.ERROR_UPLOADING_IMAGES" };
            }

            // 2. Изпращане на POST заявка за комбиниране
            try
            {
                // Изчакване след качване (ако е необходимо за сървъра)
                await Task.Delay(2000); // Намалено време за изчакване

                var requestUrl = $"{BaseApiUrl}/{AppConstants.Parameters.CONFY_FUNCTION_COMBINE_ENDPOINT}";
                var requestBody = new
                {
                    function_name = AppConstants.Parameters.CONFY_FUNCTION_GENERATE_NAME,
                    // Подаваме само имената на файловете, както се очаква от ComfyUI след качване
                    cloth_image = Path.GetFileName(clothImagePath),
                    body_image = Path.GetFileName(bodyImagePath),
                    mask_detection_method = useManualMask,
                    args = aiArgs ?? new List<string>() // Използваме подадените аргументи или празен списък
                };

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    Encoding.UTF8,
                    "application/json"
                );

                var response = await _httpClient.PostAsync(requestUrl, jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Combine API Error: {response.StatusCode}, Content: {errorContent}");
                    return new CombineApiResult { Success = false, ErrorMessage = $"HTTP Error: {response.StatusCode}. {errorContent}" };
                }

                var contentType = response.Content.Headers.ContentType?.MediaType;
                var responseData = await response.Content.ReadAsByteArrayAsync();

                if (contentType != null && contentType.StartsWith("image/"))
                {
                    return new CombineApiResult { Success = true, ImageData = responseData, IsImageResult = true };
                }
                else
                {
                    // Ако не е изображение, връщаме текста като съобщение
                    var responseText = Encoding.UTF8.GetString(responseData);
                    System.Diagnostics.Debug.WriteLine($"Combine API Text Response: {responseText}");
                    return new CombineApiResult { Success = true, ErrorMessage = responseText, IsImageResult = false };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Combine API Exception: {ex.Message}");
                return new CombineApiResult { Success = false, ErrorMessage = $"{AppConstants.Errors.ERROR}: {ex.Message}" };
            }
        }
    }
}
