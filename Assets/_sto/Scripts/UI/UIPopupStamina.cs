using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPLbl = TMPro.TextMeshProUGUI;
using GameLib.UI;

public class UIPopupStamina : MonoBehaviour
{
  [SerializeField] Transform  _content;
  [SerializeField] TMPLbl     _lblFreeReward;
  [SerializeField] TMPLbl     _lblAdReward;
  [SerializeField] TMPLbl     _lblGemsReward;
  [SerializeField] UIPanel    _freeStaminaPanel;
  [SerializeField] UIPanel    _adStaminaPanel;
  [SerializeField] UIPanel    _gemsStaminaPanel;
  [SerializeField] TMPLbl     _lblGemsCost;

  bool _showAd = false;
  void Awake()
  {
    UnityAdsRewarded.onCompleted += OnRewardedComplete;
    Level.onNoStaminaToPushout += Show;
  }
  void OnDestroy()
  {
    UnityAdsRewarded.onCompleted -= OnRewardedComplete;
    Level.onNoStaminaToPushout -= Show;
  }
  public void Show()
  {
    GetComponent<UIPanel>()?.ActivatePanel();
    if(GameState.Events.Popups.noStaminaShown == 0)
    {
      _lblFreeReward.text = "+" + UIDefaults.GetStaminaString(GameData.Econo.staminaAdReward);
      _freeStaminaPanel.ActivatePanel();
    }
    else if(UnityAdsRewarded.IsReady())
    {
      _lblAdReward.text = "+" + UIDefaults.GetStaminaString(GameData.Econo.staminaAdReward);
      _adStaminaPanel.ActivatePanel();
    }
    else
    {
      _lblGemsReward.text = "+" + UIDefaults.GetStaminaString(GameData.Econo.staminaAdReward);
      _lblGemsCost.text = UIDefaults.GetGemsString(GameData.Econo.staminaRefillGemsCost);
      _gemsStaminaPanel.ActivatePanel();
    }
  }
  public void Hide()
  {
    GetComponent<UIPanel>()?.DeactivatePanel();
  }

  void OnRewardedComplete(string adId)
  {
    AddStamina();
  }
  void AddStamina()
  {
    GameState.Econo.stamina += GameData.Econo.staminaAdReward;
    GameState.Events.Popups.noStaminaShown++;
  }
  public void OnBtnFreeClick()
  {
    AddStamina();
    Hide();
  }
  public void OnBtnAdClick()
  {
    UnityAdsRewarded.Show();
    Hide();
  }
  public void OnBtnGemsClick()
  {
    GameState.Econo.gems -= GameData.Econo.staminaRefillGemsCost;
    AddStamina();
    Hide();
  }
}
