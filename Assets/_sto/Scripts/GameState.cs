using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameLib.DataSystem;

[CreateAssetMenu, DefaultExecutionOrder(-2)]
public class GameState : SavableScriptableObject
{
  static private GameState _static_this = null;
  static public  GameState get() { return _static_this;}
  public GameState()
  {
    _static_this = this;
  }
  public static void Process()
  {
    GameState.Econo.Process();
    GameState.Progress.Locations.Process();
  }

  [System.Serializable]
  public struct ItemCache
  {
    public Item.ID id;
    public Vector2 vgrid;
    public ItemCache(Item item)
    {
      id = item.id;
      vgrid = item.vgrid;
    }
  }
  [System.Serializable]
  public struct RequestCache
  {
    public List<Item.ID> ids;
    public RequestCache(List<Item.ID> itemIds)
    {
      ids = new List<Item.ID>();
      ids.AddRange(itemIds);
    }
  }
  [System.Serializable]
  public class LocationCache
  {
    public List<ItemCache>    items = new List<ItemCache>();
    public List<ItemCache>    items2 = new List<ItemCache>();
    public List<RequestCache> requests = new List<RequestCache>();

    public void Clear()
    {
      items.Clear();
      items2.Clear();
      requests.Clear();
    }
  }

  [System.Serializable]
  class LocationState
  {
    [SerializeField] int _idx = 0;
    [SerializeField] Level.State _state = Level.State.Locked;
    [SerializeField] int _sublocations = 0; //sublevels cleard
    [SerializeField] int _visits = 0; //visited times
    [SerializeField] int _passes = 0; //passed times
    [SerializeField] long _date = 0;
    [SerializeField] LocationCache _cache = new LocationCache();

    public LocationState(int loc_idx, Level.State st) // = Level.State.Locked)
    {
      _idx = loc_idx;
      _state = st;
    }
    public LocationState(int loc_idx)
    {
      _idx = loc_idx;
      if(loc_idx == Location.FeedLocation)
        _state = Level.State.Feeding;
      else if(loc_idx == Location.ClearLocation)
        _state = Level.State.Clearing;
      else
        _state = Level.State.Locked;
    }
    public int idx => _idx;
    public Level.State state { get => _state; set => _state = value;}
    public int  sublevels {get => _sublocations; set => _sublocations = value;}
    public int  visits {get => _visits; set => _visits = value;}
    public int  passes {get => _passes; set => _passes = value;}
    public long date {get => _date; set => _date = value;}
    public LocationCache cache => _cache;
  }

  [System.Serializable]
  class ProgressState
  {
    [SerializeField] int                 _location = 0;
    [SerializeField] List<LocationState> _locations;
    [SerializeField] List<LocationState> _locationsSpec;
    [SerializeField] long                _locationsPassedTime = 0;
    [SerializeField] List<Item.ID>       _itemAppears = new List<Item.ID>();

    public static Action onAllLocationFinished;

    public  int  location { get => _location; set => _location = value; }
    public  long locationsPassTime { get => _locationsPassedTime; set => _locationsPassedTime = value;}
    public  List<LocationState> locations { get => _locations; }
    public  List<LocationState> locationsSpec {get => _locationsSpec;}

    public  LocationState  FindLocation(int loc_idx)
    {
      LocationState loc_state = null;
      if(loc_idx < Location.SpecialLocBeg)
        loc_state = _locations.Find((loc) => loc.idx == loc_idx);
      else
        loc_state = _locationsSpec.Find((loc) => loc.idx == loc_idx);  
      return loc_state;
    }
    public  bool        IsLocationUnlocked(int loc_idx)
    {
      var loc = FindLocation(loc_idx);
      return (loc != null)? loc.state >= Level.State.Unlocked : false;
    }
    public  bool        IsLocationPassed(int loc_idx)
    {
      var loc = FindLocation(loc_idx);
      return (loc != null) ? loc.state == Level.State.Finished : false;
    }
    public  void        UnlockLocation(int loc_idx)
    {
      if(loc_idx < Location.SpecialLocBeg)
      {
        var loc = FindLocation(loc_idx);
        if(loc == null)
          _locations.Add(new LocationState(loc_idx, Level.State.Unlocked));
        else
        {
          if(loc.state == Level.State.Locked)
            loc.state = Level.State.Unlocked;
        }
      }
    }
    public bool         PassSublocation(int loc_idx)
    {
      bool loc_passed = false;
      var loc = FindLocation(loc_idx);
      if(loc == null)
      {
        loc = new LocationState(loc_idx, Level.State.Finished);
        _locations.Add(loc);
      }
      loc.sublevels++;
      if(loc.sublevels >=  GameData.Levels.GetLocationDesc(loc_idx).sublocationsCnt)
      {
        loc_passed = true;
        PassLocation(loc_idx);
      }

      return loc_passed;
    }
    public void         PassLocation(int loc_idx)
    {
      var loc = FindLocation(loc_idx);
      if(loc == null)
      {
        loc = new LocationState(loc_idx, Level.State.Finished);
        _locations.Add(loc);
      }
      loc.passes++;
      if(loc.idx == Location.FeedLocation)
      {
        //GameState.Feeding.levels++;
      }
      else if(loc.idx == Location.ClearLocation)
        GameState.Cleanup.level++;
      else
        loc.state = Level.State.Finished;

      loc.date = CTime.get().ToBinary();
      if(_locations.Count >= Earth.locationsCnt)
      {
        if(_locationsPassedTime == 0)
        {
          _locationsPassedTime = CTime.get().ToBinary();
          GameState.Progress.Locations.onAllLocationFinished?.Invoke();
        }
      }
    }
    public Level.State  GetState(int loc_idx)
    {
      if(loc_idx == Location.FeedLocation)
        return Level.State.Feeding;
      if(loc_idx == Location.ClearLocation)
        return Level.State.Clearing;
      var loc = FindLocation(loc_idx);
      return (loc != null) ? loc.state : Level.State.Locked;
    }
    public void         VisitLocation(int loc_idx)
    {
      var loc = FindLocation(loc_idx);
      if(loc == null)
      {
        if(loc_idx < Location.SpecialLocBeg)
        {
          _locations.Add(new LocationState(loc_idx, Level.State.Unlocked));
          loc = _locations.last();
        }
        else
        {
          _locationsSpec.Add(new LocationState(loc_idx));
          loc = _locationsSpec.last();
        }
      }
      if(loc != null)
        loc.visits++;
    }
    public int          GetLocationVisits(int loc_idx)
    {
      int visits = 0;
      var loc = FindLocation(loc_idx);
      if(loc != null)
        visits = loc.visits;
      
      return visits;  
    }
    public bool         ItemAppeared(Item.ID id)
    {
      bool ret = false;
      if(!DidItemAppear(id))
      {
        ret = true;
        _itemAppears.Add(id);
      }
      return ret;
    }
    public bool         DidItemAppear(Item.ID id) => _itemAppears.Any((iid) => Item.ID.Eq(iid, id));

    public bool         IsLocCache(int loc_idx)
    {
      if(loc_idx == Location.FeedLocation)
        return true;
      var loc = FindLocation(loc_idx);
      if(loc != null)
        return loc.cache.items.Count > 0 || loc.cache.items2.Count > 0 || loc.cache.requests.Count > 0;
      else
        return false;
    }
    public LocationCache GetLocCache(int loc_idx)
    {
      var loc = FindLocation(loc_idx);
      return loc?.cache;
    }
    public void         CacheLoc(Level lvl)
    {
      var loc_idx = lvl.locationIdx;
      var listItems = lvl.listItems;
      var listItems2 = lvl.listItems2;
      var loc = FindLocation(loc_idx);
      loc.cache.items.Clear();
      listItems.ForEach((item) => loc.cache.items.Add(new ItemCache(item)));
      loc.cache.items2.Clear();
      listItems2.ForEach((item) => loc.cache.items2.Add(new ItemCache(item)));
      loc.cache.requests.Clear();
      for(int q = 0; q < lvl.animals.Count; ++q)
      {
        loc.cache.requests.Add(new RequestCache(lvl.animals[q].garbages));
      }
    }
    public void         ClearLocCache(int loc_idx) => FindLocation(loc_idx)?.cache.Clear();
  }
  [SerializeField] ProgressState progress;

  [System.Serializable]
  class EconomyState
  {
    public int    stamina = 50;
    public int    coins = 0;
    public int    gems = 0;
    public float  rewardPoints = 0;
    public int    rewardLevel = 0;
    public long   lastStamina = 0;
  }
  [SerializeField] EconomyState economy;

  [System.Serializable]
  class ChestState
  {
    public bool shown = false;
    public List<Item.ID> listStamina = new List<Item.ID>();
    public List<Item.ID> listCoins = new List<Item.ID>();
    public List<Item.ID> listGems = new List<Item.ID>();

    public void AddReward(GameData.Rewards.Reward rew)
    {
      for(int q = 0; q < rew.stamina; ++q)
        listStamina.Add(new Item.ID(0, 0, Item.Kind.Stamina));
      for(int q = 0; q < rew.coins; ++q)  
        listCoins.Add(new Item.ID(0, 0, Item.Kind.Coin));
      for(int q = 0; q < rew.gems; ++q)  
        listGems.Add(new Item.ID(0, 0, Item.Kind.Gem));
    }
  }
  [SerializeField] ChestState chest;

  [System.Serializable]
  class StorageState
  {
    public bool shown = false;
    public List<Item.ID> listItems;
  };
  [SerializeField] StorageState storage;

  [System.Serializable]
  class SplitMachineState
  {
    [SerializeField] int _capacity = 1;

    public int capacity {get => _capacity ; set => _capacity = value;}
  }
  [SerializeField] SplitMachineState splitMachine;

  [System.Serializable]
  class FeedingState
  {
    public int animalsLevelUps = 0;
  }
  [SerializeField] FeedingState feeding;

  [System.Serializable]
  class CleanupState
  {
    public int level = 0;
  }
  [SerializeField] CleanupState cleanup;

  [System.Serializable]
  class EventsState
  {
    //public bool  tutIntroDone = false;
    //public bool  tutPremiumDone = false;
    //public bool  tutFeedingDone = false;
    //public bool  popupAllLocFinished = false;
    public bool  tutStorageBoxDone = false;
    public int   popupNoStamina = 0;
  }
  [SerializeField] EventsState events;

  [System.Serializable]
  class GameInfoState
  {
    public long appQuitTime;
  }
  [SerializeField] GameInfoState gameInfo;

  [System.Serializable]
  class AnimalsState
  {
    [System.Serializable]
    public class AnimInfo
    {
      public Animal.Type type;
      public float       kcal;
      public int         lvl;
      public AnimInfo(Animal.Type animType)
      {
        type = animType;
        kcal = 0;
        lvl = 0;
      }
      public void Feed(float kCal) => kcal += kCal;
    }
    public List<AnimInfo> animals = new List<AnimInfo>();

    public bool AnimalAppeared(Animal.Type type)
    {
      bool ret = false;
      if(!DidAnimalAppear(type))
      {
        ret = true;
        animals.Add(new AnimInfo(type));
      }
      return ret;
    }
    public bool DidAnimalAppear(Animal.Type type) => animals.Any((info) => type == info.type);
    public bool Feed(Animal.Type type, float kcal, int baseLevelUp)
    {
      bool ret = false;  
      var ainf = GetInfo(type);
      if(ainf != null)
      {
        ainf.Feed(kcal);
        if(ainf.kcal > GameData.Econo.GetFeedForLevel(ainf.lvl+1, baseLevelUp))
        {
          ainf.lvl++;
          ret = true;
        }
      }
      return ret;
    }
    public AnimInfo GetInfo(Animal.Type type) => animals.Find((info) => info.type == type);
  }
  [SerializeField] AnimalsState animals;

  public static void Init()
  {
    get().progress.UnlockLocation(0);
    if(get().economy.lastStamina == 0)
      get().economy.lastStamina = CTime.get().ToBinary();
  }

  public static class Progress
  {
    public static int locationIdx {get => get().progress.location; set => get().progress.location = value;}
    public static class Locations
    {
      public static Action<int>   onLocationPolluted;
      public static Action        onAllLocationFinished;

      public static Level.State   GetLocationState(int loc_idx) => get().progress.GetState(loc_idx);
      //public static void        SetLocationState(int loc_idx, Location.State state) => get().progress.SetLocationState(loc_idx, state);
      public static bool          IsLocationUnlocked(int loc_idx) => get().progress.IsLocationUnlocked(loc_idx);
      public static bool          IsLocationFinished(int loc_idx) => get().progress.IsLocationPassed(loc_idx);
      public static void          SetLocationFinished(int loc_idx) => get().progress.PassLocation(loc_idx);
      public static void          SetLocationFinished() => SetLocationFinished(locationIdx);
      public static void          SetLocationUnlocked(int loc_idx) => get().progress.UnlockLocation(loc_idx);
      public static bool          SetSublocationPassed(int loc_idx) => get().progress.PassSublocation(loc_idx);
      public static void          UnlockNextLocation(int loc_idx) => get().progress.UnlockLocation(GameData.Locations.NextLocation(loc_idx));
      public static void          UnlockNextLocation() => UnlockNextLocation(locationIdx);
      public static void          VisitLocation(int loc_idx) => get().progress.VisitLocation(loc_idx);
      public static int           GetLocationVisits(int loc_idx) => get().progress.GetLocationVisits(loc_idx);
      public static int           GetSublocationPassed(int loc_idx) => get().progress.FindLocation(loc_idx)?.sublevels ?? 0;
      public static Level.State[] GetStates()
      {
        var states = new Level.State[Earth.locationsCnt];
        for(int q = 0; q < states.Length; q++)
          states[q] = GetLocationState(q);

        return states;
      }
      public static bool          AllStateFinished() => GetStates().All((state) => state >= Level.State.Finished);
      public static bool          AllStateFinished(Level.State[] states) => states.All((state) => state >= Level.State.Finished);
      public static int           GetFinishedCnt() => get().progress.locations.Count((loc) => loc.state >= Level.State.Finished);
      public static float         GetCompletionRate()
      {
        Level.State[] states = GetStates();
        float finishedCnt = states.Where((state) => state == Level.State.Finished).Count();
        return finishedCnt / Earth.locationsCnt;
      }

      public static void ClearCache(int loc_idx) => get().progress.ClearLocCache(loc_idx);
      public static LocationCache GetCache(int loc_idx) => get().progress.GetLocCache(loc_idx);
      public static void Cache(Level lvl) => get().progress.CacheLoc(lvl);
      public static bool IsCache(int loc_idx) => get().progress.IsLocCache(loc_idx);

      static int _timer = 0;
      static DateTime prevTime;
      public static void Process()
      {
        if(get().progress.locationsPassTime != 0 && ++_timer % 60 == 0)
        {
          if(DateTime.FromBinary(GameInfo.appQuitTime) > prevTime)
            prevTime = DateTime.FromBinary(GameInfo.appQuitTime);

          var locs = get().progress.locations.FindAll((loc) => loc.state == Level.State.Finished);
          locs.shuffle(100);
          locs.Reverse();
          for(int q = 0; q < SystemNotificationOnTimeEx.dateTimes.Count && locs.Count > 0; ++q)
          {
            if(prevTime < SystemNotificationOnTimeEx.dateTimes[q] && CTime.get() > SystemNotificationOnTimeEx.dateTimes[q])
            {
              locs.first().state = Level.State.Polluted;
              onLocationPolluted?.Invoke(locs.first().idx);
              locs.RemoveAt(0);
            }
          }
          prevTime = CTime.get();
        }
      }
    }

    public static class Items
    {
      public static bool ItemAppears(Item.ID id) => get().progress.ItemAppeared(id);
      public static bool DidItemAppear(Item.ID id) => get().progress.DidItemAppear(id);
    }

  }
  public static class Econo
  {
    public static Action<int>   onStaminaChanged;
    public static Action<int>   onCoinsChanged;
    public static Action<int>   onGemsChanged;
    public static Action<float> onRewardProgressChanged;

    public static int stamina 
    { 
      get => get().economy.stamina; 
      set 
      {
        var _prev_val = get().economy.stamina;
        get().economy.stamina = value; //Mathf.Clamp(value, 0, GameData.Econo.staminaMax);
        if(_prev_val != get().economy.stamina)
          onStaminaChanged?.Invoke(value);
      }
    }
    public static int coins 
    { 
      get => get().economy.coins; 
      set
      { 
        var _prev_val = get().economy.coins;
        get().economy.coins = Mathf.Clamp(value, 0, GameData.Econo.coinsMax);
        if(_prev_val != value)
          onCoinsChanged?.Invoke(value);
      }
    }
    public static int gems
    {
      get => get().economy.gems; 
      set 
      {
        var _prev_val = get().economy.gems;
        get().economy.gems = Mathf.Clamp(value, 0, GameData.Econo.gemsMax);
        if(_prev_val != value)
          onGemsChanged?.Invoke(value);
      }
    }
    public static float rewards
    {
      get => get().economy.rewardPoints;
      set
      {
        var prev_points = get().economy.rewardPoints;
        get().economy.rewardPoints = value;
        if(prev_points != value)
          onRewardProgressChanged?.Invoke(value);
      }
    }
    public static bool  CanSpendStamina(int stamina_cost) => stamina >= stamina_cost;
    public static bool  CanPushoutItem() => stamina >= GameData.Econo.staminaPushoutItemCost;
    public static bool  CanSpendCoins(int coins_cost) => coins >= coins_cost;
    public static float GetStaminaRefillPerc()
    {
      float perc = 0;
      if(stamina < GameData.Econo.staminaMax)
      {
        var now = CTime.get();
        var last = DateTime.FromBinary(get().economy.lastStamina);
        var timeDiff = now - last;
        perc = 100*(float)((now - last).TotalSeconds / GameData.Econo.staminaRefillTime);
      }

      return perc;
    }
    public static int   AddRes(Item.ID id) //without event
    {
      int amount = GameData.Econo.GetResCount(id);
      if(id.kind == Item.Kind.Stamina)
        get().economy.stamina = Mathf.Clamp(get().economy.stamina + amount, 0, GameData.Econo.staminaMax);
      else if(id.kind == Item.Kind.Coin)
        get().economy.coins = Mathf.Clamp(get().economy.coins + amount, 0, GameData.Econo.coinsMax);
      else if(id.kind == Item.Kind.Gem)
        get().economy.gems = Mathf.Clamp(get().economy.gems + amount, 0, GameData.Econo.gemsMax);

      return amount;  
    }

    public static void  Process()
    {
      var eco = get().economy;
      var now = CTime.get();
      var last = DateTime.FromBinary(eco.lastStamina);
      var timeDiff = now - last;
      if(timeDiff.TotalSeconds > GameData.Econo.staminaRefillTime)
      {
        int staminaToAdd = (int)(timeDiff.TotalSeconds / GameData.Econo.staminaRefillTime);
        if(stamina < GameData.Econo.staminaMax)
        {
          stamina = Mathf.Clamp(stamina + staminaToAdd, 0, GameData.Econo.staminaMax);
          eco.lastStamina = last.AddSeconds((double)staminaToAdd * GameData.Econo.staminaRefillTime).ToBinary();
        }
        else
          eco.lastStamina = CTime.get().ToBinary();
      }
    }
  }
  public static class SplitMachine
  {
    public static int capacity {get => get().splitMachine.capacity; set => get().splitMachine.capacity = value;}
  }
  public static class Chest
  {
    public static int rewardLevel { get => get().economy.rewardLevel; set => get().economy.rewardLevel = value; }
    public static int staminaCnt => get().chest.listStamina.Count;
    public static int coinsCnt => get().chest.listCoins.Count;
    public static int gemsCnt => get().chest.listGems.Count;
    public static Item.ID? PopRes()
    {
      Item.ID? id = null;
      if(staminaCnt > 0)
      {
        id = get().chest.listStamina.last().Validate();
        get().chest.listStamina.RemoveAt(get().chest.listStamina.last_idx());
      }
      else if(coinsCnt > 0)
      {
        id = get().chest.listCoins.last().Validate();
        get().chest.listCoins.RemoveAt(get().chest.listCoins.last_idx());
      }
      else if(gemsCnt > 0)
      {
        id = get().chest.listGems.last().Validate();
        get().chest.listGems.RemoveAt(get().chest.listGems.last_idx());
      }
      return id;
    }
    public static void AddRewards()
    {
      get().chest.AddReward(GameData.Econo.GetRewards());
    }
    public static bool shown { get => get().chest.shown; set => get().chest.shown = value;}
    public static int  itemsCnt => staminaCnt + coinsCnt + gemsCnt;
    public static bool ShouldShow() => shown;
    public static bool IsFirstShow() => !shown && itemsCnt > 0;
  }
  public static class StorageBox
  {
    public static Action onItemsCntChanged;
    
    public static void PushItem(Item.ID id)
    {
      get().storage.listItems.Add(id);
    }
    public static Item.ID TopItem()
    {
      Item.ID id = new Item.ID(0, 0, Item.Kind.None);
      if(get().storage.listItems.Count > 0)
        id = get().storage.listItems[get().storage.listItems.last_idx()];
      return id;
    }
    public static Item.ID? PopItem()
    {
      Item.ID? id = null;
      if(get().storage.listItems.Count > 0)
      {
        id = TopItem();
        get().storage.listItems.RemoveAt(get().storage.listItems.last_idx());
      }
      return id;
    }
    public static int  itemsCnt => get().storage.listItems.Count;
    public static bool shown {get => get().storage.shown; set => get().storage.shown = value;}
  }
  public static class Feeding
  {
    public static void Cache(Level lvl) => GameState.Progress.Locations.Cache(lvl);
    public static LocationCache GetCache() => get().progress.GetLocCache(Location.FeedLocation);
    public static int visits => GameState.Progress.Locations.GetLocationVisits(Location.FeedLocation);
  }
  public static class Cleanup
  {
    public static void Cache(Level lvl) => GameState.Progress.Locations.Cache(lvl);
    public static int  visits => GameState.Progress.Locations.GetLocationVisits(Location.ClearLocation);
    public static int  level  {get => get().cleanup.level; set => get().cleanup.level = value;}
  }
  public static class Animals
  {
    public static bool  AnimalAppears(Animal.Type type) => get().animals.AnimalAppeared(type);
    public static bool  DidAnimalAppear(Animal.Type type) => get().animals.DidAnimalAppear(type);
    public static float GetFoodCal(Item.ID id) => GameData.Econo.GetResCount(id) * GameData.Econo.GetFoodDesc(id).kcal;
    public static bool  Feed(Animal.Type type, Item.ID id, int baseLevelUp) 
    {
      float kcal = GetFoodCal(id);
      return get().animals.Feed(type, kcal, baseLevelUp);
    }
    public static (float kcal, mr.Range<int> lvlRng, int lvl) GetInfo(Animal.Type type, int baseLevelUp)
    {
      var ainfo = get().animals.GetInfo(type);
      var lev_beg = (int)GameData.Econo.GetFeedForLevel(ainfo.lvl, baseLevelUp);
      var lev_end = (int)GameData.Econo.GetFeedForLevel(ainfo.lvl+1, baseLevelUp);
      return new (ainfo.kcal, new mr.Range<int>(lev_beg, lev_end), ainfo.lvl);
    }
  }
  public static class Events
  {
    public static class Tutorials
    {
      public static bool storgeBoxDone { get => get().events.tutStorageBoxDone; set => get().events.tutStorageBoxDone = value;}
    }
    public static class Popups
    {
      public static int  noStaminaShown {get => get().events.popupNoStamina; set => get().events.popupNoStamina = value;}
    }
  }
  public static class GameInfo
  {
    public static long appQuitTime {get => get().gameInfo.appQuitTime; set => get().gameInfo.appQuitTime = value;}
  }

  [Header("Customization")]
  [SerializeField] int selectedTheme = 0;
  public int SetNextTheme() {
    selectedTheme = (int)Mathf.Repeat(++selectedTheme, GameData.GetThemeColors().Length);
    return selectedTheme;
  }
}
