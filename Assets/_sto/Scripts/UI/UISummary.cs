using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameLib.UI;
using TMPLbl = TMPro.TextMeshProUGUI;

public class UISummary : MonoBehaviour
{
  public static System.Action onShow, onBtnPlay;

  bool _moveToNextLoc = false;
  public void Show(Level level)
  {
    _moveToNextLoc = level.isRegular && GameState.Progress.Locations.IsLocationFinished(level.locationIdx);
    onShow?.Invoke();
    GetComponent<UIPanel>().ActivatePanel();
  }
  void Hide()
  {
    GetComponent<UIPanel>().DeactivatePanel();
  }
  public void OnBtnRestart()
  {
    Hide();
    FindObjectOfType<Game>().RestartLevel();
  }
  public void OnBtnPlay()
  {
    Hide();
    onBtnPlay?.Invoke();
    FindObjectOfType<Game>().ShowEarth(_moveToNextLoc);
  }
}
