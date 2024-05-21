using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPLbl = TMPro.TextMeshProUGUI;

public class Ship : MonoBehaviour
{
  [field: SerializeField] public string DisplayName { get; private set; } = "";
  [Header("Refs")]
  [SerializeField] Animator     _animator;
  [SerializeField] Transform    _garbageContainer;
  [SerializeField] GarbageInfo  _garbageInfo;
  [SerializeField] TMPLbl       _lbl;

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

  public List<Item.ID>    _garbagesIds = new List<Item.ID>(); //initial garbages
  public List<Item.ID>    _garbages = new List<Item.ID>(); //garbages left
  public List<Item.ID>    _garbagesCleared = new List<Item.ID>();

  public  Type          type => _type;
  public  int           baseLevelUp => _baseLevelUp;
  //public  List<Item>    garbagesView {get; private set;} = new List<Item>();
  public  bool          isActive  {get; private set;} = false;
  public  bool          isReady  {get; private set;} = false;
  public  int           requests => garbages.Count;
  public  List<Item.ID> garbages => _garbages;
  public  float         lastkCal {get; private set;} = 0;

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
    {
      _garbageInfo.Show(garbages);
      UpdateText();
    }
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
    }
  }
  public void Init(List<Item.ID> ids)
  {
    _garbagesIds = new List<Item.ID>();
    feedingMode = Level.mode == Level.Mode.Feeding;
    if(!feedingMode)
    {
      _garbagesIds.AddRange(ids);
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
    //_animator.SetTrigger("deactivate");
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
    //_animator.SetTrigger("deactivate");
    GetComponent<Collider>().enabled = false;
    this.Invoke(() => this.Activate(true), 3.0f);
    //_animator.InvokeForAnimState("_inactive", ()=> Activate(true));
  }
  public void AnimThrow()
  {
    // if(isReady)
    //   _animator.Play("itemPush");
  }
  public void AnimTalk()
  {
    // if(isReady)
    //   _animator.Play("talk", 0);
  }
  public bool CanPut(Item item)
  {
    bool ret = false;
    if(isReady)
      ret = IsReq(item);

    return ret;
  }
  //public Item GetReq(Item item) => _garbagesView.Find((garbage) => Item.EqType(item, garbage));
  public Item.ID GetReq(Item.ID id) => _garbages.FirstOrDefault((garbage_id) => Item.ID.Eq(id, garbage_id));
  public bool IsReq(Item item) => (!feedingMode)? GetReq(item.id).kind != Item.Kind.None : item.id.kind == Item.Kind.Food;
  public bool IsReq(Item.ID id) => (!feedingMode)? GetReq(id).kind != Item.Kind.None : id.kind == Item.Kind.Food;
  public void Put(Item item)
  {
    //if(isReady)
    {
      Item.ID id = garbages.FirstOrDefault((garbage) => Item.ID.Eq(garbage, item.id));
      if(id.kind != Item.Kind.None)
      {
        _garbageInfo.Remove(id);
        _garbagesCleared.Add(id);
        _garbages.Remove(id);
        UpdateText();
        //garbagesView.Remove(it);
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
  void UpdateText()
  {
    _lbl.text = $"x{_garbages.Count}";
  }
}
