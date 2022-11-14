using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPLbl = TMPro.TextMeshProUGUI;
using GameLib.UI;

public class UIEarth : MonoBehaviour
{
  [SerializeField] Button  _btnPlay;
  [SerializeField] Button  _btnFeed;
  [SerializeField] TMPLbl  _lblLevelInfo;
  [SerializeField] Slider  _slider;
  [SerializeField] TMPLbl  _btnActionInfo;
  [SerializeField] TMPLbl  _btnStaminaInfo;

  public static System.Action<Level.Mode> onBtnPlay;

  Earth   _earth = null;
  UIPanel _earthPanel = null;
  float   _cleanDst = 0.0f;

  void Awake()
  {
    Earth.onShow += OnEarthShow;
    Earth.onLevelSelected += UpdateLevelInfo;
    Earth.onLevelStart += OnEarthHide;

    _earth = FindObjectOfType<Earth>(true);
    _earthPanel = GetComponent<UIPanel>();

    _cleanDst = GameState.Progress.Locations.GetCompletionRate();
    _slider.minValue = 0;
    _slider.maxValue = 1;
    _slider.value = _cleanDst;

    _btnStaminaInfo.text = UIDefaults.GetStaminaString(GameData.Econo.staminaPlayCost);
  }
  void OnDestroy()
  {
    Earth.onShow -= OnEarthShow;
    Earth.onLevelSelected -= UpdateLevelInfo;
    Earth.onLevelStart -= OnEarthHide;
  }
  private void OnEarthShow(Earth earth) => Show(earth.selectedLocation);
  private void OnEarthHide(int idx) => Hide();
  public void  Show(int lvlIdx)
  {
    _earthPanel.ActivatePanel();
    UpdateLevelInfo(lvlIdx);
    this.Invoke(()=> _cleanDst = GameState.Progress.Locations.GetCompletionRate(), 0.25f);
  }
  void Hide()
  {
    _earthPanel.DeactivatePanel();
  }  

  bool IsLocationSelectable(int location)
  {
    var state = GameState.Progress.Locations.GetLocationState(location);
    return (state != Level.State.Locked && state != Level.State.Finished);
  }
  void UpdateLevelInfo(int location)
  {
    _btnPlay.interactable = IsLocationSelectable(location);
    _lblLevelInfo.text = "LEVEL " + (location + 1);
    var state = GameState.Progress.Locations.GetLocationState(location);
    if(state == Level.State.Feeding)
    {
      _btnActionInfo.text = "Feed";
      _btnStaminaInfo.text = UIDefaults.GetStaminaString(GameData.Econo.staminaPlayFeedCost);
    }
    else if(state == Level.State.Clearing)
    {
      _btnActionInfo.text = "Clearing";
      _btnStaminaInfo.text = UIDefaults.GetStaminaString(GameData.Econo.staminaPlayClearCost);
    }
    else
    {
      _btnActionInfo.text = "Play";
      _btnStaminaInfo.text = "";//UIDefaults.GetStaminaString(GameData.Econo.staminaPlayCost);
    }
  }

  bool IsMode(int loc_idx, Level.Mode mode)
  {
    if(mode == Level.Mode.Standard || mode == Level.Mode.Polluted)
      return loc_idx < Location.SpecialLocBeg;
    if(mode == Level.Mode.Feeding)  
      return loc_idx == Location.FeedLocation;
    if(mode == Level.Mode.Clearing)
      return loc_idx == Location.ClearLocation;
    
    return false;    
  }
  public void OnBtnPlay()
  {
    (int cost, Level.Mode mode)[] modes = 
    {
      new (GameData.Econo.staminaPlayCost * 0, Level.Mode.Standard),
      new (GameData.Econo.staminaPlayFeedCost, Level.Mode.Feeding),
      new (GameData.Econo.staminaPlayClearCost, Level.Mode.Clearing),
    };
    for(int q = 0; q < modes.Length; ++q)
    {
      if(IsMode(GameState.Progress.locationIdx, modes[q].mode))
      {
        if(GameState.Econo.CanSpendStamina(modes[q].cost))
        {
          GameState.Econo.stamina -= modes[q].cost;
          onBtnPlay?.Invoke(modes[q].mode);
        }
        else
          FindObjectOfType<UIPopupStamina>(true)?.Show();
      }
    }
  }

  void UpdateSlider()
  {
    if(_earthPanel?.IsActive ?? false)
       _slider.value = Mathf.Lerp(_slider.value, _cleanDst, Time.deltaTime * 2.0f);
  }
  void Update()
  {
    UpdateSlider();
  }
}
