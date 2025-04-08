using Plugin.AdMob;
using Plugin.AdMob.Services;
using System.Diagnostics;

namespace FashionApp.Pages;

public partial class AdvertisementPage : ContentPage
{
    private readonly IRewardedInterstitialAdService _rewardedInterstitialAdService;
    private int tokens = 0;
    public AdvertisementPage()
	{
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
        tokens += rewardItem.Amount;

        await Navigation.PopModalAsync();

        //GoogleAdsButton.IsEnabled = false;
        //GoogleAdsButton.IsVisible = false;

        //CombineImagesButton.IsEnabled = true;
        //CombineImagesButton.IsVisible = true;
        //await CombineImagesAction();
    }

    private void ToggleLoading(bool isLoading)
    {
        LoadingIndicator.IsRunning = isLoading;
        LoadingIndicator.IsVisible = isLoading;
    }
}