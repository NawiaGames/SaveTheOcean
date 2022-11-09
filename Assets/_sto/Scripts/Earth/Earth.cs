using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLib.InputSystem;

[DefaultExecutionOrder(-1)]
public class Earth : MonoBehaviour
{
  [Header("Refs")]
  [SerializeField] GameObject     _earthPrefab;
  [SerializeField] GameObject     _locationsPath;
  [SerializeField] Transform      _levelsContainer;
  [SerializeField] Transform      _locationsContainer;
  [SerializeField] Transform      _fx;
  [SerializeField] Transform      _extras;

  [Header("SpecialLocs")]
  [SerializeField] Transform _feedingTransform;
  [SerializeField] Transform _cleanupTransform;

  [Header("Earth fx")]
  [SerializeField] EarthFx      _earthFx;

  [Header("Vessel")]
  [SerializeField] Vessel _vessel;

  [Header("Rotate params")]
  [SerializeField] float _rotateDragDegrees = 180.0f;
  [SerializeField] float _rotateMax = 720;
  [SerializeField] float _rotateToLocationSpeed = 5.0f;
  [SerializeField] float _rotateDamping = 0;
  [SerializeField] float _rotateXRange = 45;

  public static System.Action<Earth> onShow;
  public static System.Action        onHide;
  public static System.Action<int>   onLevelStart, onLevelSelected, onLocationFocused;
  public static System.Action        onAllLocationFinished;

  public static int locationsCnt { get; private set; }

  int            _selectedLocation = 0;
  Vector2        _vrotateSpeed = Vector2.zero;
  Vector2?       _vdragBeg = null;
  Vector2        _vdragPrev;
  bool           _move2location = false;
  Location[]     _locations;
  Location       _feedLocation;
  Location       _clearupLocation;
  Location       _miscLocation;
  bool           _showAllLocationTut = false;
  bool           _showFeedingLoc = false;
  bool           _showClearingLoc = false;

  public int      selectedLocation => _selectedLocation;
  public Location location(int idx) => idx switch 
  {

    Location.FeedLocation => _feedLocation,
    Location.ClearLocation => _clearupLocation,
    -1 => null,
    _ => _locations[idx]
  };
 

  void Awake()
  {
    InitLocations();
    GameState.Progress.Locations.onLocationPolluted += OnLocationPolluted;
    GameState.Progress.Locations.onAllLocationFinished += OnAllLocationFinished;
    UIEarth.onBtnPlay += OnBtnPlay;
  }
  void OnDestroy()
  {
    GameState.Progress.Locations.onLocationPolluted -= OnLocationPolluted;
    GameState.Progress.Locations.onAllLocationFinished -= OnAllLocationFinished;
    UIEarth.onBtnPlay -= OnBtnPlay;
  }

  private void InitLocations()
  {
    List<Location> listLocations = new List<Location>();
    for(int q = 0; q < _levelsContainer.childCount; ++q)
    {
      var levelTransf = _levelsContainer.GetChild(q);
      if(levelTransf.gameObject.activeSelf)
      {
        var loc = GameData.Prefabs.CreateLocation(_locationsContainer);
        if(loc)
        {
          var st = GameState.Progress.Locations.GetLocationState(listLocations.Count);
          loc.Init(listLocations.Count, levelTransf, _rotateXRange, st);
          listLocations.Add(loc);
        }
      }
    }
    _locations = listLocations.ToArray();
    locationsCnt = listLocations.Count;

    _feedLocation = GameData.Prefabs.CreateLocation(_locationsContainer);
    _feedLocation.Init(Location.FeedLocation, _feedingTransform, _rotateXRange, Level.State.Feeding);
    _feedingTransform.gameObject.SetActive(false);

    _clearupLocation = GameData.Prefabs.CreateLocation(_locationsContainer);
    _clearupLocation.Init(Location.ClearLocation, _cleanupTransform, _rotateXRange, Level.State.Clearing);
    _cleanupTransform.gameObject.SetActive(false);

    _locations = listLocations.ToArray();
    _locationsPath.SetActive(!GameState.Progress.Locations.AllStateFinished());
  }
  public void Setup()
  {
    _selectedLocation = GameState.Progress.locationIdx;
    if(_selectedLocation < 0)
    {
      SelectLocation(0);
      _vessel.Init(Vector3.up);
      _vessel.transform.localRotation = Quaternion.AngleAxis(0, _vessel.vpos - _earthPrefab.transform.position);
      _fx.transform.localRotation = location(_selectedLocation).localDstRoto;
      this.Invoke(()=>MoveVesselToLocation(location(_selectedLocation)), 0.5f);
    }
    else
    {
      SelectLocation(_selectedLocation);
      _vessel.Init(location(_selectedLocation).transform.localPosition);
      _fx.transform.localRotation = location(_selectedLocation).localDstRoto;
    }
    _earthPrefab.SetActive(true);
    UpdateLevelsStates();
    onShow?.Invoke(this);
  }
  public void Show(int indexLocation, bool show_next)
  {
    SelectLocation(indexLocation);
    _fx.gameObject.SetActive(true);
    _extras.gameObject.SetActive(true);

    _locationsPath.SetActive(!GameState.Progress.Locations.AllStateFinished());

    UpdateLevelsStates();
    int location_idx = indexLocation;
    if(_showFeedingLoc)
      location_idx = Location.FeedLocation;
    else if(_showClearingLoc)
      location_idx = Location.ClearLocation;
    else
      location_idx = (show_next)? GetNextLocation(indexLocation) : indexLocation;
    this.Invoke(()=> 
    {
      SelectLocation(location_idx); 
      StartRotateToLocation(location(location_idx));
      if(show_next)
        MoveVesselToLocation(location(location_idx));

      onShow?.Invoke(this);

      if(_showAllLocationTut)
      {
        this.Invoke(() => onAllLocationFinished?.Invoke(), 1.0f);
        _showAllLocationTut = false;
      }      
    }, 1.0f);
  }
  public void Hide()
  {
    onHide?.Invoke();
    _fx.gameObject.SetActive(false);
    _extras.gameObject.SetActive(false);
  }
  public int  GetLevel(int locationIdx) => location(locationIdx).levelIdx;

  public void OnInputBeg(TouchInputData tid)
  {
    if(!_earthPrefab.activeInHierarchy)
      return;

    _vdragBeg = tid.InputPosition;
    _vdragPrev = _vdragBeg.Value;
    _vrotateSpeed = Vector2.zero;
    _move2location = false;
  }
  public void OnInputMov(TouchInputData tid)
  {
    if(!_earthPrefab.activeInHierarchy)
      return;

    if(_vdragBeg != null)
    {
      var moveDist = tid.InputPosition - _vdragPrev;
      _vrotateSpeed.x = Mathf.Clamp(moveDist.x * _rotateDragDegrees, -_rotateMax, _rotateMax);
      _vrotateSpeed.y = Mathf.Clamp(moveDist.y * _rotateDragDegrees, -_rotateMax, _rotateMax);
      _vdragPrev = tid.InputPosition;
    }
  }  
  public void OnInputEnd(TouchInputData tid)
  {
    if(!_earthPrefab.activeInHierarchy)
      return;

    if(tid.HoveredCollider && Mathf.Abs(tid.InputPosition.x - _vdragBeg.Value.x) < 0.05f)
    {
      var location = tid.HoveredCollider.GetComponentInParent<Location>();
      if(location && location.transform.position.z < 0)
      {
        if(location.IsSelectable())
        {
          SelectLocation(location);
          StartRotateToLocation(location);
          MoveVesselToLocation(location);
        }
      }
    }
    _vdragBeg = null;
  }
  void OnBtnPlay(Level.Mode mode)
  {
    Level.mode = mode;
    onLevelStart?.Invoke(_selectedLocation);
  }
  void OnAllLocationFinished()
  {
    _showAllLocationTut = true;
  }
  void OnLocationPolluted(int locationIdx)
  {
    location(locationIdx).state = GameState.Progress.Locations.GetLocationState(locationIdx);
  }  
  void UpdateLevelsStates()
  {
    for(int q = 0; q < _locations.Length; ++q)
      _locations[q].state = GameState.Progress.Locations.GetLocationState(q);
    
    _feedLocation.state = Level.State.Feeding;
    _clearupLocation.state = Level.State.Clearing;

    _showFeedingLoc = GameState.Progress.Locations.GetFinishedCnt() == GameData.Levels.GetFeedingAvailLoc();
    _showClearingLoc = GameState.Progress.Locations.GetFinishedCnt() == GameData.Levels.GetClearingAvailLoc();

    _feedLocation.gameObject.SetActive(GameState.Progress.Locations.GetFinishedCnt() >= GameData.Levels.GetFeedingAvailLoc());
    _clearupLocation.gameObject.SetActive(GameState.Progress.Locations.GetFinishedCnt() >= GameData.Levels.GetClearingAvailLoc());
  }
  int  GetNextLocation(int loc)
  {
    if(loc >= Location.FeedLocation)
      return loc;
    else  
      return Mathf.Clamp(loc + 1, 0, locationsCnt-1);  
  }
  void SelectLocation(Location location) => SelectLocation(location.idx);
  void SelectLocation(int loc)
  {
    location(_selectedLocation)?.Select(false);
    if(loc >= 0)
    {
      location(loc)?.Select(true);
      _selectedLocation = loc;
      GameState.Progress.locationIdx = loc;
      onLevelSelected?.Invoke(loc);
    }
  }
  void MoveVesselToLocation(Location loca)
  {
    _vessel.FlyTo(loca.transform.localPosition);
  }
  
  void StartRotateToLocation(Location location)
  {
    _move2location = true;
  }
  void RotateToLocation()
  {
    if(_move2location)
    {
      _vrotateSpeed = Vector2.zero;
      var rotDest = location(_selectedLocation).localDstRoto;
      _fx.transform.localRotation = Quaternion.Slerp(_fx.transform.localRotation, rotDest, _rotateToLocationSpeed * Time.deltaTime);
      if(Mathf.Abs(Quaternion.Angle(_fx.transform.localRotation, rotDest)) < 0.01f)
      {
        _move2location = false;
        onLocationFocused?.Invoke(_selectedLocation);
      }
    }
  }
  void RotateFree()
  {
    _vrotateSpeed *= Mathf.Pow(_rotateDamping, TimeEx.deltaTimeFrame);
    float angle = Vector3.SignedAngle(Vector3.ProjectOnPlane(_fx.transform.localRotation * Vector3.up, Vector3.right), Vector3.up, Vector3.right);
    if(angle <= -_rotateXRange)
    {
      if(_vrotateSpeed.y >= 0)
        _vrotateSpeed.y = -0.1f;
    }
    else if(angle >= _rotateXRange)
    {
      if(_vrotateSpeed.y <= 0)
        _vrotateSpeed.y = 0.1f;
    }
    var qpreR = _fx.transform.localRotation;
    _fx.transform.localRotation = Quaternion.AngleAxis(_vrotateSpeed.y, Vector3.right) * _fx.transform.localRotation;
    _fx.transform.localRotation *= Quaternion.AngleAxis(-_vrotateSpeed.x, Vector3.up);

    _earthFx?.RotoSpeed(_vrotateSpeed.x);
    //_earthFx.RotoParams(_fx.transform.localRotation, _vrotateSpeed.magnitude);
  }

  void Update()
  {
    RotateToLocation();
    RotateFree();

  #if UNITY_EDITOR
    if(Input.GetKeyDown(KeyCode.Y))
    {
      for(int q = 0; q < Earth.locationsCnt - 1; ++q)
      {
        GameState.Progress.Locations.SetLocationFinished(q);
      }
      GameState.Progress.Locations.SetLocationUnlocked(Earth.locationsCnt-1);
      UpdateLevelsStates();
    }
  #endif
  }
}
