using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdsIronSource : AdsProvider
{
#if USE_IRONSOURCE_SDK

  [SerializeField] bool                         _initializeInCode = false;
  [Tooltip("should be auto filled on gamestart with key from IronSourceMediationSettings asset")]
  [SerializeField] string                       _appKey;
  [SerializeField] IronSourceMediationSettings  _ISMedAsset;
  [Header("banner opts")]
  [SerializeField] IronSourceBannerPosition     _bannerPos;
  [SerializeField] Ads.Banner.Type              _bannerType;

  void Awake()
  {
  }
  void Start()
  {
    if(Debug.isDebugBuild)
    {
      IronSource.Agent.validateIntegration();
    }
    else
    {
      Debug.unityLogger.logEnabled = false;
    }

    if(_initializeInCode)
    {
      _appKey = _ISMedAsset.AndroidAppKey;
      Log($"call init key:[{_appKey}]");
      IronSource.Agent.setConsent(GameLib.Mobile.GDPR.AdvertisingConsent);
      IronSource.Agent.init(_appKey);
      //onInitialize?.Invoke();
    }
  }
  void OnEnable ()
  {
    IronSourceEvents.onSdkInitializationCompletedEvent += SdkInitializationCompletedEvent;

    IronSourceRewardedVideoEvents.onAdOpenedEvent += RewardedVideoOnAdOpenedEvent;
    IronSourceRewardedVideoEvents.onAdClosedEvent += RewardedVideoOnAdClosedEvent;
    IronSourceRewardedVideoEvents.onAdAvailableEvent += RewardedVideoOnAdAvailable;
    IronSourceRewardedVideoEvents.onAdUnavailableEvent += RewardedVideoOnAdUnavailable;
    IronSourceRewardedVideoEvents.onAdShowFailedEvent += RewardedVideoOnAdShowFailedEvent;
    IronSourceRewardedVideoEvents.onAdRewardedEvent += RewardedVideoOnAdRewardedEvent;
    IronSourceRewardedVideoEvents.onAdClickedEvent += RewardedVideoOnAdClickedEvent;

    IronSourceInterstitialEvents.onAdReadyEvent += InterstitialOnAdReadyEvent;
    IronSourceInterstitialEvents.onAdLoadFailedEvent += InterstitialOnAdLoadFailed;
    IronSourceInterstitialEvents.onAdOpenedEvent += InterstitialOnAdOpenedEvent;
    IronSourceInterstitialEvents.onAdClickedEvent += InterstitialOnAdClickedEvent;
    IronSourceInterstitialEvents.onAdShowSucceededEvent += InterstitialOnAdShowSucceededEvent;
    IronSourceInterstitialEvents.onAdShowFailedEvent += InterstitialOnAdShowFailedEvent;
    IronSourceInterstitialEvents.onAdClosedEvent += InterstitialOnAdClosedEvent;

    IronSourceBannerEvents.onAdLoadedEvent += BannerOnAdLoadedEvent;
    IronSourceBannerEvents.onAdLoadFailedEvent += BannerOnAdLoadFailedEvent;
    IronSourceBannerEvents.onAdClickedEvent += BannerOnAdClickedEvent;
    IronSourceBannerEvents.onAdScreenPresentedEvent += BannerOnAdScreenPresentedEvent;
    IronSourceBannerEvents.onAdScreenDismissedEvent += BannerOnAdScreenDismissedEvent;
    IronSourceBannerEvents.onAdLeftApplicationEvent += BannerOnAdLeftApplicationEvent;
  }
  void OnDisable()
  {
    IronSourceRewardedVideoEvents.onAdOpenedEvent -= RewardedVideoOnAdOpenedEvent;
    IronSourceRewardedVideoEvents.onAdClosedEvent -= RewardedVideoOnAdClosedEvent;
    IronSourceRewardedVideoEvents.onAdAvailableEvent -= RewardedVideoOnAdAvailable;
    IronSourceRewardedVideoEvents.onAdUnavailableEvent -= RewardedVideoOnAdUnavailable;
    IronSourceRewardedVideoEvents.onAdShowFailedEvent -= RewardedVideoOnAdShowFailedEvent;
    IronSourceRewardedVideoEvents.onAdRewardedEvent -= RewardedVideoOnAdRewardedEvent;
    IronSourceRewardedVideoEvents.onAdClickedEvent -= RewardedVideoOnAdClickedEvent;

    IronSourceInterstitialEvents.onAdReadyEvent -= InterstitialOnAdReadyEvent;
    IronSourceInterstitialEvents.onAdLoadFailedEvent -= InterstitialOnAdLoadFailed;
    IronSourceInterstitialEvents.onAdOpenedEvent -= InterstitialOnAdOpenedEvent;
    IronSourceInterstitialEvents.onAdClickedEvent -= InterstitialOnAdClickedEvent;
    IronSourceInterstitialEvents.onAdShowSucceededEvent -= InterstitialOnAdShowSucceededEvent;
    IronSourceInterstitialEvents.onAdShowFailedEvent -= InterstitialOnAdShowFailedEvent;
    IronSourceInterstitialEvents.onAdClosedEvent -= InterstitialOnAdClosedEvent;

    IronSourceBannerEvents.onAdLoadedEvent -= BannerOnAdLoadedEvent;
    IronSourceBannerEvents.onAdLoadFailedEvent -= BannerOnAdLoadFailedEvent;
    IronSourceBannerEvents.onAdClickedEvent -= BannerOnAdClickedEvent;
    IronSourceBannerEvents.onAdScreenPresentedEvent -= BannerOnAdScreenPresentedEvent;
    IronSourceBannerEvents.onAdScreenDismissedEvent -= BannerOnAdScreenDismissedEvent;
    IronSourceBannerEvents.onAdLeftApplicationEvent -= BannerOnAdLeftApplicationEvent;
  }
  void OnApplicationPause(bool isPaused)
  {
    IronSource.Agent.onApplicationPause(isPaused);
  }
  void SdkInitializationCompletedEvent()
  {
    Log("SdkInitializationCompletedEvent");
    IronSource.Agent.shouldTrackNetworkState(true);
    IronSource.Agent.loadInterstitial();
    IronSource.Agent.loadRewardedVideo();
    //IronSource.Agent.loadBanner(IronSourceBannerSize.LARGE, _bannerPos, "DefaultBanner");
  }

  public override bool IntersIsAvailable(string placement = null) => IronSource.Agent.isInterstitialReady();
  public override void IntersShow(string placement)
  {
    if(!string.IsNullOrEmpty(placement))
      IronSource.Agent.showInterstitial(placement);
    else
      IronSource.Agent.showInterstitial();
  }
  public override bool RewardedIsAvailable(string placement = null) => IronSource.Agent.isRewardedVideoAvailable();
  public override void RewardedShow(string placement)
  {
    if(!string.IsNullOrEmpty(placement))
      IronSource.Agent.showRewardedVideo(placement);
    else
      IronSource.Agent.showInterstitial();
  }

  void RewardedVideoOnAdOpenedEvent(IronSourceAdInfo adInfo)
  {
    Log("RewardedVideoOnAdOpenedEvent");
  }
  void RewardedVideoOnAdClosedEvent(IronSourceAdInfo adInfo)
  {
    Log("RewardedVideoOnAdClosedEvent");
    IronSource.Agent.loadRewardedVideo();
  }
  void RewardedVideoOnAdAvailable(IronSourceAdInfo adInfo)
  {
    Log("RewardedVideoOnAdAvailable");
    Ads.Rewarded.onAvailable?.Invoke();
  }
  void RewardedVideoOnAdUnavailable()
  {
    Log("RewardedVideoOnAdUnavailable");
    IronSource.Agent.loadRewardedVideo();
  }
  void RewardedVideoOnAdShowFailedEvent(IronSourceError error, IronSourceAdInfo adInfo)
  {
    Log("RewardedVideoOnAdShowFailedEvent" + " " + error.getDescription());
  }
  void RewardedVideoOnAdRewardedEvent(IronSourcePlacement placement, IronSourceAdInfo adInfo)
  {
    Log("RewardedVideoOnAdRewardedEvent" + ":" + placement.getPlacementName());
    Ads.Rewarded.onRewarded?.Invoke(placement.getPlacementName());
  }
  void RewardedVideoOnAdClickedEvent(IronSourcePlacement placement, IronSourceAdInfo adInfo)
  {
    Log("RewardedVideoOnAdClickedEvent");
    Ads.Rewarded.onClicked?.Invoke(placement.getPlacementName());
  }

  void InterstitialOnAdReadyEvent(IronSourceAdInfo adInfo)
  {
    Log("InterstitialOnAdReadyEvent");
    Ads.Interstitial.onAvailable?.Invoke();
  }
  void InterstitialOnAdLoadFailed(IronSourceError error)
  {
    Log("InterstitialOnAdLoadFailed" + " " + error.getDescription());
  }
  void InterstitialOnAdOpenedEvent(IronSourceAdInfo adInfo)
  {
    Log("InterstitialOnAdOpenedEvent");
  }
  void InterstitialOnAdClickedEvent(IronSourceAdInfo adInfo)
  {
    Log("InterstitialOnAdClickedEvent");
    Ads.Interstitial.onClicked?.Invoke(adInfo.instanceName);
  }
  void InterstitialOnAdShowSucceededEvent(IronSourceAdInfo adInfo)
  {
    Log("InterstitialOnAdShowSucceededEvent");
  }
  void InterstitialOnAdShowFailedEvent(IronSourceError error, IronSourceAdInfo adInfo)
  {
    Log("InterstitialOnAdShowFailedEvent" + " " + error.getDescription());
  }
  void InterstitialOnAdClosedEvent(IronSourceAdInfo adInfo)
  {
    Log("InterstitialOnAdClosedEvent");
    IronSource.Agent.loadInterstitial();
    Ads.Interstitial.onClosed?.Invoke(adInfo.instanceName);
  }

  void BannerOnAdLoadedEvent(IronSourceAdInfo adInfo)
  {
    Debug.Log("BannerOnAdLoadedEvent With AdInfo " + adInfo.ToString());
    Ads.Banner.onLoaded?.Invoke();
  }
  void BannerOnAdLoadFailedEvent(IronSourceError ironSourceError)
  {
    Debug.Log("BannerOnAdLoadFailedEvent With Error " + ironSourceError.ToString());
  }
  void BannerOnAdClickedEvent(IronSourceAdInfo adInfo)
  {
    Debug.Log("BannerOnAdClickedEvent With AdInfo " + adInfo.ToString());
  }
  void BannerOnAdScreenPresentedEvent(IronSourceAdInfo adInfo)
  {
    Debug.Log("BannerOnAdScreenPresentedEvent With AdInfo " + adInfo.ToString());
  }
  void BannerOnAdScreenDismissedEvent(IronSourceAdInfo adInfo)
  {
    Debug.Log("BannerOnAdScreenDismissedEvent With AdInfo " + adInfo.ToString());
  }
  void BannerOnAdLeftApplicationEvent(IronSourceAdInfo adInfo)
  {
    Debug.Log("BannerOnAdLeftApplicationEvent With AdInfo " + adInfo.ToString());
  }

  static void Log(string str) => Debug.Log("<color=green>ADS_IS:" + str + "</color>");

#endif
}
