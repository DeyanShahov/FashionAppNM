using FashionApp.core;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;

namespace FashionApp.Pages;

public partial class AdvertisementPage : ContentPage
{
    //private readonly Settings _appSettings; // ���� �� ���������� �� ����������� �� Settings

    // ��������, ��� ����� UI ���� �� �� ������ (bind)
    //public string CurrentUserName => _appSettings.UserName;
    //public string CurrentPhoneNumber => _appSettings.PhoneIdentificatoryNumber;
    //public int CurrentTokens => _appSettings.Tokens;

    // ������� �� �������� ������� �� �����������
    public ICommand UpdateSettingsCommand { get; }

    //NOWO ZADRASKANA !!!!
    //private readonly IRewardedInterstitialAdService _rewardedInterstitialAdService;

    //public AdvertisementPage(Settings settings)
    public AdvertisementPage()
    {
        //_appSettings = settings; // ��������� ������������� ���������

        //// ������� �� ������� � �����������, �� �� ������� ViewModel ����������
        //_appSettings.PropertyChanged += (sender, args) =>
        //{
        //    // ��������� OnPropertyChanged �� ����������� �������� ��� ViewModel,
        //    // �� �� �������� UI �� ���������.
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
        //        OnPropertyChanged(nameof(CanIncreaseTokens)); // ���������� � ����������� �� ���������
        //    }
        //};

        //// �������������� ���������
        //UpdateSettingsCommand = new Command(UpdateUserSettings, CanIncreaseTokens);


        InitializeComponent();

        //NOWO ZADRASKANA !!!!
        //_rewardedInterstitialAdService = FashionApp.core.services.ServiceProvider.GetRequiredService<IRewardedInterstitialAdService>();
        //_rewardedInterstitialAdService.OnAdLoaded += (_, __) => Debug.WriteLine("Rewarded interstitial ad prepared.");
        //_rewardedInterstitialAdService.PrepareAd(onUserEarnedReward: UserDidEarnReward);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        CreateRewardedInterstitial();
    }

    #region AdMob
    private void CreateRewardedInterstitial()
    {
        //NOWO ZADRASKANA !!!!
        //var rewardedInterstitialAd = _rewardedInterstitialAdService.CreateAd();
        //rewardedInterstitialAd.OnUserEarnedReward += (_, reward) =>
        //{
        //    UserDidEarnReward(reward);
        //};
        //rewardedInterstitialAd.OnAdLoaded += RewardedInterstitialAd_OnAdLoaded;
        //rewardedInterstitialAd.Load();

    }

    private void RewardedInterstitialAd_OnAdLoaded(object? sender, EventArgs e)
    {
        //NOWO ZADRASKANA !!!!
        //if (sender is IRewardedInterstitialAd rewardedInterstitialAd)
        //{
        //    ToggleLoading(false);
        //    rewardedInterstitialAd.Show();
        //}
    }

    //NOWO ZADRASKANA !!!!
    //private async void UserDidEarnReward(RewardItem rewardItem)
    //{
    //    Debug.WriteLine($"User earned {rewardItem.Amount} {rewardItem.Type}.");
    //    UpdateUserSettings();
    //    await Navigation.PopModalAsync();
    //}
    #endregion


    #region Settings
    // �����, ���������� �� ���������
    private void UpdateUserSettings()
    {
        // ������ �� ������� �� �����������
        //_appSettings.Tokens += 1;
        // PhoneIdentificatoryNumber ���� �� �� ������� �� ���� �����
        //_appSettings.UserName = "New User";

        AppSettings.Tokens++;
    }

    // �����, ����� �������� ���� ��������� ���� �� �� �������
    //private bool CanIncreaseTokens()
    //{
    //    // ������: ����������� ����������� ���� ��� �������� �� ��� 10
    //    //return _appSettings.Tokens < 10;
    //    return AppSettings.Tokens < 10;
    //}


    // --- ���������� �� INotifyPropertyChanged �� ViewModel ---
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    // --- ���� �� ������������ �� INotifyPropertyChanged ---
    #endregion


    #region UI
    private void ToggleLoading(bool isLoading)
    {
        LoadingIndicator.IsRunning = isLoading;
        LoadingIndicator.IsVisible = isLoading;
    }
    #endregion
}