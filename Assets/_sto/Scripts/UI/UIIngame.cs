using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPLbl = TMPro.TextMeshProUGUI;
using GameLib.UI;

public class UIIngame : MonoBehaviour
{
  [Header("Refs")]
  [SerializeField] Slider     _progress;
  //[SerializeField] UIPanel    _topPanel;
  //[SerializeField] TMPLbl     _score;
  //[SerializeField] TMPLbl     _lblLevelInfo;

  float _pollution = 0;
  float _pollutionDest = 0;
  Level _lvl = null;

  void Awake()
  {
    Game.onLevelRestart += OnLevelRestart;
    Level.onCreate += OnLevelCreated;
    Level.onFinished += OnLevelFinished;
    Level.onStart += OnLevelStart;
    //Level.onHide += OnLevelHide;
    //Level.onTutorialStart += OnTutorialStart;
    Level.onDestroy += OnLevelDestroy;
    Level.onGarbageOut += OnLevelGarbageOut;
  }
  void OnDestroy()
  {
    Game.onLevelRestart -= OnLevelRestart;
    Level.onCreate -= OnLevelCreated;
    Level.onFinished -= OnLevelFinished;
    Level.onStart -= OnLevelStart;
    //Level.onHide -= OnLevelHide;
    //Level.onTutorialStart -= OnTutorialStart;
    Level.onDestroy -= OnLevelDestroy;
    Level.onGarbageOut -= OnLevelGarbageOut;
  }

  public void Show(Level level)
  {
    GetComponent<UIPanel>()?.ActivatePanel();
  }
  void Hide()
  {
    GetComponent<UIPanel>()?.DeactivatePanel();
  }

  void OnLevelCreated(Level lvl)
  {
    _lvl = lvl;
  }

  void OnLevelStart(Level lvl)
  {
    _lvl = lvl;
    //_lblLevelInfo.text = "Level " + (lvl.levelIdx + 1);
    _progress.minValue = 0;
    _progress.maxValue = 1;
    _progress.value = 0;
    _pollution = 0;
    _pollutionDest = 0;
    
    UpdateScore();
    Show(lvl);
  }
  void OnLevelRestart(Level lvl)
  {
    Hide();
  }
  // void OnLevelHide(Level lvl)
  // {
    
  // }
  void OnLevelFinished(Level lvl)
  {
    Hide();
  }
  public void SetLevel(Level lvl)
  {
    _lvl = lvl;
  }
  void UpdateScore()
  {
    //_score.text = ""
  }
  void OnLevelDestroy(Level lvl)
  {
    _lvl = null;
  }
  void OnLevelGarbageOut(Level lvl)
  {
    _pollutionDest = lvl.PollutionRate();
  }
  // void OnTutorialStart(Level lvl)
  // {

  // }
  public void OnBtnRestart()
  {
    FindObjectOfType<Game>()?.RestartLevel();
  }
  public void OnBtnQuit()
  {
    _lvl?.Quit();
    Hide();
    FindObjectOfType<Game>().ShowEarth(false);
  }

  void Update()
  {
    if(_pollution < _pollutionDest)
    {
      _pollution = Mathf.Lerp(_pollution, _pollutionDest, Time.deltaTime * 2);
      _progress.value = Mathf.Clamp01(_pollution);
    }
  }

  public void ShowMergeInfoWindow(){
    FindObjectOfType<UIItemsInfo>().Show();
  }
}
