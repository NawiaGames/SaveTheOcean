using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameLib;
using GameLib.InputSystem;

using GarbCats = GameData.GarbCats;

public class Level : MonoBehaviour
{
  public static System.Action<Level>   onCreate, onStart, onGarbageOut, onNoRoomOnGrid, onItemHovered, onAnimalHovered;
  public static System.Action<Level>   onDone, onFinished, onHide, onDestroy;
  public static System.Action<Vector3> onMagnetBeg;
  public static System.Action<bool>    onMagnetEnd;
  public static System.Action<Item>    onPremiumItem, onItemCollected, onItemCleared, onUnderwaterSpawn;
  public static System.Action          onNoStaminaToPushout;

  [Header("Refs")]
  [SerializeField] Transform      _itemsContainer;
  [SerializeField] Transform      _tilesContainer;
  [SerializeField] Transform[]    _animalContainers;
  [SerializeField] Renderer       _waterRenderer;
  [SerializeField] RewardChest    _rewardChest;
  [SerializeField] SplitMachine   _splitMachine;
  [SerializeField] FeedingMachine _feedingMachine;
  
  //[SerializeField] Transform[] _paths;
  //[SerializeField] Transform _poiLT;
  //[SerializeField] Transform _poiRB;

  [Header("Settings")]
  [SerializeField] Vector2Int _dim;
  [SerializeField] float      _gridSpace = 1.0f;
  [SerializeField] Color      _waterColor;
  [SerializeField] float      _inputRad = 1.5f;
  [SerializeField] float      _inputAnimRad = 2.0f;
  [Header("LvlDesc")]
  [SerializeField] int        _resItemPerItems = 0;
  [SerializeField] float      _resGemsPart = 0.1f;
  [SerializeField] float      _resCoinsPart = 0.4f;
  [SerializeField] float      _resStaminaPart = 0.5f;
  
  public enum Mode
  {
    Standard,
    Clearing,
    Feeding,
    Polluted,
  };
  static public Mode mode {get; set;} = Mode.Standard;

  public enum State
  {
    Locked,
    Unlocked,
    Started,
    Finished,
    Polluted,
    Feeding,
    Clearing,
  }

  public Transform GetPrimaryAnimalContainer() => _animalContainers.FirstOrDefault();
  public int       GetUnderwaterGarbagesCnt() => _items2.Count((item) => !item.id.IsSpecial);  

  public int      locationIdx {get; private set;} = -1;
  public bool     succeed {get; private set;}
  public bool     finished {get; private set;}
  public int      points {get; set;} = 0;
  public int      stars {get; set;}
  public int      itemsCount => _items.Count + _items2.Count;
  //public int      initialItemsCnt => _initialItemsCnt;
  public bool     hoverItemMatch = false;
  public Vector2Int dim => _dim;
  public List<Item> listItems => _items;
  public List<Item> listItems2 => _items2;
  public List<Animal> animals => _animals;

  public int         garbagesCapacity(int garb_type) => _garbagesCapacity[garb_type];
  public int         garbagesCleared(int garb_type) => _garbagesCleared[garb_type];
  public int         totalGarbagesCapacity => _garbagesCapacity.Sum();
  public int         totalGarbagesCleared => _garbagesCleared.Sum();
  
  public bool isRegular => locationIdx < Location.SpecialLocBeg && !isPolluted;
  public bool isPolluted => GameState.Progress.Locations.GetLocationState(locationIdx) == Level.State.Polluted;
  public bool isFeedingMode => locationIdx == Location.FeedLocation;
  public bool isCleanupMode => locationIdx == Location.ClearLocation;
  public Mode GetMode()
  {
    if(isFeedingMode)
      return Mode.Feeding;
    else if(isCleanupMode)
      return Mode.Clearing;
    else if(isPolluted)
      return Mode.Polluted;
    else
      return Mode.Standard;  
  }
  public int  visitsCnt => GameState.Progress.Locations.GetLocationVisits(locationIdx);

  UISummary    _uiSummary = null;
  UIStatusBar  _uiStatusBar = null;
  List<Animal> _animals = new List<Animal>();
  MaterialPropertyBlock _mpb = null;

  Item          _itemSelected;
  Item          _itemHovered;
  Item          _itemTileSelected;
  GridTile      _tileSelected;  
  Animal        _animalHovered;
  List<Item>    _items = new List<Item>();
  List<Item>    _items2 = new List<Item>();
  
  GarbagePile[] _garbagePiles;
  int[]         _garbagesCapacity = new int[4];
  int[]         _garbagesCleared = new int[4];

  StorageBox _storageBox;

  public class Grid
  {
    Vector2Int  _dim;
    float       _gridSpace;
    int[,]      _grid;
    GridTile[,] _tiles;
    public void Init(Vector2Int dim, float grid_space)
    {
      _dim = dim;
      _gridSpace = grid_space;
      _grid = new int[dim.y, dim.x];
      _tiles = new GridTile[dim.y, dim.x];
      System.Array.Clear(_grid, 0, _grid.Length);
    }
    public static Vector2Int g2a(Vector2 vgrid, Vector2Int _dim)
    {
      int ax = (int)System.Math.Round(vgrid.x + _dim.x * 0.5f - 0.1f, System.MidpointRounding.AwayFromZero);
      int ay = (int)System.Math.Round(vgrid.y + _dim.y * 0.5f - 0.1f, System.MidpointRounding.AwayFromZero);
      return new Vector2Int(ax, ay);
    }
    public static Vector2 a2g(Vector2Int va, Vector2Int _dim)
    {
      Vector2 v = Vector2.zero;
      v.y = ((-_dim.y + 1) * 0.5f + va.y);
      v.x = (-_dim.x + 1) * 0.5f + va.x;
      return v;
    }
    public void set(Vector2 vgrid, int val, Item.Kind kind = Item.Kind.None)
    {
      var va = g2a(vgrid, _dim);
      _grid[va.y, va.x] = val;
      _tiles[va.y, va.x].Set((val!=0)? true : false, kind == Item.Kind.Garbage);
    }
    public int get(Vector2 vgrid)
    {
      var va = g2a(vgrid, _dim);
      return _grid[va.y, va.x];
    }
    public GridTile getTile(Vector2 vgrid)
    {
      var va = g2a(vgrid, _dim);
      return _tiles[va.y, va.x];
    }
    public void tile(GridTile gt, Vector2 vgrid)
    {
      var va = g2a(vgrid, _dim);
      _tiles[va.y, va.x] = gt;
      gt.vgrid = vgrid;
      gt.Set(false);
    }
    public void hovers(bool hov)
    {
      for(int y = 0; y < _dim.y; ++y)
      {
        for(int x = 0; x < _dim.x; ++x)
        {
          _tiles[y,x].Hover(hov);
        }
      }
    }
    public Vector2? getEmpty()
    {
      List<Vector2> vps = new List<Vector2>();
      for(int y = 0; y < _dim.y; ++y)
      {
        for(int x = 0; x < _dim.x; ++x)
        {
          if(_grid[y, x] == 0)
            vps.Add(a2g(new Vector2Int(x, y), _dim));
        }
      }
      return (vps.Count > 0)? vps.get_random() : null;
    }
    public bool isInside(Vector2 vgrid) => Mathf.Abs(vgrid.x * 2) <= _dim.x && Mathf.Abs(vgrid.y*2) <= _dim.y;
    public bool isOverAxisZ(Vector3 vpos)
    {
      Vector2 vdim = new Vector2(_dim.x, _dim.y) * _gridSpace;
      return vpos.z <= vdim.y / 2;
    }
    public float getMaxZ()
    {
      return _dim.y * 0.5f * _gridSpace;
    }
  }

  Grid _grid = new Grid();

  void Awake()
  {
    locationIdx = GameState.Progress.locationIdx;

    Item.gridSpace = _gridSpace;
    _uiSummary = FindObjectOfType<UISummary>(true);
    _uiStatusBar = FindObjectOfType<UIStatusBar>(true); 

    _mpb = new MaterialPropertyBlock();
    _mpb.SetColor("_BaseColor", _waterColor);
    _waterRenderer.SetPropertyBlock(_mpb);

    GameState.Progress.Locations.VisitLocation(locationIdx);

    _splitMachine.Init(_items);
    _splitMachine.gameObject.SetActive(false);
    _feedingMachine.gameObject.SetActive(isFeedingMode);

    _storageBox = GetComponentInChildren<StorageBox>();

    _garbagePiles = GetComponentsInChildren<GarbagePile>();
    _garbagesCapacity = GameData.Levels.GetLocationDesc(locationIdx).garbages.clone();
    _garbagesCleared.fill(0);
    
    int sublocation = GameState.Progress.Locations.GetSublocationPassed(locationIdx);
    _dim = GameData.Levels.GetSublocation(locationIdx, sublocation).dim;

    onCreate?.Invoke(this);
  }
  void OnDestroy()
  {
    onDestroy?.Invoke(this);
  }
  IEnumerator Start()
  {
    yield return null;
    yield return null; //do not remove!
    yield return null; //do not remove!
    Init();
    yield return null;

    if(!GameState.Progress.Locations.IsCache(locationIdx))
      InitAnimals();
    else
      RestoreAnimals();

    onStart?.Invoke(this);

    if(GameState.Chest.shown)
      _rewardChest.Show();

    CheckMatchingItems();
    CacheLoc();
  }
  public void Hide()
  {
    onHide?.Invoke(this);
  }
  void  Init()
  {
    _grid.Init(dim, _gridSpace);

    List<Vector2> vs = new List<Vector2>();
    Vector2 v = Vector2.zero;
    for(int y = 0; y < dim.y; ++y)
    {
      v.y = -((-dim.y + 1) * 0.5f + y);
      for(int x = 0; x < dim.x; ++x)
      {
        v.x = (-dim.x + 1) * 0.5f + x;
        vs.Add(v);
        var tile = GameData.Prefabs.CreateGridElem(_tilesContainer);
        tile.transform.localPosition = Item.ToPos(v);
        _grid.tile(tile, v);
      }
    }
    vs.shuffle(100);
    vs.Reverse();

    if(!GameState.Progress.Locations.IsCache(locationIdx))
    {
      Item.ID id = new Item.ID();
      List<Item.ID> ids = new List<Item.ID>();
      for(int type = 0; type < _garbagesCapacity.GetLength(0); ++type)
      {
        for(int q = 0; q < _garbagesCapacity[type]; ++q)
        {
          id.kind = Item.Kind.Garbage;
          id.type = type;
          id.lvl = 0;
          ids.Add(id);
        }
      }
      ids.shuffle(ids.Count * 5);

      List<Item.ID> specIds = new List<Item.ID>();
      if(_resItemPerItems > 0)
      {
        int resItems = ids.Count / _resItemPerItems;
        var extras = new (Item.Kind kind, float weight)[]
        {
          new (Item.Kind.Stamina, _resStaminaPart),
          new (Item.Kind.Coin, _resCoinsPart),
          new (Item.Kind.Gem, _resGemsPart),
        };

        for(int q = 0; q < extras.Length; ++q)
        {
          int cnt = Mathf.RoundToInt(resItems * extras[q].weight);
          for(int i = 0; i < cnt ; ++i)
          {
            var spec_id = new Item.ID(0, 0, extras[q].kind).Validate();
            specIds.Add(spec_id);
          } 
        }
        specIds.shuffle(100);
      }

      for(int q = 0; q < ids.Count; ++q)
      {
        var item = GameData.Prefabs.CreateItem(ids[q], _itemsContainer);
        if(vs.Count > 0)
        {
          item.Init(vs.first());
          vs.RemoveAt(0);
          item.Spawn(item.vgrid, null, 15, Random.Range(0.5f, 1.5f));
          AddItem(item);
        }
        else
        {
          item.Init(Vector2.zero);
          _items2.Add(item);
          item.gameObject.SetActive(false);
        }
      }
      for(int q = 0; q < specIds.Count; ++q)
      {
        var item = GameData.Prefabs.CreateItem(specIds[q], _itemsContainer);
        if(vs.Count > 0)
        {
          item.Init(vs.first());
          vs.RemoveAt(0);
          item.Spawn(item.vgrid, null, 15, Random.Range(0.5f, 1.5f));
          AddItem(item);
        }
        else
        {
          item.Init(Vector2.zero);
          _items2.Add(item);
          item.gameObject.SetActive(false);
        }
      }
      _items2.shuffle(_items2.Count * 5);
    }
    else
    {
      Restore();
    }
    //_initialItemsCnt = itemsCount;

    InitGarbagePiles();
  }
  void InitGarbagePiles()
  {
    for(int q = 0; q < _garbagePiles.Length; ++q)
      _garbagePiles[q].Init(q, _items2.Count((Item it) => it.id.type == q));
  }
  void Restore()
  {
    GameState.LocationCache locCache = null;
    locCache = GameState.Progress.Locations.GetCache(locationIdx);
    for(int q = 0; q < locCache.items.Count; ++q)
    {
      var ic = locCache.items[q];
      var item = GameData.Prefabs.CreateItem(ic.id, _itemsContainer);
      item.Init(ic.vgrid);
      item.Spawn(item.vgrid, null, 15, Random.Range(0.5f, 1.5f));
      AddItem(item);
    }
    for(int q = 0; q < locCache.items2.Count; ++q)
    {
      var ic = locCache.items2[q];
      var item = GameData.Prefabs.CreateItem(ic.id, _itemsContainer);
      item.Init(ic.vgrid);
      _items2.Add(item);
      item.gameObject.SetActive(false);
    }
  }
  void InitAnimals()
  {
    var anim_type = GameData.Levels.GetLocationDesc(locationIdx).animalType;
    Animal animal = GameData.Prefabs.CreateAnimal(anim_type, _animalContainers[0]);
    GameState.Animals.AnimalAppears(animal.type);
    animal.Init();
    animal.Activate(true);
    _animals.Add(animal);
  }
  void RestoreAnimals()
  {
    InitAnimals();
  }
  //bool  firstPremium = false;
  void  AddItem(Item item)
  {
    _items.Add(item);
    _grid.set(item.vgrid, 1, item.id.kind);
    GameState.Progress.Items.ItemAppears(item.id);
  }
  void  SpawnItem(Vector2 vgrid)
  {
    if(_items2.Count > 0)
    {
      var item = _items2.first();
      _items2.RemoveAt(0);
      onUnderwaterSpawn?.Invoke(item);
      item.Spawn(vgrid, null, 15, 1);
      AddItem(item);
    }
  }
  void  MoveItemBack(Item item)
  {
    item.Select(false);
    item.MoveBack();
    if(!item.IsInMachine)
      _grid.set(item.vgrid, 1, item.id.kind);
  }  
  void  DestroyItem(Item item)
  {
    _items.Remove(item);
    _grid.set(item.vgrid, 0);
    onItemCleared?.Invoke(item);
    item.Hide();
  }

  public float PollutionRate()
  {
    // int requests = 0;
    // _animals.ForEach((animal) => requests += animal.requests);
    // return (float)requests / _requestCnt;

    return (float)totalGarbagesCleared / totalGarbagesCapacity;
  }
  
  Item GetNearestItem(Item[] arr)
  {
    Item item = null;
    
    if(arr.Length > 0)
    {
      item = System.Array.Find(arr, (it) => (_itemSelected.vwpos - it.vwpos).get_xz().magnitude < 1.0f);
      var match = System.Array.Find(arr, (it) => Item.Mergeable(_itemSelected, it));
      if(match)
        item = match;
    }
    return item;
  }
  public void OnInputBeg(TouchInputData tid)
  {
    _itemSelected = null;
    _itemSelected = tid.GetClosestCollider(_inputRad, Item.layerMask)?.GetComponent<Item>() ?? null;
    _itemSelected?.Select(true);
    _itemHovered = null;
    voffs = Vector3.zero;
  }
  Vector3 voffs = Vector3.zero;
  public void OnInputMov(TouchInputData tid)
  {
    if(finished)
      return;

    hoverItemMatch = false;  
    Item   nearestItem = null;
    Animal nearestAnimal = null;
    if(_itemSelected && tid.RaycastData.HasValue)
    {
      var vpt = tid.RaycastData.Value.point;
      voffs.y = Mathf.Lerp(voffs.y, 2.0f, Time.deltaTime * 10);
      _itemSelected.vwpos = Vector3.Lerp(_itemSelected.vwpos, vpt + voffs + _itemSelected.vbtmExtent, Time.deltaTime * 20);

      //nearest item
      {
        nearestItem = GetNearestItem(tid.GetObjectsInRange<Item>(_inputRad, Item.layerMask, true));
        if(nearestItem)
        {
          nearestItem.Hover(true);
          if(nearestItem != _itemHovered)
          {
            hoverItemMatch = Item.Mergeable(_itemSelected, nearestItem);
            onItemHovered?.Invoke(this);
          }
        }
        _itemHovered = nearestItem;
      }

      //nearest animal
      {
        nearestAnimal = tid.GetClosestObjectInRange<Animal>(_inputAnimRad, Animal.layerMask);
        if(nearestAnimal)
        {
          if(nearestAnimal != _animalHovered)
          {
            hoverItemMatch = nearestAnimal.CanPut(_itemSelected);
            onAnimalHovered?.Invoke(this);
            if(!isFeedingMode && hoverItemMatch)
              nearestAnimal.AnimTalk();
          }
        }
        _animalHovered = nearestAnimal;
      }
    }

    _grid.hovers(false);
    if(_itemSelected)
    {
      if(nearestItem)
      {
        _grid.getTile(nearestItem.vgrid).Hover(true);
      }
      else  
      {
        var tileHit = tid.GetClosestObjectInRange<GridTile>(_inputRad);
        tileHit?.Hover(true);
      }
    }
    if(nearestAnimal && nearestAnimal.CanPut(_itemSelected))
      onMagnetBeg?.Invoke(nearestAnimal.transform.position);
    else if(nearestItem != null && Item.Mergeable(_itemSelected, nearestItem))
      onMagnetBeg?.Invoke(nearestItem.transform.position);
    else
      onMagnetEnd?.Invoke(false);
  }
  public void OnInputEnd(TouchInputData tid)
  {
    if(!_itemSelected)
      return;

    bool isHit = IsItemHit(tid) || IsAnimalHit(tid) || IsTileHit(tid) || IsStorageHit(tid) || IsChestHit(tid);
    _itemSelected = null;
    _grid.hovers(false);
    _itemHovered = null;
    CheckMatchingItems();
    onMagnetEnd?.Invoke(false);
    CacheLoc();
  }
  double tapTime = 0;
  public void OnInputTapped(TouchInputData tid)
  {
    int layers = RewardChest.layerMask | StorageBox.layerMask;
    if(_feedingMachine.gameObject.activeInHierarchy)
      layers |= FeedingMachine.layerMask;
    var box = tid.GetClosestCollider(_inputRad, layers);
    if(box)
    {
      //if(Time.timeAsDouble - tapTime < 1.0f)
      {
        tapTime = 0;
        Vector2? vg = _grid.getEmpty();
        Vector3  vbeg = Vector3.zero;
        if(vg != null)
        {
          Item.ID? id = null;
          var chest = box.GetComponent<RewardChest>();
          if(chest)
          {
            id = chest.Pop();
            vbeg = chest.transform.position;
          }
          else
          {
            var storage = box.GetComponent<StorageBox>();
            if(storage)
            {
              id = storage.Pop();
              vbeg = storage.transform.position;
            }
            else
            {
              var feeding = box.GetComponent<FeedingMachine>();
              if(feeding)
              {
                id = feeding.Pop();
                vbeg = feeding.vpos;
              }
            }
          }
          if(id != null)
          {
            var item = GameData.Prefabs.CreateItem(id.Value, _itemsContainer);
            item.vgrid = vg.Value;
            AddItem(item);
            item.Throw(vbeg, item.vgrid);
            CacheLoc();
          }
        }
        else
          onNoRoomOnGrid?.Invoke(this);
      }
      //else
      //  tapTime = Time.timeAsDouble;
    }
    else
    {
      var item = tid.GetClosestCollider(_inputRad, Item.layerMask)?.GetComponent<Item>();
      if(item)
      {
        if(Time.timeAsDouble - tapTime < 0.25f)
        {
          if(item.id.IsSpecial)
          {
            tapTime = 0;
            int amount = GameState.Econo.AddRes(item.id);
            _items.Remove(item);
            _grid.set(item.vgrid, 0);
            _uiStatusBar.MoveCollectedUI(item, amount);
            onItemCollected?.Invoke(item);
            item.Hide();
            SpawnItem(item.vgrid);
            CacheLoc();
          }
        }
        else
        {
          tapTime = Time.timeAsDouble;
          if(_itemTileSelected == null)
          {
            SelectTile(item);
          }
          else
          {
            if(_itemTileSelected == item)
              DeselectTile();
            else
            {
              if(Item.Mergeable(_itemTileSelected, item))
              {
                var newItem = Item.Merge(_itemTileSelected, item, _items);
                if(newItem)
                {
                  _grid.set(_itemTileSelected.vgrid, 0);
                  _splitMachine.RemoveFromSplitSlot(_itemTileSelected);
                  onItemCleared?.Invoke(_itemTileSelected);
                  newItem.Show();
                  GameState.Progress.Items.ItemAppears(newItem.id);
                  SpawnItem(_itemTileSelected.vgrid);
                  CacheLoc();
                  DeselectTile();
                }
                else
                {
                  DeselectTile();
                  SelectTile(item);  
                }
              }
              else
              {
                DeselectTile();
                SelectTile(item);
              }
            }
          }
        }
      }
      else
      {
        DeselectTile();
      }
    }
  }
  void SelectTile(Item item)
  {
    _itemTileSelected = item;
    _tileSelected = _grid.getTile(_itemTileSelected.vgrid);    
    _tileSelected.Hover(true);
  }
  void DeselectTile()
  {
    if(_tileSelected)
      _tileSelected.Hover(false);
    _tileSelected = null;
    _itemTileSelected = null;
    
  }
  bool TryPushoutItem(Animal animalHit, Item item)
  {
    bool pushed = false;
    if(!item.id.IsSpecial)
    {
      Item.onPut?.Invoke(item);
      if(!isFeedingMode)
      {
        animalHit.Put(item);
        GameState.Econo.stamina -= 1;
        _garbagesCleared[item.id.type] += 1<<item.id.lvl;
        pushed = true;
      }
      else
        animalHit.Feed(item);
      onItemCleared?.Invoke(item);
      _grid.set(item.vgrid, 0);
      _items.Remove(item);
      if(item.IsInMachine)
        _splitMachine.RemoveFromSplitSlot(item);

      //_pollutionDest = PollutionRate();
      onGarbageOut?.Invoke(this);
      SpawnItem(item.vgrid);
      CacheLoc();
    }
    return pushed;
  }
  bool IsItemHit(TouchInputData tid)
  {
    bool is_hit = false;
    var itemHit = GetNearestItem(tid.GetObjectsInRange<Item>(_inputRad, Item.layerMask, true));//     tid.GetClosestCollider(_inputRad, Item.layerMask)?.GetComponent<Item>() ?? null;
    bool is_merged = false;
    if(itemHit && itemHit != _itemSelected && !itemHit.IsInMachine)
    {
      is_hit = true;
      var newItem = Item.Merge(_itemSelected, itemHit, _items);
      if(newItem)
      {
        _grid.set(_itemSelected.vgrid, 0);
        _splitMachine.RemoveFromSplitSlot(_itemSelected);
        onItemCleared?.Invoke(_itemSelected);
        newItem.Show();
        GameState.Progress.Items.ItemAppears(newItem.id);
        SpawnItem(_itemSelected.vgrid);
        is_merged = true;
        CacheLoc();  
      }
    }
    if(!is_hit || (is_hit && !is_merged))
    {
      MoveItemBack(_itemSelected);
    }

    return is_hit;
  }
  bool IsAnimalHit(TouchInputData tid)
  {
    bool is_hit = false;
    var animalHit = tid.GetClosestCollider(_inputAnimRad, Animal.layerMask)?.GetComponent<Animal>() ?? null;
    if(animalHit)
    {
      if(GameState.Econo.CanPushoutItem())
      {
        is_hit = true;
        if(!TryPushoutItem(animalHit, _itemSelected))
          MoveItemBack(_itemSelected);
        CheckEnd();
        CacheLoc();
      }
      else
      {
        onNoStaminaToPushout?.Invoke();
      }
    }

    if(!is_hit)
      MoveItemBack(_itemSelected);

    return is_hit;
  }
  bool IsTileHit(TouchInputData tid)
  {
    bool is_hit = false;
    var tileHit = tid.GetClosestObjectInRange<GridTile>(_inputRad);
    if(tileHit && _grid.get(tileHit.vgrid) == 0)
    {
      _grid.set(_itemSelected.vgrid, 0);
      _itemSelected.vgrid = tileHit.vgrid;
      _grid.set(_itemSelected.vgrid, 1, _itemSelected.id.kind);
      _itemSelected.Select(false);
      _splitMachine.RemoveFromSplitSlot(_itemSelected);
      _itemSelected.MoveToGrid();
      is_hit = true;
      _grid.hovers(false);
      CacheLoc();
    }
    
    if(!is_hit)
      MoveItemBack(_itemSelected);

    return is_hit;
  }
  bool IsStorageHit(TouchInputData tid)
  {
    bool is_hit = false;
    var storage = tid.GetClosestObjectInRange<StorageBox>(_inputRad, StorageBox.layerMask);
    if(storage)
    {
      if(_itemSelected.id.IsSpecial)
      {
        storage.Push(_itemSelected.id);
        _items.Remove(_itemSelected);
        _grid.set(_itemSelected.vgrid, 0);
        _itemSelected.Hide();
        is_hit = true;
        SpawnItem(_itemSelected.vgrid);
      }
      else
      {
        storage.NoPush(_itemSelected.id);
      }
    }
    if(!is_hit)
      MoveItemBack(_itemSelected);

    return is_hit;
  }
  bool IsChestHit(TouchInputData tid)
  {
    bool is_hit = false;
    var chest = tid.GetClosestObjectInRange<RewardChest>(_inputRad, RewardChest.layerMask);
    if(chest)
    {
      chest.NoPush(_itemSelected.id);
      MoveItemBack(_itemSelected);
    }
    
    return is_hit;
  }
  bool IsTutorial<T>() where T : TutorialSystem.TutorialStep
  {
    return GetComponentInChildren<T>(true) != null;
  }
  void CacheLoc()
  {
    GameState.Progress.Locations.Cache(this);  
  }
  void CacheClear()
  {
    GameState.Progress.Locations.ClearCache(locationIdx);
  }
  public void Quit()
  {
    CacheLoc();
  }
  IEnumerator coMoveToSB()
  {
    Vector3 vsbox = _storageBox.transform.position + new Vector3(0, 1, 0);
    List<Item> itms = _items.FindAll((Item item) => item.id.IsSpecial).ToList();
    while(itms.Count > 0)
    {
      for(int q = 0; q < itms.Count;)
      {
        itms[q].vwpos = Vector3.Lerp(itms[q].vwpos, vsbox, Time.deltaTime * 4);
        if(Vector3.Distance(itms[q].vwpos, vsbox) < 0.1f)
        {
          _storageBox.Push(itms[q].id);
          DestroyItem(itms[q]);
          itms.RemoveAt(q);
          --q;
        }
        ++q;
      }
      yield return null;
    }
  }
  IEnumerator coEnd()
  {
    yield return StartCoroutine(coMoveToSB());
    yield return new WaitForSeconds(1.0f);
    foreach(var anim in _animals)
      anim.Deactivate();
    yield return new WaitForSeconds(1.5f);
    succeed = true;
    onFinished?.Invoke(this);
    if(!isFeedingMode)
    {
      //GameState.Progress.Locations.SetLocationFinished();
      //bool passedLoc = GameState.Progress.Locations.SetSublocationPassed(GameState.Progress.locationIdx);
      if(!isCleanupMode)// && passedLoc)
        GameState.Progress.Locations.UnlockNextLocation();
    }
    yield return new WaitForSeconds(0.5f);
    GameState.Progress.Locations.ClearCache(locationIdx);
    _uiSummary.Show(this);
  }
  void CheckEnd()
  {
    if(isFeedingMode)
      return;

    //int activeAnimals = _animals.Count((animal) => animal.isActive);
    //if(!finished && activeAnimals == 0)
    if(!finished && totalGarbagesCleared >= totalGarbagesCapacity)
    {
      finished = true;
      StartCoroutine(coEnd());
    }
  }
  void CheckMatchingItems()
  {
    List<Item> req_items = new List<Item>();
    _animals.ForEach((anim) => req_items.AddRange(anim.garbagesView));
    req_items.ForEach((reqit) => reqit.tickIco = _items.Any((it) => Item.EqType(it, reqit)));
  }
  void Process()
  {
    foreach(var _item in _items)
    {
      if(!_item.IsSelected)
        _item.Hover(false);
    }
  }
  void Update()
  {
    Process();

  #if UNITY_EDITOR
    if(Input.GetKeyDown(KeyCode.E))
    {
      if(!finished)
      {
        finished = true;
        StartCoroutine(coEnd());
      }
    }
  #endif     
  }

  void OnDrawGizmos()
  {
    Gizmos.color = Color.red;
    Vector3 vLB = new Vector3(-dim.x * 0.5f, 0, -dim.y * 0.5f);
    Vector3 vRT = new Vector3( dim.x * 0.5f, 0, dim.y * 0.5f);
    var v1 = Vector3.zero;
    v1.x = vLB.x;
    v1.z = vRT.z;
    Gizmos.DrawLine(vLB, v1);
    v1.x = vRT.x;
    v1.z = vLB.z;
    Gizmos.DrawLine(vLB, v1);
    v1.x = vRT.x;
    v1.z = vLB.z;
    Gizmos.DrawLine(vRT, v1);
    v1.x = vLB.x;
    v1.z = vRT.z;
    Gizmos.DrawLine(vRT, v1);
  }
}
