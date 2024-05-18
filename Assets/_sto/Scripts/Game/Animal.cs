using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameLib;

public class Animal : MonoBehaviour
{
  [field: SerializeField] public string DisplayName { get; private set; } = "";
  [Header("Refs")]
  [SerializeField] Animator     _animator;
  [SerializeField] Transform    _garbageContainer;
  [SerializeField] FeedInfo     _feedingInfo;
  [SerializeField] GarbageInfo  _garbageInfo;

  [Header("Props")]
  [SerializeField] Type _type;
  [SerializeField] int  _baseLevelUp = 100;


  public static System.Action<Animal> onFeed, onLevelUp;

  public enum Type
  {
    None,
    Dolphin,
    Turtle,
    Hammerfish,
    Octopus,
  }

  List<Item.ID>    _garbagesIds = new List<Item.ID>(); //initial garbages
  List<Item.ID>    _garbages = new List<Item.ID>(); //garbages left
  List<Item>       _garbagesCleared = new List<Item>();

  public Type          type => _type;
  public int           baseLevelUp => _baseLevelUp;
  public List<Item>    garbagesView {get; private set;} = new List<Item>();
  public bool          isActive  {get; private set;} = false;
  public bool          isReady  {get; private set;} = false;
  public int           requests => garbagesView.Count;
  public List<Item.ID> garbages => _garbages;
  public List<Item.ID> garbagesIds => _garbagesIds;
  //public Vector3       garbagePos => _garbageContainer.transform.position;
  public float         lastkCal {get; private set;} = 0;

  static public int layer = 0;
  static public int layerMask = 0;

  bool feedingMode = false;

  void Awake()
  {
    layer = gameObject.layer;
    layerMask = LayerMask.GetMask(LayerMask.LayerToName(layer));
  }

  IEnumerator ShowInfo()
  {
    yield return _animator.WaitForAnimState("_active");
    isReady = true;
    if(!feedingMode)
      _garbageInfo.Show(garbagesView);
    else
      _feedingInfo.Show(this);
  }
  public void Init(GameData.GarbCats[] garbCats)
  {
    _garbagesIds = new List<Item.ID>();
    feedingMode = Level.mode == Level.Mode.Feeding;

    if(!feedingMode)
    {
      foreach(var gcat in garbCats)
      {
        var gcatup = gcat;
        var item = GameData.Prefabs.GetGarbagePrefab(gcatup);
        _garbagesIds.Add(item.id);
      }
      _garbages.AddRange(_garbagesIds);
      garbagesView.Clear();
      foreach(var id in _garbagesIds)
      {
        garbagesView.Add(GameData.Prefabs.CreateBagStaticItem(id, _garbageInfo.itemContainer));
      }
    }
  }
  public void Init(List<Item.ID> ids)
  {
    _garbagesIds = new List<Item.ID>();
    feedingMode = Level.mode == Level.Mode.Feeding;
    if(!feedingMode)
    {
      _garbagesIds.AddRange(ids);
      garbagesView.Clear();
      foreach(var id in _garbagesIds)
      {
        garbagesView.Add(GameData.Prefabs.CreateBagStaticItem(id, _garbageInfo.itemContainer));
      }
      _garbages.AddRange(_garbagesIds);
    }
  }

  public void Activate(bool show_info)
  {
    isActive = true;
    _animator.SetTrigger("activate");
    GetComponent<Collider>().enabled = true;
    if(show_info)
      StartCoroutine(ShowInfo());
  }
  public void Deactivate()
  {
    isReady = false;
    isActive = false;
    _garbageInfo.Hide();
    _animator.SetTrigger("deactivate");
    GetComponent<Collider>().enabled = false;
    _animator.InvokeForAnimStateEnd("_deactivate", ()=> gameObject.SetActive(false));
  }
  public void SetInactive()
  {
    isReady = false;
    isActive = false;
    GetComponent<Collider>().enabled = false;
    gameObject.SetActive(false);
  }
  public void AnimLvl()
  {
    isReady = false;
    isActive = false;
    _garbageInfo.Hide();
    _feedingInfo.Hide();
    _animator.SetTrigger("deactivate");
    GetComponent<Collider>().enabled = false;
    this.Invoke(() => this.Activate(true), 3.0f);
    //_animator.InvokeForAnimState("_inactive", ()=> Activate(true));
  }
  public void AnimThrow()
  {
    if(isReady)
      _animator.Play("itemPush");
  }
  public void AnimTalk()
  {
    if(isReady)
      _animator.Play("talk", 0);
  }
  public bool CanPut(Item item)
  {
    bool ret = false;
    if(isReady)
    {
      if(!feedingMode)
        ret = IsReq(item);
      else
        ret = item.id.kind == Item.Kind.Food;
    }
    return ret;
  }
  public Item GetReq(Item item) => garbagesView.Find((garbage) => Item.EqType(item, garbage));
  public Item GetReq(Item.ID id) => garbagesView.Find((garbage) => Item.ID.Eq(id, garbage.id));
  public bool IsReq(Item item) => (!feedingMode)? GetReq(item) != null : item.id.kind == Item.Kind.Food;
  public bool IsReq(Item.ID id) => (!feedingMode)? GetReq(id) != null : id.kind == Item.Kind.Food;
  public void Feed(Item item)
  {
    bool next_lvl = GameState.Animals.Feed(type, item.id, _baseLevelUp);
    _feedingInfo.UpdateInfo();
    lastkCal = GameState.Animals.GetFoodCal(item.id);
    onFeed?.Invoke(this);
    if(next_lvl)
      onLevelUp?.Invoke(this);
    isReady = true;
    item.gameObject.SetActive(false);
    if(!next_lvl)
      AnimTalk();
    else
      AnimLvl();
  }
  public void Put(Item item)
  {
    //if(isReady)
    {
      Item it = garbagesView.Find((garbage) => Item.EqType(garbage, item));
      if(it)
      {
        _garbageInfo.Remove(it.id);
        _garbagesCleared.Add(it);
        _garbages.Remove(it.id);
        garbagesView.Remove(it);
        item.gameObject.SetActive(false);
        GameObject model = null;
        if(isReady)
        {
          model = item.mdl;
          model.transform.parent = _garbageContainer;
          model.transform.localPosition = Vector2.zero;
          model.SetActive(true);
        }

        if(_garbages.Count > 0)
        {
          if(isReady)
          {
            AnimThrow();
            StartCoroutine(_animator.InvokeForAnimStateEnd("itemPush", ()=>
            {
              isReady = true;
              model.SetActive(false);
            }));
          }
          isReady = false;
        }
        else
        {
          isReady = false;
          Deactivate();
        }
      }
    }
  }
}
