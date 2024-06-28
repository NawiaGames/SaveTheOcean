using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPLbl = TMPro.TextMeshProUGUI;

public class UIFakeAds : AdsProvider
{
  [SerializeField] GameObject _rewarded;
  [SerializeField] GameObject _interst;
  [SerializeField] TMPLbl     _lblRewarded;
  [SerializeField] TMPLbl     _lblInterst;
  [Header("Test")]
  [SerializeField] bool _intersAvail;
  [SerializeField] bool _rewardAvail;

  string _rewardPlacement = "";
  string _intersPlacement = "";

  void Awake()
  {
    _interst.SetActive(false);
    _rewarded.SetActive(false);
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
  public void OnInterstClose()
  {
    _interst.SetActive(false);
    Ads.Interstitial.onClosed?.Invoke(_intersPlacement);
  }
}
