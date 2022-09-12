using System;
using UnityEngine;

public class Location : MonoBehaviour
{
  [SerializeField] GameObject[] _stateModels;
  [SerializeField] GameObject   _selectionModel;
  [SerializeField] Level.State  _state = Level.State.Locked;

  [Header("IngameLevel")]
  [SerializeField] int _level = -1;

  Quaternion _localDstRoto = Quaternion.identity;
  private int _idx = -1;

  public Quaternion localDstRoto => _localDstRoto;
  public int  idx => _idx;
  public int  levelIdx => _level;

  public void Init(int idx, Transform levelTransf, float vert_roto_range, Level.State level_state)
  { 
    _idx = idx;
    _level = Mathf.Clamp(idx, 0, GameData.Levels.levelsCnt-1);
    state = level_state;

    vert_roto_range -= 5;

    transform.localPosition = levelTransf.localPosition;
    transform.localRotation = Quaternion.LookRotation(-levelTransf.localPosition) * Quaternion.AngleAxis(-90, Vector3.right);
    var posxz = Vector3.ProjectOnPlane(transform.localPosition, Vector3.up);
    //posxz.y = 0;
    _localDstRoto = Quaternion.AngleAxis(Vector3.SignedAngle(posxz, -Vector3.forward, Vector3.up), Vector3.up);
    var posyz = Vector3.ProjectOnPlane(transform.localPosition, Vector3.right);
    //posyz.x = 0;
    _localDstRoto = Quaternion.AngleAxis(Mathf.Clamp(Vector3.SignedAngle(posyz, -Vector3.forward, Vector3.right), -vert_roto_range, vert_roto_range), Vector3.right) * _localDstRoto;

    Select(false);
  }
  public Level.State state 
  {
    get => _state;
    set
    {
      _state = value;
      SetStateModel(_state);
    }
  }
  public void Select(bool sel)
  {
    _selectionModel.SetActive(sel);
  }

  int  State2MI(Level.State state) => (int)state;
  void SetStateModel(Level.State state)
  {
    int mi = State2MI(state);
    for(int q = 0; q < _stateModels.Length; ++q)
    {
      if(_stateModels[q] != null)
        _stateModels[q]?.SetActive(q == mi);
    }
  }
}
