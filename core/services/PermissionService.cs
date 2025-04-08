namespace FashionApp.core.services
{
    public static class PermissionService
    {
        // Общ метод за проверка и искане на разрешение
        private static async Task<PermissionStatus> CheckAndRequestPermissionAsync<TPermission>()
            where TPermission : Permissions.BasePermission, new()
        {
            PermissionStatus status = await Permissions.CheckStatusAsync<TPermission>();

            // Ако вече е дадено или ограничено (напр. от родителски контрол), не можем да искаме
            if (status == PermissionStatus.Granted || status == PermissionStatus.Restricted)
            {
                return status;
            }

            // На iOS, ако е отказано перманентно, потребителят трябва да отиде в настройките
            if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
            {
                // Може да покажете съобщение, което да насочи потребителя към настройките
                await Shell.Current.DisplayAlert("Нужно Разрешение",
                   $"За да използвате тази функция, моля активирайте разрешението '{typeof(TPermission).Name.Replace("Permissions+", "")}' от настройките на приложението.",
                   "OK");
                return status; // Връщаме отказан статус
            }

            // Показване на обяснение (rationale) преди искане (по избор, но добра практика)
            // MAUI Permissions API може да покаже свое обяснение на някои платформи.
            // Можете да добавите и ваше собствено тук, ако Permissions.ShouldShowRationale<TPermission>() върне true.
            // if (Permissions.ShouldShowRationale<TPermission>())
            // {
            //     await Shell.Current.DisplayAlert("Обяснение", "Имаме нужда от това разрешение за...", "Разбирам");
            // }

            // Искане на разрешението
            status = await Permissions.RequestAsync<TPermission>();

            return status;
        }

        // --- Специфични методи за всяко разрешение ---

        public static async Task<PermissionStatus> CheckAndRequestCameraAsync()
        {
            return await CheckAndRequestPermissionAsync<Permissions.Camera>();
        }

        /// <summary>
        /// Проверява и иска разрешение за четене на снимки от галерията.
        /// Използва Permissions.Photos, което се адаптира към версията на Android.
        /// </summary>
        public static async Task<PermissionStatus> CheckAndRequestPhotosReadAsync()
        {
            // Permissions.Photos е по-добрият избор за четене на снимки на всички версии
            return await CheckAndRequestPermissionAsync<Permissions.Photos>();

            // Алтернатива (по-стара):
            // return await CheckAndRequestPermissionAsync<Permissions.StorageRead>();
        }

        /// <summary>
        /// Проверява и иска разрешение за запис в хранилището.
        /// ВНИМАНИЕ: На Android 11+ (API 30+) това разрешение има силно
        /// ограничени възможности и НЕ позволява запис/триене навсякъде.
        /// Използвайте MediaStore или Storage Access Framework за тези цели.
        /// </summary>
        public static async Task<PermissionStatus> CheckAndRequestStorageWriteAsync()
        {
            // Предупреждение за ограниченията на API 30+
            if (DeviceInfo.Platform == DevicePlatform.Android && DeviceInfo.Version.Major >= 11) // Android 11+
            {
                Console.WriteLine("ПРЕДУПРЕЖДЕНИЕ: Permissions.StorageWrite има ограничени права на Android 11+.");
                // Може да покажете и съобщение на потребителя, ако е нужно
            }
            return await CheckAndRequestPermissionAsync<Permissions.StorageWrite>();
        }

        // --- Пример за комбинирано искане (ако е нужно при старт на дадена функционалност) ---

        /// <summary>
        /// Проверява и иска всички необходими разрешения за основната функционалност (камера и четене на снимки).
        /// Връща true, ако ВСИЧКИ нужни разрешения са дадени, иначе false.
        /// </summary>
        public static async Task<bool> EnsureRequiredPermissionsAsync()
        {
            Console.WriteLine("Проверка на необходимите разрешения...");

            var cameraStatus = await CheckAndRequestCameraAsync();
            if (cameraStatus != PermissionStatus.Granted)
            {
                await Shell.Current.DisplayAlert("Грешка", "Разрешението за камера е необходимо за тази функция.", "OK");
                Console.WriteLine($"Статус на камера: {cameraStatus}");
                return false;
            }
            Console.WriteLine("Разрешение за камера: OK");


            var photosStatus = await CheckAndRequestPhotosReadAsync();
            if (photosStatus != PermissionStatus.Granted)
            {
                // StorageRead може да е алтернатива или допълнение при нужда от достъп до други файлове
                // var storageReadStatus = await CheckAndRequestPermissionAsync<Permissions.StorageRead>();
                // if (storageReadStatus != PermissionStatus.Granted) { ... }

                await Shell.Current.DisplayAlert("Грешка", "Разрешението за достъп до снимки е необходимо.", "OK");
                Console.WriteLine($"Статус на снимки: {photosStatus}");
                return false;
            }
            Console.WriteLine("Разрешение за четене на снимки: OK");

            // НЕ искайте StorageWrite тук, освен ако не е АБСОЛЮТНО необходимо
            // и сте наясно с ограниченията. Искайте го точно преди операцията по запис/триене.
            // var storageWriteStatus = await CheckAndRequestStorageWriteAsync();
            // if (storageWriteStatus != PermissionStatus.Granted)
            // {
            //     await Shell.Current.DisplayAlert("Грешка", "Разрешението за запис в хранилището е необходимо.", "OK");
            //     return false;
            // }

            Console.WriteLine("Всички основни разрешения са налични.");
            return true;
        }

    }
}
