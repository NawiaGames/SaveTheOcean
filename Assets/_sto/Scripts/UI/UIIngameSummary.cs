using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLib.UI;

public class UIIngameSummary : MonoBehaviour
{
  public static System.Action onShow;

  void Awake()
  {
    Level.onSublocCleared += Show;
  }
  void OnDestroy()
  {
    Level.onSublocCleared -= Show;
  }

  public void Show(Level level)
  {
    onShow?.Invoke();
    GetComponent<UIPanel>().ActivatePanel();
  }
  void Hide()
  {
    GetComponent<UIPanel>().DeactivatePanel();
  }

  public void OnBtnClicked()
  {
    Hide();
  }  
}
