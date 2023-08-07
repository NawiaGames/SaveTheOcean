using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLib;
using GameLib.InputSystem;
using Cinemachine;

public class Game : MonoBehaviour
{
  [SerializeField] Transform levelsContainer;
  [SerializeField] CamCtlr _camCtrl = null;

  public static System.Action<Level> onLevelRestart;

  Level   _level = null;
  Earth   _earth = null;
  UIFade  _uiFade = null;

	void Awake()
  {
    TouchInputData.onTap += OnInputTapped;
    TouchInputData.onInputStarted += OnInputBeg;
    TouchInputData.onInputMoveStarted += OnInputStart;
    TouchInputData.onInputUpdated += OnInputMov;
    TouchInputData.onInputEnded += OnInputEnd;

    Earth.onLevelStart += ShowLevel;

    _earth = FindObjectOfType<Earth>(true);
    _uiFade = FindObjectOfType<UIFade>(true);
  }
  void OnDestroy()
  {
    TouchInputData.onTap -= OnInputTapped;
    TouchInputData.onInputStarted -= OnInputBeg;
    TouchInputData.onInputMoveStarted -= OnInputStart;
    TouchInputData.onInputUpdated -= OnInputMov;
    TouchInputData.onInputEnded -= OnInputEnd;

    Earth.onLevelStart -= ShowLevel;
  }
  IEnumerator Start()
  {
    yield return new WaitForSeconds(0.0125f);
    _earth.Setup();
    FindObjectOfType<UIStatusBar>(true).Show();
  }

  void OnInputTapped(TouchInputData tid)
  {
    _level?.OnInputTapped(tid);
  }
  int _inputID = -1;
  void OnInputBeg(TouchInputData tid)
  {
    if(_inputID < 0)
    {
      _inputID = tid.inputId;
      _earth?.OnInputBeg(tid);
    }
  }
  void OnInputStart(TouchInputData tid)
  {
    if(tid.inputId == _inputID)
      _level?.OnInputBeg(tid);
  }
  void OnInputMov(TouchInputData tid)
  {
    if(tid.inputId == _inputID)
    {
      _level?.OnInputMov(tid);
      _earth?.OnInputMov(tid);
    }
  }
  void OnInputEnd(TouchInputData tid)
  {
    if(tid.inputId == _inputID)
    {
      _level?.OnInputEnd(tid);
      _earth?.OnInputEnd(tid);
      _inputID = -1;
    }
  }

  public void CreateLevel(int locationIdx)
  {
    _earth.Hide();
    if(_level)
      Destroy(_level.gameObject);
    _level = null;
    GameState.Progress.locationIdx = locationIdx;
    CreateLevel();
  }
  public void CreateLevel()
  {
    if(_level)
      Destroy(_level.gameObject);
    _level = null;

    if(Level.mode == Level.Mode.Feeding)
      _level = GameData.Levels.CreateFeedingLevel(levelsContainer);
    else if(Level.mode == Level.Mode.Clearing)
      _level = GameData.Levels.CreateClearingLevel(levelsContainer);
    else //if(Level.mode == Level.Mode.Standard || Level.mode == Level.Mode.Polluted)
      _level = GameData.Levels.CreateLevel(_earth.GetLevel(GameState.Progress.locationIdx), levelsContainer);
  }
  public void RestartLevel()
  {
    // if(_level)
    //   _level.End();
    if(_level)
      GameState.Progress.Locations.ClearCache(_level.locationIdx);
    onLevelRestart?.Invoke(_level);
    CreateLevel();
  }
  public void PrevLocation(bool create = true)
  {
    GameState.Progress.locationIdx = GameData.Locations.PrevLocation(GameState.Progress.locationIdx);
    if(create)
      CreateLevel();
  }
  public void NextLocation(bool create = true)
  {
    GameState.Progress.locationIdx = GameData.Locations.NextLocation(GameState.Progress.locationIdx);
    if(create)
      CreateLevel();
  }
  public void DestroyLevel()
  {
    if(_level)
    {
      //_level.End();
      Destroy(_level.gameObject);
    }
    _level = null;
  }
  public void ShowEarth(bool show_next)
  {
    StartCoroutine(coShowEarth(show_next));
  }
  private IEnumerator coShowEarth(bool show_next)
  {
    _level?.Hide();
    _uiFade.FadeIn(_camCtrl.zoomSpeed * 3.0f);
    yield return new WaitForSeconds(0.5f);
    DestroyLevel();
    _earth.Show(GameState.Progress.locationIdx, show_next);
    _camCtrl.SwitchToTransit();
    yield return new WaitForSeconds(0.5f);
    _uiFade.FadeOut(_camCtrl.zoomSpeed * 2f);
    _camCtrl.SwitchToGlobe();
  }
  public void ShowLevel(int locationIdx)
  {
    GameState.Progress.locationIdx = locationIdx;
    StartCoroutine(coShowLevel(locationIdx));
  }
  IEnumerator coShowLevel(int locationIdx)
  {
    _camCtrl.SwitchToTransit();
    yield return new WaitForSeconds(0.125f);
    _uiFade.FadeIn(_camCtrl.zoomSpeed * 4.0f);
    yield return new WaitForSeconds(0.5f);
    _earth.Hide();
    CreateLevel(locationIdx);
    _camCtrl.SwitchToIngame();
    yield return new WaitForSeconds(1.0f);
    _uiFade.FadeOut(_camCtrl.zoomSpeed * 2);
  }

  void Update()
  {
    GameState.Process();

#if UNITY_EDITOR
    if(Input.GetKeyDown(KeyCode.Z))
    {
      Level.onFinished?.Invoke(_level);
      PrevLocation();
    }
    else if(Input.GetKeyDown(KeyCode.X))
    {
      Level.onFinished?.Invoke(_level);
      NextLocation();
    }
    else if(Input.GetKeyDown(KeyCode.R))
    {
      Level.onFinished?.Invoke(_level);
      RestartLevel();
    }
#endif
  }
}
