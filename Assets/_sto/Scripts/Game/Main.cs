using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLib.DataSystem;

[DefaultExecutionOrder(-1)]
public class Main : MonoBehaviour
{
  [Header("DataRefs")]
  [SerializeField] GameState  gameState;
  [SerializeField] GameData   gameData;

  [Header("Debugparams")]
  [SerializeField] int       framerate = 60;
  
  void Awake()
  {
    DataManager.LoadAllData();
    GameData.Init();
    GameState.Init();
  }
  void OnApplicationPause(bool paused)
  {
    if(paused)
    {
      GameState.GameInfo.appQuitTime = CTime.get().ToBinary();
      DataManager.SaveAllData();
    }
  }
  void OnApplicationQuit()
  {
    GameState.GameInfo.appQuitTime = CTime.get().ToBinary();
    DataManager.SaveAllData();
  }

  void OnValidate()
  {
    Application.targetFrameRate = framerate;
  }
}
