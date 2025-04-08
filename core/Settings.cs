using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace FashionApp.core
{
    public class Settings
    {
        private string userName = "Guest";
        private string phoneIdentificatoryNumber = "0";
        private int tokens = 0;

        public string UserName
        {
            get => userName;
            set => SetProperty(ref userName, value);
        }

        public string PhoneIdentificatoryNumber
        {
            get => phoneIdentificatoryNumber;
            set => SetProperty(ref phoneIdentificatoryNumber, value);
        }

        public int Tokens
        {
            get => tokens;
            set => SetProperty(ref tokens, value);
        }



        // --- Реализация на INotifyPropertyChanged ---
        // Това позволява на UI елементите, които са свързани (bind-нати) към тези свойства,
        // да се обновят автоматично, когато стойността на свойството се промени.

        public event PropertyChangedEventHandler PropertyChanged;

        // Метод за лесно задаване на стойност и извикване на PropertyChanged
        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false; // Стойността не е променена
            }

            storage = value; // Задаване на новата стойност
            OnPropertyChanged(propertyName); // Уведомяване за промяна
            return true;
        }

        // Метод за извикване на събитието PropertyChanged
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        // --- Край на реализацията на INotifyPropertyChanged ---


        // Можете да добавите методи за запазване/зареждане на настройките, ако е необходимо
        // Например, използвайки Preferences:
        // public void SaveSettings()
        // {
        //     Preferences.Set(nameof(UserName), UserName);
        //     Preferences.Set(nameof(PhoneIdentificatoryNumber), PhoneIdentificatoryNumber);
        //     Preferences.Set(nameof(Tokens), Tokens);
        // }
        //
        // public void LoadSettings()
        // {
        //     UserName = Preferences.Get(nameof(UserName), "Guest");
        //     PhoneIdentificatoryNumber = Preferences.Get(nameof(PhoneIdentificatoryNumber), "0");
        //     Tokens = Preferences.Get(nameof(Tokens), 0);
        // }

    }
}
