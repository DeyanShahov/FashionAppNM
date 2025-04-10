using System.Collections.ObjectModel;

namespace FashionApp.core.services
{
    public interface IMaskManagementService
    {
        Task LoadAvailableMasksAsync(ObservableCollection<JacketModel> targetCollection);
        Task<string?> GetMaskPathAsync(string maskIdentifier); // Добавяме метод за вземане на път
        Task CopyMaskToCacheAsync(string maskIdentifier); // Добавяме метод за копиране
    }
}
