using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameLib;


[CreateAssetMenu, DefaultExecutionOrder(-2)]
public class GameData : ScriptableObject
{
  private static GameData _static_this = null;
  public static  GameData get(){ return _static_this; }
  GameData()
  {
    _static_this = this;
  }

  public enum GarbCats
  {
    Cat_00_lvl0 = 0,
    Cat_00_lvl1 = 1,
    Cat_00_lvl2 = 2,
    Cat_00_lvl3 = 3,
    Cat_00_lvl4 = 4,
    Cat_00_lvl5 = 5,
    Cat_00_lvl6 = 6,
    Cat_00_lvl7 = 7,

    Cat_01_lvl0 = 10,
    Cat_01_lvl1 = 11,
    Cat_01_lvl2 = 12,
    Cat_01_lvl3 = 13,
    Cat_01_lvl4 = 14,
    Cat_01_lvl5 = 15,
    Cat_01_lvl6 = 16,

    Cat_02_lvl0 = 20,
    Cat_02_lvl1 = 21,
    Cat_02_lvl2 = 22,
    Cat_02_lvl3 = 23,
    Cat_02_lvl4 = 24,
    Cat_02_lvl5 = 25,
    Cat_02_lvl6 = 26,

    Cat_03_lvl0 = 30,
    Cat_03_lvl1 = 31,
    Cat_03_lvl2 = 32,
    Cat_03_lvl3 = 33,
    Cat_03_lvl4 = 34,
    Cat_03_lvl5 = 35,
    Cat_03_lvl6 = 36,
  }

  public static void Init()
  {
    var items = get()._items;
    for(int type = 0; type < items.Length; ++type)
    {
      var item = items[type];
      //Debug.Log("kind: " + item.kind);
      for(int lvl = 0; lvl < item.Count; ++lvl)
      {
        item.Get(lvl).id = new Item.ID(type, lvl, items[type].kind);
      }
    }
  }

  [System.Serializable]
  struct Items
  {
    [SerializeField] Item.Kind _kind;
    [SerializeField] Item[] _items;
    public Item.Kind kind => _kind;
    public Item Get(int idx) => _items[Mathf.Clamp(idx, 0, _items.Length -1)];
    public int  Count => _items.Length;
  }

  [System.Serializable]
  public struct Rewards
  {
    public int points2Chest;
    [System.Serializable]
    public struct Reward
    {
      public int stamina;
      public int coins;
      public int gems;
    }
    public Reward _reward;
  }

  [System.Serializable]
  public struct FoodDesc
  {
    [SerializeField] string  name;

    public float foodChance;
    public float kcal;
  }

  [Header("--Prefabs--")]
  [SerializeField] Items[]  _items;
  [SerializeField] GridTile _gridTile;
  [SerializeField] Location _locationPrefab;
  //[SerializeField] Earth    _earthPrefab;
  //[Header("Levels")]
  [SerializeField] List<Level> _listLevels;
  [SerializeField] Level       _levelFeeding;
  [SerializeField] Level       _levelClearing;
  [Header("--Econo--")]
  [SerializeField] int        _staminaMax = 99;
  [SerializeField] int        _staminaPlayCost = 5;
  [SerializeField] int        _staminaFeedCost = 1;
  [SerializeField] int        _staminaClearCost = 10;
  [SerializeField] int        _staminaAdReward = 15;
  [SerializeField] float      _staminaRefillTime = 60.0f;
  [SerializeField] int        _coinFeedCost = 1;
  [SerializeField] int        _coinsMax = 999;
  [SerializeField] int        _gemsStaminaRefillCost = 10;
  [SerializeField] int        _gemsMax = 999;
  [SerializeField] float      _resouceItemsAmountFactor = 1.5f;
  [SerializeField] FoodDesc[] _foodsDesc = null;
  [SerializeField] float      _animalLevelFactor = 1.25f;
  [SerializeField] Rewards[]  _rewards;

  [Header("--Settings--")]
  [SerializeField] int        _feedingAvailLoc = 3;
  [SerializeField] int        _clearingAvailLoc = 4;
  [SerializeField] bool       _disableStorageBox = true;
  [SerializeField] bool       _disableRewardChest = true;

  [SerializeField] Color[]    themeColors;
  public static Color[]       GetThemeColors() => get().themeColors;


  public static class Prefabs
  {
    static List<Items>     garbages = new List<Items>();
    public static Item     GetItemPrefab(Item.ID id) => get()._items[id.type].Get(id.lvl);
    public static Item     GetGarbagePrefab(GarbCats cat)
    {
      var garbage_items = System.Array.FindAll(get()._items, (items_) => items_.kind == Item.Kind.Garbage);
      int garb_type = Mathf.Clamp((int)cat / 10, 0, garbage_items.Length-1);
      return garbage_items[garb_type].Get((int)cat % 10);
    }
    public static GridTile CreateGridElem(Transform parent)
    {
      return Instantiate(get()._gridTile, parent);
    }
    public static Item CreateItem(Item.ID id, Transform parent)
    {
      Item item = null;
      if(id.type < 0)
        id.type = UnityEngine.Random.Range(0, ItemTypesCnt);
      item = Instantiate(get()._items[id.type].Get(id.lvl), parent);
      id.kind = get()._items[id.type].kind;
      item.id = id;
      return item;
    }
    public static Item CreateStaticItem(Item.ID id, Transform parent)
    {
      Item item = CreateItem(id, parent);
      item.SetAsStatic();
      item.enabled = false;
      return item;
    }
    public static GameObject[] CreateStaticItemModels(Item.ID id, Transform parent, int count)
    {
      id.lvl = 0;
      Item item = CreateItem(id, parent);
      item.Init(Vector2.zero);
      GameObject[] models = new GameObject[count];
      for(int q = 0; q < count; ++q)
        models[q] = Instantiate(item.mdl, parent);
      Destroy(item.gameObject);

      return models;
    }
    public  static int ItemLevelsCnt(Item.ID id)
    {
      int levels = 0;
      if(id.kind == Item.Kind.Garbage || id.kind == Item.Kind.Food)
        levels = get()._items[id.type].Count;
      else
        levels = get()._items[id.type].Get(0).modelContainer.childCount;
      return levels;
    }
    public static int ItemTypeFromKind(Item.Kind kind)
    {
      return Array.FindIndex(get()._items, (item) => item.kind == kind);
    }
    public static int[] ItemTypesOfKind(Item.Kind kind)
    {
      List<int> types = new List<int>();
      for(int q = 0; q < ItemTypesCnt; ++q)
      {
        if(get()._items[q].kind == kind)
          types.Add(q);
      }

      return types.ToArray();
    }
    public static int ItemTypesCnt => get()._items.Length;

    public static Location CreateLocation(Transform parent) => Instantiate(get()._locationPrefab, parent);
  }
  public static class Levels
  {
    static public Level GetPrefab(int idx) => get()._listLevels[idx];
    static public Level CreateLevel(int idx, Transform levelsContainer) => Instantiate(get()._listLevels[idx], levelsContainer);
    static public Level CreateFeedingLevel(Transform levelsContainer) => Instantiate(get()._levelFeeding, levelsContainer);
    static public Level CreateClearingLevel(Transform levelsContainer) => Instantiate(get()._levelClearing, levelsContainer);
    static public int   levelsCnt => get()._listLevels.Count;
    //static public Vector2Int feedingDim => get()._feedingBoardDim;
    static public int   GetFeedingAvailLoc() => get()._feedingAvailLoc;
    static public int   GetClearingAvailLoc() => get()._clearingAvailLoc;
  }
  public static class Locations
  {
    static public int PrevLocation(int loc_idx)
    {
      return Mathf.Clamp(loc_idx - 1, 0, Earth.locationsCnt - 1);
    }
    static public int NextLocation(int loc_idx)
    {
      return Mathf.Clamp(loc_idx + 1, 0, Earth.locationsCnt - 1);
    }
  }
  public static class Econo
  {
    public static int   staminaPlayCost => get()._staminaPlayCost;
    public static int   staminaPlayFeedCost => get()._staminaFeedCost;
    public static int   staminaPlayClearCost => get()._staminaClearCost;
    public static int   staminaAdReward => get()._staminaAdReward;
    public static float staminaRefillTime => get()._staminaRefillTime;
    public static int   staminaMax => get()._staminaMax;
    public static int   coinsMax => get()._coinsMax;
    public static int   coinFeedCost => get()._coinFeedCost;
    public static int   staminaRefillGemsCost => get()._gemsStaminaRefillCost;
    public static int   gemsMax => get()._gemsMax;
    public static FoodDesc[] foodsDesc => get()._foodsDesc;
    public static float animalLevelFactor => get()._animalLevelFactor;
    public static float GetFeedForLevel(int lvl, int baseLevelUp) => (lvl > 0)? baseLevelUp * Mathf.Pow(GameData.Econo.animalLevelFactor,  lvl-1) : 0;

    public static FoodDesc GetFoodDesc(Item.ID foodId)
    {
      int[] foods_type = GameData.Prefabs.ItemTypesOfKind(foodId.kind);
      int idx = Mathf.Clamp(foodId.type - foods_type[0], 0, foods_type.Length-1);
      return foodsDesc[idx];
    }
    public static int   GetResCount(Item.ID id) => (int)Mathf.Pow(id.lvl + 1, get()._resouceItemsAmountFactor);// ((1 << id.lvl) * get()._resouceItemsAmountFactor);
    public struct RewardProgress
    {
      public int    lvl;
      public float  progress_points;
      public float  progress_range_lo;
      public float  progress_range_hi;

      public RewardProgress(int lvl_idx)
      {
        lvl = lvl_idx;
        progress_points = 0;
        progress_range_lo = 0;
        progress_range_hi = 0;
      }
    }
    public static int RewardChestValue(int lvl)
    {
      return get()._rewards[Mathf.Clamp(lvl, 0, get()._rewards.last_idx())].points2Chest;
    }
    //public static int
    public static RewardProgress GetRewardProgress(float rewardPoints)
    {
      int rewardIdx = Array.FindLastIndex(get()._rewards, (Rewards rewards) => rewardPoints >= rewards.points2Chest);
      var rp = new RewardProgress(Mathf.Max(rewardIdx, GameState.Chest.rewardLevel));
      rp.progress_range_lo = RewardChestValue(rp.lvl);
      rp.progress_range_hi = RewardChestValue(rp.lvl +1);
      rp.progress_points = rewardPoints;

      return rp;
    }
    public static Rewards.Reward GetRewards()
    {
      return GetRewards(GameState.Chest.rewardLevel);
    }
    public static Rewards.Reward GetRewards(int chestLvl)
    {
      int idx = Mathf.Min(chestLvl, get()._rewards.last_idx());
      return get()._rewards[idx]._reward;
    }
  }

  public static class Settings
  {
    public static bool GetDisableStorageBox() => get()._disableStorageBox;
    public static bool GetDisableRewardChest() => get()._disableRewardChest;
  }
}