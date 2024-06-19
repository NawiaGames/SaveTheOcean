using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameLib;
using GameLib.InputSystem;

using GarbCats = GameData.GarbCats;

public class Level : MonoBehaviour
{
  public static System.Action<Level>   onCreate, onStart, onGarbageOut, onNoRoomOnGrid, onItemHovered, onAnimalHovered, onShipHovered;
  public static System.Action<Level>   onDone, onFinished, onHide, onDestroy;
  public static System.Action<Vector3> onMagnetBeg;
  public static System.Action<bool>    onMagnetEnd;
  public static System.Action<Item>    onPremiumItem, onItemCollected, onItemCleared, onUnderwaterSpawn;

  [Header("Refs")]
  [SerializeField] Transform      _itemsContainer;
  [SerializeField] Transform      _tilesContainer;
  [SerializeField] Transform      _animalsContainer;
  [SerializeField] Transform[]    _animalContainers;
  [SerializeField] Transform      _friendsContainer;
  [SerializeField] Transform      _specContainer;
  [SerializeField] Renderer       _waterRenderer;
  [SerializeField] RewardChest2   _rewardChest2Prefab;
  [SerializeField] SplitMachine   _splitMachine;
  [SerializeField] FeedingMachine _feedingMachine;
  [SerializeField] Transform[]    _boundsNSWE;
  [SerializeField] Ship           _ship;
  //[SerializeField] Transform[] _paths;
  //[SerializeField] Transform _poiLT;
  //[SerializeField] Transform _poiRB;

  [Header("Settings")]
  [SerializeField] Vector2Int _dim;
  [SerializeField] float      _gridSpace = 1.0f;
  [SerializeField] Color      _waterColor;
  [SerializeField] float      _inputRad = 1.5f;
  [SerializeField] float      _inputAnimRad = 2.0f;
  [SerializeField] float      _inputShipRad = 2.5f;
  [SerializeField] float      _inputStorageRad = 2.0f;
  [SerializeField] bool       _inputAnimRadMatching = true;
  [Header("LvlDesc")]
  [SerializeField] float[]    _chanceToDowngradeItem = new float[6];
  [field:SerializeField] public int  _resItemPerItems {get; private set;}= 0;
  [SerializeField] float      _resGemsPart = 0.1f;
  [SerializeField] float      _resCoinsPart = 0.4f;
  [SerializeField] float      _resStaminaPart = 0.5f;
  [SerializeField] LvlDesc[]  _lvlDescs;

  public bool IsSolvable(){
    var allItems = new List<GarbCats>();
    foreach(var x in _lvlDescs)
      allItems.AddRange(x.itemsCats);
    return allItems.Count == allItems.Distinct().Count();
  }

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

  [System.Serializable]
  public struct LvlDesc
  {
    [SerializeField] Animal _animal;
    [SerializeField] GarbCats[] _itemsCats;

    public void IncCats(int incBy)
    {
      for(int i = 0; i <_itemsCats.Length ; i++)
      {
        var id = GameData.Prefabs.GetGarbagePrefab(_itemsCats[i]).id;
        id.lvl = Mathf.Clamp(id.lvl + incBy, 0, id.LevelsCnt-1);
        _itemsCats[i] = (GarbCats)(id.type * 10 + id.lvl);
      }
    }

    public int GetSolutionMoveCount()
    {
      var solution = 0;
      for(int i = 0; i < _itemsCats.Length; i++)
        solution += (int)Mathf.Pow(2, (int)_itemsCats[i]%10);
      return solution;
    }

    public Animal  animal => _animal;
    public GarbCats[] itemsCats => _itemsCats;
    public Item items(int idx) => GameData.Prefabs.GetGarbagePrefab(_itemsCats[idx]);
  }

  public int       GetNumberOfMovesToSolve(){
    var solution = 0;
    foreach (var animal in _lvlDescs){
        solution += animal.GetSolutionMoveCount();
    }
    return solution;
  }
  public Transform GetPrimaryAnimalContainer() => _animalContainers.FirstOrDefault();
  public int       GetUnderwaterGarbagesCnt() => _items2.Count((item) => !item.id.IsSpecial);

  public int      locationIdx {get; private set;} = -1;
  public bool     succeed {get; private set;}
  public bool     finished {get; private set;}
  public int      points {get; set;} = 0;
  public int      stars {get; set;}
  public int      itemsCount => _items.Count + _items2.Count;
  public int      initialItemsCnt => _initialItemsCnt;
  public bool     hoverItemMatch = false;
  public Vector2Int dim => _dim;
  public List<Item> listItems => _items;
  public List<Item> listItems2 => _items2;
  public List<Animal> animals => _animals;
  public List<Item.ID> requestsList => _ship._garbages;
  public List<RewardChest2> rewardChests => _rewardChests;

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
  List<Animal> _animals = new();

  MaterialPropertyBlock _mpb = null;

  Item        _itemSelected;
  Item        _itemHovered;
  Animal      _animalHovered;
  Ship        _shipHovered;
  List<Item>  _items = new List<Item>();
  List<Item>  _items2 = new List<Item>();
  int         _requestCnt = 0;
  int         _initialItemsCnt = 0;
  List<RewardChest2> _rewardChests = new();
  List<AnimalFriend> _friends = new();

  //float       _pollutionRate = 1.0f;
  float       _pollutionDest = 1.0f;

  void Awake()
  {
    locationIdx = GameState.Progress.locationIdx;

    Item.gridSpace = _gridSpace;
    _uiSummary = FindFirstObjectByType<UISummary>(FindObjectsInactive.Include);
    _uiStatusBar = FindFirstObjectByType<UIStatusBar>(FindObjectsInactive.Include);

    _mpb = new MaterialPropertyBlock();
    _mpb.SetColor("_BaseColor", _waterColor);
    _waterRenderer.SetPropertyBlock(_mpb);

    GameState.Progress.Locations.VisitLocation(locationIdx);

    _splitMachine.Init(_items);
    _splitMachine.gameObject.SetActive(false);
    _feedingMachine.gameObject.SetActive(isFeedingMode);

    Item.onMerged += OnItemMerged;
    GameState.Econo.onRewardProgressChanged += OnRewardChanged;

    onCreate?.Invoke(this);
  }
  void OnDestroy()
  {
    Item.onMerged -= OnItemMerged;
    GameState.Econo.onRewardProgressChanged -= OnRewardChanged;
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
    {
      if(!isFeedingMode)
        InitShip();
      else
        InitAnimals();
    }
    else
    {
      if(!isFeedingMode)
        RestoreShip();
      else
        RestoreAnimals();
    }
    InitFriends();

    onStart?.Invoke(this);

    CacheLoc();
  }
  public void Hide()
  {
    onHide?.Invoke(this);
  }

  void Init()
  {
    List<int> levels_idx = new(){Capacity = 1000};
    for (int q = 0; q < _chanceToDowngradeItem.Length; ++q)
    {
      int cnt = (int)(1000 * _chanceToDowngradeItem[q]);
      for(int w = 0; w < cnt; ++w)
        levels_idx.Add(q);
    }
    levels_idx.shuffle(2000);

    // List<Vector2> vs = new List<Vector2>();
    // Vector2 v = Vector2.zero;
    // for(int y = 0; y < dim.y; ++y)
    // {
    //   v.y = -((-dim.y + 1) * 0.5f + y);
    //   for(int x = 0; x < dim.x; ++x)
    //   {
    //     v.x = (-dim.x + 1) * 0.5f + x;
    //     vs.Add(v);
    //   }
    // }
    // vs.shuffle(100);
    // vs.Reverse();

    if(!GameState.Progress.Locations.IsCache(locationIdx))
    {
      Item.ID id = new Item.ID();
      List<Item.ID> ids = new List<Item.ID>();
      for(int q = 0; q < _lvlDescs.Length; ++q)
      {
        if(isCleanupMode)
          _lvlDescs[q].IncCats(GameState.Cleanup.level);

        var lvlDesc = _lvlDescs[q];
        _requestCnt += lvlDesc.itemsCats.Length;
        for(int i = 0; i < lvlDesc.itemsCats.Length; ++i)
        {
          var item = _lvlDescs[q].items(i);
          int itemLevel = item.id.lvl;
          id.type = item.id.type;
          id.kind = item.id.kind;
          id.lvl = item.id.lvl;
          int vi = (levels_idx.Count > 0)? levels_idx[Random.Range(0, levels_idx.Count-1)] : 0;
          if(vi < itemLevel)
          {
            for(int d = 0; d < 1 << (itemLevel-vi); ++d)
            {
              id.lvl = vi;
              ids.Add(id);
            }
          }
          else
          {
            ids.Add(id);
          }
        }
      }
      ids.shuffle(ids.Count * 5);

      // List<Item.ID> specIds = new List<Item.ID>();
      // if(_resItemPerItems > 0)
      // {
      //   int resItems = ids.Count / _resItemPerItems;
      //   var extras = new (Item.Kind kind, float weight)[]
      //   {
      //     new (Item.Kind.Stamina, _resStaminaPart),
      //     new (Item.Kind.Coin, _resCoinsPart),
      //     new (Item.Kind.Gem, _resGemsPart),
      //   };

      //   for(int q = 0; q < extras.Length; ++q)
      //   {
      //     int cnt = Mathf.RoundToInt(resItems * extras[q].weight);
      //     for(int i = 0; i < cnt ; ++i)
      //     {
      //       var spec_id = new Item.ID(0, 0, extras[q].kind).Validate();
      //       specIds.Add(spec_id);
      //     }
      //   }
      //   specIds.shuffle(100);
      // }
      int itemsCnt = dim.x * dim.y;
      for(int q = 0; q < ids.Count; ++q)
      {
        var item = GameData.Prefabs.CreateItem(ids[q], _itemsContainer);
        if(itemsCnt > 0)
        {
          //item.Init(vs.first());
          item.Init(GetRandomPosXZ());
          itemsCnt--;
          item.Spawn(item.vwpos, null, 15, Random.Range(0.5f, 1.5f), Random.Range(0, 3.0f));
          AddItem(item);
        }
        else
        {
          item.Init(Vector3.zero);
          _items2.Add(item);
          item.gameObject.SetActive(false);
        }
      }
      // for(int q = 0; q < specIds.Count; ++q)
      // {
      //   var item = GameData.Prefabs.CreateItem(specIds[q], _itemsContainer);
      //   if(vs.Count > 0)
      //   {
      //     item.Init(GetRandomGridPos());// vs.first());
      //     vs.RemoveAt(0);
      //     item.Spawn(item.vgrid, null, 15, Random.Range(0.5f, 1.5f), Random.Range(0.0f, 3f));
      //     AddItem(item);
      //   }
      //   else
      //   {
      //     item.Init(Vector2.zero);
      //     _items2.Add(item);
      //     item.gameObject.SetActive(false);
      //   }
      // }
      _items2.shuffle(_items2.Count * 5);
    }
    else
    {
      Restore();
    }
    _initialItemsCnt = itemsCount;
  }
  void Restore()
  {
    GameState.LocationCache locCache = null;
    locCache = GameState.Progress.Locations.GetCache(locationIdx);
    for(int q = 0; q < locCache.items.Count; ++q)
    {
      var ic = locCache.items[q];
      var item = (!ic.bag)? GameData.Prefabs.CreateItem(ic.id, _itemsContainer) : GameData.Prefabs.CreateBagItem(ic.id, _itemsContainer);
      item.Init(ic.vpos);
      item.Spawn(ic.vpos, null, 15, Random.Range(0.5f, 1.5f), Random.Range(0.0f, 3.0f));
      AddItem(item);
    }
    for(int q = 0; q < locCache.items2.Count; ++q)
    {
      var ic = locCache.items2[q];
      var item = GameData.Prefabs.CreateItem(ic.id, _itemsContainer);
      item.Init(ic.vpos);
      _items2.Add(item);
      item.gameObject.SetActive(false);
    }
    for(int q = 0; q < locCache.chests.Count; ++q)
    {
      var cc = locCache.chests[q];
      CreateRewardChest(cc.vpos, new GameData.Rewards.Reward(){stamina = cc.rewStamina, coins = cc.rewCoins, gems = cc.rewGems}, true);
    }
  }
  void InitAnimals()
  {
    for(int q = 0; q < _lvlDescs.Length; ++q)
    {
      if(isFeedingMode)
      {
        if(!GameState.Animals.DidAnimalAppear(_lvlDescs[q].animal.type))
          continue;
      }
      Animal animal = Instantiate(_lvlDescs[q].animal, _animalContainers[q]);
      GameState.Animals.AnimalAppears(animal.type);
      animal.Init(_lvlDescs[q].itemsCats);
      animal.Activate(true);
      _animals.Add(animal);
    }
  }
  void RestoreAnimals()
  {
    var cache = GameState.Progress.Locations.GetCache(locationIdx);
    if(!isFeedingMode)
    {
      for(int q = 0; q < _lvlDescs.Length; ++q)
      {
        Animal animal = Instantiate(_lvlDescs[q].animal, _animalContainers[q]);
        animal.Init(cache.requests[q].ids);
        if(cache.requests[q].ids.Count > 0)
          animal.Activate(true);
        else
          animal.SetInactive();
        _animals.Add(animal);
      }
    }
    else
      InitAnimals();
  }
  void InitShip()
  {
    List<Item.ID> ids = new();
    for(int q = 0; q < _lvlDescs.Length; ++q)
    {
      foreach(var ictg in _lvlDescs[q].itemsCats)
      {
        var item = GameData.Prefabs.GetGarbagePrefab(ictg);
        ids.Add(item.id);
      }
    }
    _ship.Init(ids);
    _ship.Activate(true);
  }
  void RestoreShip()
  {
    var cache = GameState.Progress.Locations.GetCache(locationIdx);
    if(!isFeedingMode)
    {
      List<Item.ID> ids = new();
      foreach(var req in cache.requests)
      {
        ids.AddRange(req.ids);
      }
      _ship.Init(ids);
      _ship.Activate(true);
    }
    else
      InitAnimals();
  }
  void InitFriends()
  {
    for(int q = 0; q < 3; ++q)
    {
      var friend = GameData.Prefabs.CreateAnimalFriend(GameState.Progress.locationIdx, _friendsContainer);
      _friends.Add(friend);
      friend.StartMove(_ship.transform.position, 4 + q * 1.5f);
    }
  }
  void  AddItem(Item item)
  {
    _items.Add(item);
    GameState.Progress.Items.ItemAppears(item.id);
  }
  Vector3 GetRandomPosXZ() => new Vector3(Random.Range(_boundsNSWE[2].position.x + 2, _boundsNSWE[3].position.x - 2), 0, Random.Range(_boundsNSWE[1].position.z + 1, -1));////new Vector3(Random.Range(-_dim.x/2, _dim.x/2), 0, Random.Range(-dim.y/2, 0) - 2);
  void  SpawnItem(Vector3 vpos)
  {
    if(_items2.Count > 0)
    {
      var item = _items2.first();
      _items2.RemoveAt(0);
      item.Spawn(vpos, null, 15, 1);
      onUnderwaterSpawn?.Invoke(item);
      AddItem(item);
    }
  }
  void  EndMoveItem(Item item)
  {
    item.Select(false);
    item.DragEnd();
    // if(!item.IsInMachine)
    //   _grid.set(item.vgrid, 1, item.id.kind);
  }
  void  DestroyItem(Item item)
  {
    _items.Remove(item);
    //_grid.set(item.vgrid, 0);
    onItemCleared?.Invoke(item);
    item.Hide();
  }

  public float PollutionRate()
  {
    int requests = 0;
    if(isFeedingMode)
      _animals.ForEach((animal) => requests += animal.requests);
    else
      requests = _ship.requests;
    return (float)requests / _requestCnt;
  }

  Item GetNearestItem(Item[] arr)
  {
    Item item = null;

    if(arr.Length > 0)
    {
      //item = System.Array.Find(arr, (it) => it != _itemSelected && (_itemSelected.vwpos - it.vwpos).get_xz().magnitude < 1.5f);
      var match = System.Array.Find(arr, (it) => _itemSelected != it && Item.Mergeable(_itemSelected, it));
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
    Ship   nearestShip = null;

    if(_itemSelected && tid.RaycastData.HasValue)
    {
      var vpt = tid.RaycastData.Value.point;
      vpt.x = Mathf.Clamp(vpt.x, _boundsNSWE[2].position.x + 1f, _boundsNSWE[3].position.x - 1f);
      vpt.z = Mathf.Clamp(vpt.z, _boundsNSWE[1].position.z + 1f, _boundsNSWE[0].position.z - 1f);
      voffs.y = Mathf.Lerp(voffs.y, 0.15f, Time.deltaTime * 10);
      _itemSelected.MoveSelectedTo(vpt + voffs);

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
      //nearest ship
      {
        nearestShip = tid.GetClosestObjectInRange<Ship>(_inputShipRad, Ship.layerMask);
        if(nearestShip)
        {
          if(nearestShip != _shipHovered)
          {
            hoverItemMatch = nearestShip.CanPut(_itemSelected);
            onShipHovered?.Invoke(this);
          }
        }
        _shipHovered = nearestShip;
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
    if(nearestShip && _ship.CanPut(_itemSelected))
      onMagnetBeg?.Invoke(_ship.transform.position);
    else if(nearestAnimal && (!_inputAnimRadMatching || nearestAnimal.CanPut(_itemSelected)))
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

    bool is_hit = IsItemHit(tid) || IsShipHit(tid) || IsAnimalHit(tid) || IsSplitMachineHit(tid);
    if(!is_hit)
    {
      EndMoveItem(_itemSelected);
    }
    _itemSelected = null;
    _itemHovered = null;
    onMagnetEnd?.Invoke(false);
    CacheLoc();
  }
  double tapTime = 0;
  public void OnInputTapped(TouchInputData tid)
  {
    int layers = 1<<_rewardChest2Prefab.gameObject.layer;
    if(_feedingMachine.gameObject.activeInHierarchy)
      layers |= FeedingMachine.layerMask;
    var box = tid.GetClosestCollider(_inputRad, layers);
    if(box)
    {
      //if(Time.timeAsDouble - tapTime < 1.0f)
      {
        tapTime = 0;
        Vector3  vp = GetRandomPosXZ();//_grid.getEmpty();
        Vector3  vbeg = Vector3.zero;
        Item.ID? id = null;
        var chest = box.GetComponent<RewardChest2>();
        if(chest)
        {
          if(!chest.isOpen)
          {
            chest.OpenLid();
          }
          else if(!chest.rewardClaimed)
          {
            int[] counts = {chest.rewardStamina, chest.rewardCoins, chest.rewardGems};
            Item.ID[] ids = {Item.ID.FromKind(Item.Kind.Stamina, 0, 0), Item.ID.FromKind(Item.Kind.Coin, 0, 0), Item.ID.FromKind(Item.Kind.Gem, 0, 0)};
            for(int q = 0; q < 3; ++q)
            {
              var cid = ids[q];
              var item = GameData.Prefabs.CreateItem(cid, _itemsContainer);
              item.vwpos = chest.transform.position;
              int amount = GameState.Econo.AddRes(cid, counts[q]);
              _uiStatusBar.MoveCollectedUI(item, Mathf.Min(amount, 10));
              onItemCollected?.Invoke(item);
              item.Hide();
              CacheLoc();
            }
            chest.Hide();
            return;
          }
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
          item.vwpos = vbeg;
          AddItem(item);
          item.Throw(vbeg, vp);
          CacheLoc();
        }
      }
      // else
      //  tapTime = Time.timeAsDouble;
    }
    else
    {
      var item = tid.GetClosestCollider(_inputRad, Item.layerMask)?.GetComponent<Item>();
      if(item)
      {
        if(Time.timeAsDouble - tapTime < 0.5f)
        {
          if(item.id.IsSpecial)
          {
            tapTime = 0;
            int amount = GameState.Econo.AddRes(item.id);
            _items.Remove(item);
            _uiStatusBar.MoveCollectedUI(item, amount);
            onItemCollected?.Invoke(item);
            item.Hide();
            SpawnItem(item.vwpos);
            CacheLoc();
          }
          else if(item.id.kind == Item.Kind.Garbage)
          {
            tapTime = 0;
            if(_ship && _ship.IsReq(item))
            {
              item.ThrowToShip(_ship.transform.position + new Vector3(0,0.5f, 0), (Item item) => {PutItemToShip(_ship, item); CacheLoc(); CheckEnd();});
            }
          }
        }
        else
        {
          tapTime = Time.timeAsDouble;
        }
      }
    }
  }
  bool IsItemHit(TouchInputData tid)
  {
    bool is_hit = false;
    var itemHit = GetNearestItem(tid.GetObjectsInRange<Item>(_inputRad, Item.layerMask, true));//     tid.GetClosestCollider(_inputRad, Item.layerMask)?.GetComponent<Item>() ?? null;
    bool is_merged = false;
    if(itemHit && itemHit != _itemSelected && !itemHit.IsInMachine &&  Item.EqType(itemHit, _itemSelected) && itemHit.IsUpgradable)
    {
      is_hit = true;
      var next_id = Item.ChgLvl(itemHit.id);
      bool makeBag = _ship._garbages.FindIndex((ri) => Item.ID.Eq(ri, next_id)) >= 0 && _items.FindIndex((it) => Item.ID.Eq(it.id, next_id)) < 0;
      var newItem = Item.Merge(_itemSelected, itemHit, _items, makeBag);
      if(newItem)
      {
        _splitMachine.RemoveFromSplitSlot(_itemSelected);
        onItemCleared?.Invoke(_itemSelected);
        newItem.Show();
        GameState.Progress.Items.ItemAppears(newItem.id);
        SpawnItem(_itemSelected.vwpos);
        is_merged = true;
        CacheLoc();
      }
    }
    if(is_hit && !is_merged)
    {
      EndMoveItem(_itemSelected);
    }

    return is_hit;
  }
  void PutItemToShip(Ship shipHit, Item item)
  {
    Item.onPut?.Invoke(item);
    shipHit.Put(item);
    onItemCleared?.Invoke(item);
    _items.Remove(item);
    if(item.IsInMachine)
      _splitMachine.RemoveFromSplitSlot(item);

    _pollutionDest = PollutionRate();
    onGarbageOut?.Invoke(this);
    SpawnItem(item.vwpos);
    CacheLoc();
  }
  void PutItemToAnim(Animal animalHit, Item item)
  {
    Item.onPut?.Invoke(item);
    if(!isFeedingMode)
      animalHit.Put(item);
    else
      animalHit.Feed(item);
    onItemCleared?.Invoke(item);
    _items.Remove(item);
    if(item.IsInMachine)
      _splitMachine.RemoveFromSplitSlot(item);

    _pollutionDest = PollutionRate();
    onGarbageOut?.Invoke(this);
    SpawnItem(item.vwpos);
    CacheLoc();
  }
  bool IsShipHit(TouchInputData tid)
  {
    bool is_hit = false;
    var shipHit = tid.GetClosestCollider(_inputShipRad, Ship.layerMask)?.GetComponent<Ship>() ?? null;
    if(shipHit)
    {
      if(shipHit.IsReq(_itemSelected)) //CanPut(_itemSelected))
      {
        PutItemToShip(shipHit, _itemSelected);
        CheckEnd();
        is_hit = true;
        CacheLoc();
      }
      else
      {
        if(!shipHit.IsReq(_itemSelected))
          Item.onNoPut?.Invoke(_itemSelected);
      }
    }

    return is_hit;
  }
  bool IsAnimalHit(TouchInputData tid)
  {
    bool is_hit = false;
    var animalHit = tid.GetClosestCollider(_inputAnimRad, Animal.layerMask)?.GetComponent<Animal>() ?? null;
    if(animalHit)
    {
      if(animalHit.IsReq(_itemSelected)) //CanPut(_itemSelected))
      {
        PutItemToAnim(animalHit, _itemSelected);
        CheckEnd();
        is_hit = true;
        CacheLoc();
      }
      else
      {
        if(!animalHit.IsReq(_itemSelected))
          Item.onNoPut?.Invoke(_itemSelected);
      }
    }

    return is_hit;
  }
  bool IsSplitMachineHit(TouchInputData tid)
  {
    bool is_hit = false;
    var splitMachineHit = tid.GetClosestCollider(0.5f);
    bool is_split_machine = _splitMachine?.IsDropSlot(splitMachineHit) ?? false;
    if(is_split_machine)
    {
      bool itemFromSplitMachine = _itemSelected.IsInMachine && _splitMachine.capacity == 1;
      if(_splitMachine.IsReady || itemFromSplitMachine)
      {
        if(_itemSelected.IsSplitable && !_itemSelected.id.IsSpecial)
        {
          if(itemFromSplitMachine)
            _splitMachine.RemoveFromSplitSlot(_itemSelected);
          _splitMachine.DropDone();
          _splitMachine.AddToDropSlot(_itemSelected);
          is_hit = true;
        }
        else
          _splitMachine.DropNoSplittable();
      }
      else
        _splitMachine.DropNoCapacity();

      SplitMachine.onDropped?.Invoke(_splitMachine);
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
  IEnumerator coEnd()
  {
    yield return new WaitForSeconds(1.0f);
    succeed = true;
    onFinished?.Invoke(this);
    if(!isFeedingMode)
    {
      GameState.Progress.Locations.SetLocationFinished();
      if(!isCleanupMode)
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

    if(_ship.requests == 0)
    {
      finished = true;
      StartCoroutine(coEnd());
    }
  }
  void OnItemMerged(Item item)
  {
    if(locationIdx > 0)
      GameState.Econo.rewards += 1;
  }
  void OnRewardChanged(float rewardPoints)
  {
    var rewardProgress = GameData.Econo.GetRewardProgress(rewardPoints);
    if(rewardProgress.lvl > GameState.Chest.rewardLevel && itemsCount > 1)
    {
      GameState.Chest.rewardLevel = rewardProgress.lvl;
      CreateRewardChest(new Vector3(Random.Range(-dim.x * 0.5f + 0.5f, dim.x * 0.5f - 0.5f), -4, Random.Range(-dim.y * 0.5f + 0.5f, dim.y * 0.5f - 0.5f)), GameState.Chest.GetReward(), false);
    }
  }
  void CreateRewardChest(Vector3 v, GameData.Rewards.Reward rew, bool isRestoring)
  {
    var chest = Instantiate(_rewardChest2Prefab, v, Quaternion.identity, _specContainer);
    _rewardChests.Add(chest);
    chest.Show(rew);
    if(!isRestoring)
      CacheLoc();
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
    if(finished)
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
    if(Input.GetKeyDown(KeyCode.Q))
    {
      CreateRewardChest(new Vector3(Random.Range(-dim.x * 0.5f, dim.x * 0.5f), -4, Random.Range(-dim.y * 0.5f, dim.y * 0.5f)), new GameData.Rewards.Reward(){stamina = 5, coins = 6, gems = 4}, false);
    }
  #endif
  }

  void OnDrawGizmos()
  {
    Gizmos.color = Color.red;
    Vector3 vLB = new(-dim.x * 0.5f - 0.25f, 0, -dim.y * 0.5f - 0.25f);
    Vector3 vRT = new( dim.x * 0.5f + 0.25f, 0,  dim.y * 0.5f + 0.25f);
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
