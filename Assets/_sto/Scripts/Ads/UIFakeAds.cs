using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPLbl = TMPro.TextMeshProUGUI;

public class UIFakeAds : AdsProvider
{
  [SerializeField] GameObject _rewarded;
  [SerializeField] GameObject _interst;
  [SerializeField] GameObject _banner;
  [SerializeField] GameObject _bannerTop, _bannerBtm;

  [SerializeField] TMPLbl     _lblRewarded;
  [SerializeField] TMPLbl     _lblInterst;
  //[SerializeField] TMPLbl     _lblBanner;
  [Header("Test")]
  [SerializeField] bool _intersAvail;
  [SerializeField] bool _rewardAvail;
  [SerializeField] bool _bannerAvail;

  string _rewardPlacement = "";
  string _intersPlacement = "";
  string _bannerPlacement = "";

  bool   _bannerLoaded = false;

  void Awake()
  {
    _interst.SetActive(false);
    _rewarded.SetActive(false);
    _banner.SetActive(false);
  }

  public override bool IntersIsAvailable(string placement = null) => _intersAvail;
  public override void IntersShow(string placement)
  {
    if(_intersAvail)
    {
      _intersPlacement = placement;
      _lblInterst.text = $"Close Interstitial [{placement}]";
      _interst.SetActive(true);
    }
  }
  public void OnInterstClose()
  {
    _interst.SetActive(false);
    Ads.Interstitial.onClosed?.Invoke(_intersPlacement);
  }

  public override bool RewardedIsAvailable(string placement = null) => _rewardAvail;
  public override void RewardedShow(string placement)
  {
    if(_rewardAvail)
    {
      _rewardPlacement = placement;
      _lblRewarded.text = $"Get Reward [{placement}]";
      _rewarded.SetActive(true);
    }
  }
  public void OnRewardClose()
  {
    _rewarded.SetActive(false);
    Ads.Rewarded.onRewarded?.Invoke(_rewardPlacement);
  }

  public override bool BannerIsLoaded(string placement) => _bannerLoaded;
  public override void BannerLoad(string placement, bool force)
  {
    if(_bannerAvail)
    {
      _bannerPlacement = placement;
      //_lblBanner.text = $"banner [{_bannerPlacement}]";
      //_banner
      _bannerTop.SetActive(Ads.Banner.IsOnTopEdge);
      _bannerBtm.SetActive(Ads.Banner.IsOnBtmEdge);
      var bannerSize = Ads.Banner.GetSizePx(Ads.Banner.size);
      _bannerTop.GetComponent<RectTransform>().sizeDelta = new Vector2(_bannerTop.GetComponent<RectTransform>().sizeDelta.x, bannerSize);
      _bannerBtm.GetComponent<RectTransform>().sizeDelta = new Vector2(_bannerBtm.GetComponent<RectTransform>().sizeDelta.x, bannerSize);
      Ads.Banner.Loaded(true);
      _bannerLoaded = true;
    }
  }
  public override void BannerShow(){if(_bannerLoaded)_banner.SetActive(true);}
  public override void BannerHide(){if(_bannerLoaded)_banner.SetActive(false);}
}
