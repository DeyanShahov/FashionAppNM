using FashionApp.core;
using Plugin.AdMob;
using Plugin.AdMob.Services;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;

namespace FashionApp.Pages;

public partial class AdvertisementPage : ContentPage
{
    private readonly Settings _appSettings; // Поле за съхранение на инстанцията на Settings

    // Свойства, към които UI може да се свърже (bind)
    public string CurrentUserName => _appSettings.UserName;
    public string CurrentPhoneNumber => _appSettings.PhoneIdentificatoryNumber;
    public int CurrentTokens => _appSettings.Tokens;

    // Команда за примерна промяна на настройките
    public ICommand UpdateSettingsCommand { get; }


    private readonly IRewardedInterstitialAdService _rewardedInterstitialAdService;
    //private int tokens = 0;
    public AdvertisementPage(Settings settings)
    {
        _appSettings = settings; // Запазваме инжектираната инстанция

        //// Слушаме за промени в настройките, за да обновим ViewModel свойствата
        //_appSettings.PropertyChanged += (sender, args) =>
        //{
        //    // Извикваме OnPropertyChanged за съответното свойство във ViewModel,
        //    // за да уведомим UI за промяната.
        //    if (args.PropertyName == nameof(Settings.UserName))
        //    {
        //        OnPropertyChanged(nameof(CurrentUserName));
        //    }
        //    else if (args.PropertyName == nameof(Settings.PhoneIdentificatoryNumber))
        //    {
        //        OnPropertyChanged(nameof(CurrentPhoneNumber));
        //    }
        //    else if (args.PropertyName == nameof(Settings.Tokens))
        //    {
        //        OnPropertyChanged(nameof(CurrentTokens));
        //        OnPropertyChanged(nameof(CanIncreaseTokens)); // Обновяваме и състоянието на командата
        //    }
        //};

        //// Инициализираме командата
        //UpdateSettingsCommand = new Command(UpdateUserSettings, CanIncreaseTokens);


        InitializeComponent();

        _rewardedInterstitialAdService = FashionApp.core.services.ServiceProvider.GetRequiredService<IRewardedInterstitialAdService>();
        _rewardedInterstitialAdService.OnAdLoaded += (_, __) => Debug.WriteLine("Rewarded interstitial ad prepared.");
        _rewardedInterstitialAdService.PrepareAd(onUserEarnedReward: UserDidEarnReward);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        CreateRewardedInterstitial();
    }

    #region AdMob
    private void CreateRewardedInterstitial()
    {
        var rewardedInterstitialAd = _rewardedInterstitialAdService.CreateAd();
        rewardedInterstitialAd.OnUserEarnedReward += (_, reward) =>
        {
            UserDidEarnReward(reward);
        };
        rewardedInterstitialAd.OnAdLoaded += RewardedInterstitialAd_OnAdLoaded;
        rewardedInterstitialAd.Load();

    }

    private void RewardedInterstitialAd_OnAdLoaded(object? sender, EventArgs e)
    {
        if (sender is IRewardedInterstitialAd rewardedInterstitialAd)
        {
            ToggleLoading(false);
            rewardedInterstitialAd.Show();
        }
    }

    private async void UserDidEarnReward(RewardItem rewardItem)
    {
        Debug.WriteLine($"User earned {rewardItem.Amount} {rewardItem.Type}.");
        UpdateUserSettings();
        //tokens += rewardItem.Amount;

        await Navigation.PopModalAsync();
    }
    #endregion


    #region Settings
    // Метод, изпълняван от командата
    private void UpdateUserSettings()
    {
        // Пример за промяна на настройките
        _appSettings.Tokens += 1;
        // PhoneIdentificatoryNumber може да се промени по друг начин
        //_appSettings.UserName = "New User";
    }

    // Метод, който определя дали командата може да се изпълни
    private bool CanIncreaseTokens()
    {
        // Пример: Позволяваме увеличаване само ако токените са под 10
        return _appSettings.Tokens < 10;
    }


    // --- Реализация на INotifyPropertyChanged за ViewModel ---
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    // --- Край на реализацията на INotifyPropertyChanged ---
    #endregion


    #region UI
    private void ToggleLoading(bool isLoading)
    {
        LoadingIndicator.IsRunning = isLoading;
        LoadingIndicator.IsVisible = isLoading;
    }
    #endregion
}