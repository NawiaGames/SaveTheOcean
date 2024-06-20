using System;
using UnityEngine;

public class Location : MonoBehaviour
{
  [SerializeField] Marker[]     _stateModels;
  [SerializeField] GameObject   _selectionModel;
  [SerializeField] Level.State  _state = Level.State.Locked;

  [Header("IngameLevel")]
  [SerializeField] int _level = -1;

  [System.Serializable]
  class Marker
  {
    [SerializeField] string name;
    public GameObject mdl;
  }

  public const int SpecialLocBeg = 1000;
  public const int FeedLocation = 1000;
  public const int ClearLocation = 1001;


  Quaternion _localDstRoto = Quaternion.identity;
  private int _idx = -1;

  public Quaternion localDstRoto => _localDstRoto;
  public int  idx => _idx;
  public int  levelIdx => _level;

  public bool IsSelectable()
  {
    state = GameState.Progress.Locations.GetLocationState(idx);
    return state >= Level.State.Unlocked && state != Level.State.Finished;
  }
  public void Init(int idx, Transform levelTransf, float vert_roto_range, Level.State level_state)
  {
    _idx = idx;
    _level = idx;
    state = level_state;

    //vert_roto_range -= 5;

    transform.localPosition = levelTransf.localPosition;
    transform.localRotation = Quaternion.LookRotation(-levelTransf.localPosition) * Quaternion.AngleAxis(-90, Vector3.right);

    var vnp = transform.localPosition.normalized;
    var posxz = Vector3.ProjectOnPlane(vnp, Vector3.up);
    var ang_y = Vector3.SignedAngle(posxz.normalized, -Vector3.forward, Vector3.up);
    //var ang_y = Mathf.Atan2(vnp.z, vnp.x) * Mathf.Rad2Deg + 90;

    var posyz = Vector3.ProjectOnPlane(vnp, -Vector3.right);
    var ang_x = Vector3.SignedAngle(posyz.normalized, -Vector3.forward, Vector3.right);
    if(ang_x > 90)
      ang_x = 180 - ang_x;
    else if(ang_x < -90)
      ang_x = -180 - ang_x;
    ang_x = Mathf.Clamp(ang_x, -vert_roto_range, vert_roto_range);
    //var ang_xx = Mathf.Asin(vnp.y) * Mathf.Rad2Deg;

    _localDstRoto = Quaternion.AngleAxis(ang_x, Vector3.right) * Quaternion.AngleAxis(ang_y, Vector3.up);

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
      if(_stateModels[q].mdl != null)
        _stateModels[q].mdl?.SetActive(false);
    }
    _stateModels[mi].mdl.SetActive(true);
  }
}
