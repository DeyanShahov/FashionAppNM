using FashionApp.Data.Constants;
using System.Collections.ObjectModel;

namespace FashionApp.core.services
{
    public class MaskManagementService : IMaskManagementService
    {
        // Списък с дефиниции на маските за по-лесно управление
        private readonly List<(string Constant, string IconPath, string DataIdentifier)> _maskDefinitions = new()
        {
            (AppConstants.ImagesConstants.DRESS_MASK, "Macros/icons_dress.png", "dress"),
            (AppConstants.ImagesConstants.DRESS_FULL_MASK, "Macros/icons_dress_full.png", "dress_full"),
            (AppConstants.ImagesConstants.JACKET_MASK, "Macros/icons_jacket.png", "jacket"),
            (AppConstants.ImagesConstants.CLOSED_JACKET_MASK, "Macros/icons_jacket_closed.png", "jacket_closed"),
            (AppConstants.ImagesConstants.OPEN_JACKET_MASK, "Macros/icons_jacket_open.png", "jacket_open"),
            (AppConstants.ImagesConstants.NO_SET_MASK, "Macros/icons_no_set.png", "no_set"),
            (AppConstants.ImagesConstants.PANTS_MASK, "Macros/icons_pants.png", "pants"),
            (AppConstants.ImagesConstants.PANTS_SHORT_MASK, "Macros/icons_pants_short.png", "pants_short"),
            (AppConstants.ImagesConstants.RAINCOAT_MASK, "Macros/icons_raincoat.png", "raincoat"),
            (AppConstants.ImagesConstants.SHIRT_MASK, "Macros/icons_shirt.png", "shirt"),
            (AppConstants.ImagesConstants.SKIRT_MASK, "Macros/icons_skirt.png", "skirt"),
            (AppConstants.ImagesConstants.SKIRT_LONG_MASK, "Macros/icons_skirt_long.png", "skirt_long"),
            (AppConstants.ImagesConstants.TANK_TOP_MASK, "Macros/icons_tank_top.png", "tank_top")
        };

        private readonly IServiceProvider _serviceProvider; // За достъп до платформени услуги

        public MaskManagementService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task LoadAvailableMasksAsync(ObservableCollection<JacketModel> targetCollection)
        {
#if ANDROID
            targetCollection.Clear(); // Изчистваме преди зареждане

            // Проверка за разрешения
            _serviceProvider.GetService<CheckForAndroidPermissions>()?.CheckStorage();

            try
            {
                var fileChecker = _serviceProvider.GetService<IFileChecker>();
                if (fileChecker == null)
                {
                    System.Diagnostics.Debug.WriteLine("IFileChecker service not found.");
                    return; // Или хвърлете грешка
                }


                foreach (var maskDef in _maskDefinitions)
                {
                    bool fileExists = await fileChecker.CheckFileExistsAsync(maskDef.Constant);
                    if (fileExists)
                    {
                        targetCollection.Add(new JacketModel { ImagePath = maskDef.IconPath, Data = maskDef.DataIdentifier });
                    }
                }
            }
            catch (Exception ex)
            {
                // Логване на грешката
                System.Diagnostics.Debug.WriteLine($"{AppConstants.Errors.ERROR_CKECK_MASKS}: {ex.Message}");
            }
#else
            // Не правим нищо на други платформи засега
            await Task.CompletedTask;
#endif
        }

        // Връща пълния път до файла с маската в специфичната за платформата директория
        public Task<string?> GetMaskPathAsync(string maskIdentifier)
        {
            string? maskFileName = _maskDefinitions.FirstOrDefault(m => m.DataIdentifier == maskIdentifier).Constant;
            if (string.IsNullOrEmpty(maskFileName))
            {
                return Task.FromResult<string?>(null);
            }

#if ANDROID
            string imagePath = Path.Combine(
                    AppConstants.Parameters.APP_BASE_PATH,
                    AppConstants.Parameters.APP_NAME,
                    AppConstants.Parameters.APP_FOLDER_MASK,
                    maskFileName); // Използваме името на файла от константата
            return Task.FromResult<string?>(imagePath);
#else
             // Върнете null или имплементирайте за други платформи
             return Task.FromResult<string?>(null);
#endif
        }

        // Копира маската в кеша и връща пътя до кеширания файл
        public async Task CopyMaskToCacheAsync(string maskIdentifier)
        {
            string? sourcePath = await GetMaskPathAsync(maskIdentifier);
            if (string.IsNullOrEmpty(sourcePath) || !File.Exists(sourcePath))
            {
                System.Diagnostics.Debug.WriteLine($"Mask source file not found: {sourcePath}");
                return; // Връщаме null, ако няма изходен файл
            }

            try
            {
                var fileName = Path.GetFileName(sourcePath);
                var cacheDir = FileSystem.CacheDirectory;
                var destPath = Path.Combine(cacheDir, fileName); // Дестинация в кеша

                // Копираме само ако файлът не съществува в кеша или е различен (опционално)
                if (!File.Exists(destPath) /* || CompareFiles(sourcePath, destPath) */)
                {
                    using (var sourceStream = File.OpenRead(sourcePath))
                    using (var destStream = File.Create(destPath))
                    {
                        await sourceStream.CopyToAsync(destStream);
                    }
                }
                // Връщаме пътя до кеширания файл
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error copying mask to cache: {ex.Message}");
                // Връщаме null при грешка
            }
        }
    }
}
