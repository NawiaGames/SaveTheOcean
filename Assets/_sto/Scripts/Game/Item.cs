using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLib;
using GameLib.Utilities;

public class Item : MonoBehaviour
{
  [Header("Refs")]
  [SerializeField] GameObject         _modelContainer;
  [SerializeField] ActivatableObject  _activatable;
  [SerializeField] Rigidbody          _rb;
  [SerializeField] Transform          _fx;
  [SerializeField] GameObject         _tickIco;
  [Header("Settings")]

  List<GameObject> _models = new List<GameObject>();

  public enum Kind
  {
    None,
    Garbage,
    Stamina,
    Coin,
    Gem,
    Food,
  }
  public enum MergeType
  {
    Ok,
    RejectMaxed,
    RejectWrongType,
    RejectWrongAnim,
  }

  [System.Serializable]
  public struct ID
  {
    [SerializeField] int  _type;
    [SerializeField] int  _lvl;
    [SerializeField] Kind _kind;

    public ID(int item_type, int item_lvl, Kind item_kind, bool clampLevel = false)
    {
      _type = item_type;
      _lvl = item_lvl;
      _kind = item_kind;
      if(clampLevel)
        _lvl = Mathf.Min(item_lvl, Item.LevelsCnt(this) - 1);
    }
    public ID Validate()
    {
      int[] types = GameData.Prefabs.ItemTypesOfKind(_kind);
      _type = Mathf.Clamp(_type, types.first(), types.last());
      _lvl = Mathf.Min(_lvl, Item.LevelsCnt(this) - 1);
      return this;
    }
    public int type {get => _type; set => _type = value;}
    public int lvl {get => _lvl; set => _lvl = value;}
    public Kind kind {get => _kind; set => _kind = value;}
    public bool IsSpecial => _kind == Kind.Stamina || _kind == Kind.Coin || _kind == Kind.Gem;
    public static bool Eq(ID id0, ID id1) => id0.type == id1.type && id0.lvl == id1.lvl;
    public int LevelsCnt => Item.LevelsCnt(this);
    public static ID FromKind(Item.Kind kind, int idx, int lvl = 0)
    {
      int[] types = GameData.Prefabs.ItemTypesOfKind(kind);
      var id = new ID(types[idx], lvl, kind);
      return id.Validate();
    }
  }

  ID         _id = new ID();
  float      _lifetime = 0;
  Vector2    _grid = Vector2.zero;
  Vector2Int _agrid = Vector2Int.zero;
  Vector3[]  _path = new Vector3[4];
  Vector3?   _vdstPos = null;
  Vector3    _vdim = Vector3.one;
  Vector3    _vextent = Vector3.zero;
  Vector3    _vbtmExtent = Vector3.zero;
  Vector3    _vmin = Vector3.zero;
  Vector3    _vmax = Vector3.zero;
  //float      _phaseOffs = 0;
  bool       _inMachine = false;
  float      _sinkTimer = 0;
  bool       _ready = false;
  //bool       _staticItem = false;
  bool       _levelFinished = false;
  Quaternion _qinitial;

  public static float gridSpace = 1.0f;
  public static System.Action<Item> onShow, onShown, onMerged, onPut, onNoPut, onHide, onNoMerged, onSelect, onDropped, onSpawn;
  public static Item Merge(Item item0, Item item1, List<Item> _items, List<Item.ID> required)
  {
    Item newItem = null;
    MergeType mergeType = MergeType.Ok;
    item0.mergeType = mergeType;

    if(EqType(item0, item1))
    {
      if(item1.IsUpgradable)
      {
        item0.Hide();
        _items.Remove(item0);
        onMerged?.Invoke(item1);
        newItem = Upgrade(item1, _items, required);
      }
      else
        mergeType = MergeType.RejectMaxed;
    }
    else
      mergeType = MergeType.RejectWrongType;

    if(newItem == null)
    {
      item0.mergeType = mergeType;
      onNoMerged?.Invoke(item0);
    }

    return newItem;
  }
  public static bool Mergeable(Item item0, Item item1)
  {
    return EqType(item0, item1) && item1.IsUpgradable;
  }
  public static Item[] Split(Item item, List<Item> _items)
  {
    Item[] new_items = null;
    if(item.id.lvl > 0)
    {
      item.decLvl();
      new_items = new Item[2];
      new_items[0] = GameData.Prefabs.CreateItem(item.id, item.transform.parent);
      new_items[0]._inMachine = true;
      new_items[0].Init(Vector2.zero);
      new_items[1] = GameData.Prefabs.CreateItem(item.id, item.transform.parent);
      new_items[1]._inMachine = true;
      new_items[1].Init(Vector2.zero);
      //item.Hide();

      _items.Remove(item);
      _items.Add(new_items[0]);
      _items.Add(new_items[1]);
    }

    return new_items;
  }
  public static Item   Upgrade(Item item, List<Item> _items, List<Item.ID> req)
  {
    Item new_item = null;
    if(item.IsUpgradable)
    {
      if(item.id.kind == Item.Kind.Garbage || item.id.kind == Item.Kind.Food)
      {
        item.incLvl();
        if(item.id.kind == Kind.Garbage && req.FindIndex((r) => ID.Eq(r, item.id)) >= 0)
          new_item = GameData.Prefabs.CreateBagItem(item.id, item.transform.parent);
        else
          new_item = GameData.Prefabs.CreateItem(item.id, item.transform.parent);
        item.Hide();
        new_item.Init(item.vgrid);
        new_item.GetComponent<Collider>().enabled = true;
        new_item.transform.position = item.transform.position;
        new_item._rb.velocity = Vector3.zero;
        new_item._rb.MovePosition(item.vwpos);
        new_item._rb.AddForce(new Vector3(0, -50, 0));
        new_item._rb.AddTorque(Random.rotationUniform * (Vector3.right * Random.Range(5.0f, 10.0f)));
        new_item._ready = true;

        _items.Remove(item);
        _items.Add(new_item);
      }
      else
      {
        new_item = item;
        item.incLvl();
        item.SetModel(item.id.lvl);
        new_item._rb.AddForce(new Vector3(0, -50, 0));
        new_item._rb.AddTorque(Random.rotationUniform * (Vector3.right * Random.Range(5.0f, 10.0f)));
      }
    }
    return new_item;
  }
  public static Vector3 ToPos(Vector2 vgrid) => new Vector3(vgrid.x, 0, vgrid.y) * Item.gridSpace + new Vector3(Random.Range(-0.125f, 0.125f), 0, Random.Range(-0.125f, 0.125f));
  public static bool    EqType(Item item0, Item item1) => item0 != null && item1 != null && ID.Eq(item0.id, item1.id);
  public static int     LevelsCnt(Item.ID id) => GameData.Prefabs.ItemLevelsCnt(id);

  static public int layer = 0;
  static public int layerMask = 0;

  public ID         id { get => _id; set { _id = value; } }
  public GameObject mdl {get; private set;}
  public Vector3    vdim => _vdim;
  public Vector3    vbtmExtent => _vbtmExtent;
  public Vector2    vgrid {get => _grid; set => _grid = value;}
  public Vector2Int agrid {get => _agrid; set => _agrid = value;}
  public Vector3    vlpos {get => transform.localPosition; set => transform.localPosition = value;}
  public Vector3    vwpos { get => transform.position; set => transform.position = value;}
  public Vector3?   vdstPos {get=> _vdstPos; set => _vdstPos = value;}
  public Vector3    gridPos => Item.ToPos(vgrid);
  public bool       IsMaxLevel => id.lvl + 1 == levelsCnt;
  public bool       IsUpgradable => id.lvl + 1 < levelsCnt;
  public bool       IsSplitable => id.lvl > 0;
  public bool       IsSelected {get; set;}
  public bool       IsKinematic {get => _rb.isKinematic; set => _rb.isKinematic = value;}
  public bool       IsInMachine {get => _inMachine; set => _inMachine = value;}
  public void       incLvl(int amount = 1){_id.lvl = Mathf.Clamp(_id.lvl + amount, 0, _id.LevelsCnt-1);}
  public void       decLvl(){if(_id.lvl > 0) _id.lvl--;}
  public MergeType  mergeType {get; set;} = MergeType.Ok;
  public int        levelsCnt {get; private set;}
  public Transform  modelContainer => _modelContainer.transform;
  public bool       tickIco
  {
    get => _tickIco.activeInHierarchy;
    set => _tickIco.SetActive(value);
  }
  bool  levelsAsModels => id.IsSpecial;

  void Awake()
  {
    layer = gameObject.layer;
    layerMask = LayerMask.GetMask(LayerMask.LayerToName(layer));

    for(int q = 0; q < _modelContainer.transform.childCount; ++q)
      _models.Add(_modelContainer.transform.GetChild(q).gameObject);

    _tickIco.SetActive(false);
  }
  public void SetAsStatic()
  {
    _activatable.ActivateObject();
    GetComponent<BoxCollider>().enabled = false;
    Destroy(_rb);
    _rb = null;

    transform.localScale = Vector3.one;
    var osir = GetComponentInChildren<ObjectSpinInRange>(true);
    if(osir)
    {
      osir.transform.localRotation = Quaternion.identity;
      osir.enabled = false;
    }
    System.Array.ForEach(GetComponentsInChildren<ObjectRandomizeTransform>(), (ort) => ort.transform.reset());
    mdl = _models[0];
    SetModel(0);
  }
  public void Init(Vector2 grid)
  {
    vgrid = grid;
    vlpos = ToPos(vgrid);
    GetComponent<BoxCollider>().enabled = false;
    levelsCnt = Item.LevelsCnt(id);
    if(levelsAsModels) //id.kind == Kind.Garbage || id.kind == Kind.Food)
      SetModel(id.lvl);
    else
      SetModel(0);
  }
  void SetModel(int model_idx)
  {
    for(int q = 0; q < _models.Count; ++q)
    {
      if(q == model_idx)
      {
        mdl = _models[q];
        _models[q].SetActive(true);
      }
      else
        _models[q].SetActive(false);
    }

    _vdim = Vector3.zero;
    Renderer[] renderers = mdl.GetComponentsInChildren<Renderer>(true);
    var _center = Vector3.zero;
    System.Array.ForEach(renderers, (rend) =>
    {
      _center = rend.bounds.center;
      _vdim = Vector3.Max(_vdim, rend.bounds.size);
      _vmin = Vector3.Min(_vmin, rend.bounds.min);
      _vmax = Vector3.Max(_vmax, rend.bounds.max);
      _vextent = Vector3.Max(_vextent, rend.bounds.extents);
    });
    _vbtmExtent.y = -(_center.y - _vdim.y * 0.5f);

    _qinitial = mdl.transform.localRotation;
  }
  public bool IsReady => _ready; //!_activatable.InTransition && _lifetime > 0.125f;
  public void Show()
  {
    _activatable.ActivateObject();
    this.Invoke(()=> GetComponent<Collider>().enabled = true, 0.5f);
    onShow?.Invoke(this);
  }
  public void Spawn(Vector2 vgrid, Vector3[] vpath, float touch, float speed, float delay = 0)
  {
    Init(vgrid);
    gameObject.SetActive(true);
    GetComponent<Collider>().enabled = false;
    _activatable.ActivateObject();
    if(vpath != null)
    {
      System.Array.Copy(vpath, _path, 3);
      _path[3] = ToPos(vgrid);
      vwpos = _path[0];
      _rb.MovePosition(vwpos);
      StartCoroutine(MovePath(speed));
    }
    else
    {
      float immers = -6;
      _path[0] = ToPos(vgrid);
      _path[0].y = immers;
      _path[3] = ToPos(vgrid);

      _path[1] = Vector3.Lerp(_path[0], _path[3], 0.25f);
      _path[2] = Vector3.Lerp(_path[0], _path[3], 0.75f);

      vwpos = _path[0];
      _rb.MovePosition(vwpos);
      onShow?.Invoke(this);
      StartCoroutine(SwimUp(delay));
    }
  }
  IEnumerator SwimUp(float delay)
  {
    yield return new WaitForSeconds(delay);

    GetComponent<Collider>().enabled = true;
    _rb.AddTorque(Random.rotationUniform * (Vector3.forward * Random.Range(150, 200)));
    _rb.AddForce(Vector3.up * 250);

    yield return new WaitUntil(() => vlpos.y > -0.5f);
    onShown?.Invoke(this);
    _ready = true;
  }
  IEnumerator MovePath(float speed)
  {
    float t = 0.0f;
    _rb.MovePosition(Vector3Ex.bezier(_path, 0));
    _rb.AddTorque(Random.rotationUniform * (Vector3.forward * Random.Range(150, 200)));
    GetComponent<Collider>().enabled = false;
    while(t <= 1)
    {
      float prev_t = t;
      t += Time.deltaTime * speed;
      float tc = Mathf.Clamp01(t);
      _rb.MovePosition(Vector3Ex.bezier(_path, tc));
      if(prev_t < 0.5f && t >= 0.5f)
        GetComponent<Collider>().enabled = true;
      if(prev_t < 0.95f && t >= 0.95f)
        onShown?.Invoke(this);
      yield return null;
    }
    GetComponent<Collider>().enabled = true;
    _path = null;
    yield return new WaitForSeconds(0.125f);
    _ready = true;
  }
  public void Hide(bool silent = false)
  {
    gameObject.SetActive(false);
    onHide?.Invoke(this);
  }
  public void Deactivate()
  {
    _activatable.DeactivateObject();
  }
  public void LevelOut()
  {
    _levelFinished = false;
    _rb.isKinematic = true;
    GetComponent<Collider>().enabled = false;
  }
  Vector3   _vBackPos = Vector3.zero;
  Coroutine _coMoveHandle = null;
  public void Select(bool sel)
  {
    var coll = GetComponent<Collider>();
    IsSelected = sel;
    onSelect?.Invoke(this);
    if(sel)
    {
      if(_coMoveHandle != null)
        StopCoroutine(_coMoveHandle);
      _vBackPos = vlpos;
    }
  }
  public void MoveTo(Vector3 dst) => _rb.MovePosition(dst);
  public void MoveToGrid()
  {
    _coMoveHandle = StartCoroutine(coMoveToGrid());
  }
  IEnumerator coMoveToGrid()
  {
    // float speed = Time.deltaTime * 20;
    // while(Vector3.Distance(vlpos, vdst) > 0.01f)
    // {
    //   vlpos = vlpos = Vector3.MoveTowards(vlpos, vdst, speed); //Vector3.Lerp(vlpos, vdst, Time.deltaTime * 8);
    //   speed *= 1 + Time.deltaTime * 6;
    //   yield return null;
    // }
    // vlpos = vdst;
    _rb.AddForce(Vector3.down * 5);
    yield return null;
    _coMoveHandle = null;
    onDropped?.Invoke(this);
  }
  public void DragEnd()
  {
    _coMoveHandle = StartCoroutine(coDragEnd());
  }
  IEnumerator coDragEnd()
  {
    // var vdst = (IsInMachine)? _vBackPos : Item.ToPos(vgrid);
    // float speed = Time.deltaTime * 20;
    // while(Vector3.Distance(vlpos, vdst) > 0.01f)
    // {
    //   vlpos = Vector3.MoveTowards(vlpos, vdst, speed); //Vector3.Lerp(vlpos, vdst, Time.deltaTime * 8);
    //   speed *= 1 + Time.deltaTime * 6;
    //   yield return null;
    // }
    // vlpos = vdst;
    _rb.AddForce(Vector3.down * 5);
    yield return null;
    _coMoveHandle = null;
    onDropped?.Invoke(this);
  }
  public void Hover(bool act)
  {
    if(act && !IsSelected)
      _rb.AddForce(Vector3.down * 5);
  }
  public void MoveSelectedTo(Vector3 vdest)
  {
    if(Vector2.Distance(transform.position.get_xz(), vdest.get_xz()) > 0.1f)
      _rb.MovePosition(Vector3.Lerp(transform.position, vdest, Time.deltaTime * 10));
    else
      _rb.velocity = Vector3.zero;
  }
  public void Throw(Vector3 item_vbeg, Vector2 item_vgrid)
  {
    vwpos = item_vbeg;
    vgrid = item_vgrid;
    Vector3[] vcps = new Vector3[4];
    vcps[0] = item_vbeg;
    vcps[3] = Item.ToPos(vgrid);
    vcps[1] = Vector3.Lerp(vcps[0], vcps[3], 0.45f) + new Vector3(0, 5, 0);
    vcps[2] = Vector3.Lerp(vcps[0], vcps[3], 0.55f) + new Vector3(0, 5, 0);
    Spawn(item_vgrid, vcps, -40.0f, 2);
  }
  public void ThrowToAnimal(Vector3 animal_vpos, System.Action<Item> endAction)
  {
    _ready = false;

    Vector3[] vcps = new Vector3[4];
    vcps[0] = vwpos;
    vcps[3] = animal_vpos;
    vcps[1] = Vector3.Lerp(vcps[0], vcps[3], 0.45f) + new Vector3(0, 4, 0);
    vcps[2] = Vector3.Lerp(vcps[0], vcps[3], 0.55f) + new Vector3(0, 4, 0);
    StartCoroutine(coMovePathToAnim(vcps, endAction));
  }
  IEnumerator coMovePathToAnim(Vector3[] vcps, System.Action<Item> action)
  {
    float t = 0.0f;
    while(t <= 1)
    {
      float prev_t = t;
      t += Time.deltaTime * 2;
      float tc = Mathf.Clamp01(t);
      vwpos = Vector3Ex.bezier(vcps, tc);
      yield return null;
    }

    action?.Invoke(this);
  }

  //Vector3 _vsink = Vector3.zero;
  void Update()
  {
    if(_levelFinished)
      return;

    _lifetime += Time.deltaTime;
  }
  [SerializeField] float _buoyancy  = 1.0f;
  [SerializeField] float _damp_over = 0.0f;
  [SerializeField] float _damp_under = 0.5f;
  [SerializeField] float _sinkAmp = 0.25f;
  [SerializeField] float _sinkTime = 10f;
  float _sink = 0;
  float _sinkTimeOffs = 0;
  void FixedUpdate()
  {
     if(_levelFinished)
      return;

    if(_sinkTimeOffs == 0)
    {
      _sinkTimeOffs += Random.Range(1.0f, 60.0f);
      _sinkAmp *= Random.Range(0.9f, 1.1f);
      _sinkTime *= Random.Range(0.9f, 1.1f);
    }
    if(IsReady && !IsSelected)
    {
      _sink = Mathf.Sin((Time.time + _sinkTimeOffs) * Mathf.Deg2Rad * _sinkTime) * _sinkAmp + 0.25f;
      var depth = _sink + transform.position.y;
      Vector3 buoyantForce = new Vector3(0, _buoyancy * -depth * 50, 0);
      _rb?.AddForce(buoyantForce);
      _rb?.AddForce(-_rb.velocity * ((depth < 0)? _damp_under : _damp_over));

      var posy = Mathf.Clamp(transform.position.y, - 3f, 1.5f);
      transform.position = new Vector3(transform.position.x, posy, transform.position.z);
    }
    else if(!IsReady && vwpos.y < 0.25f)
    {
      _rb.AddForce(Vector3.up * 2);
    }
  }
  void LateUpdate()
  {

  }
}
