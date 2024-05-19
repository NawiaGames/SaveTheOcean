using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPLbl = TMPro.TextMeshPro;
using GameLib;
using GameLib.Utilities;

public class GarbageInfo : MonoBehaviour
{
  [SerializeField] GameObject content;
  [SerializeField] GameObject requestContainer;
  [SerializeField] RectTransform _popupRect;
  [SerializeField] ActivatableObject _actObj;
  [SerializeField] Item _bag;

  [SerializeField] float _itemSpacing = 1.25f;
  [SerializeField] float _itemScale = 1.0f;

  public static int layer { get; private set; } = 0;
  public static int layerMask { get; private set; } = 0;

  public static System.Action<GarbageInfo> onGoodItem, onWrongItem;
  public static System.Action<GarbageInfo> onShow, onHide;

  public Transform itemContainer => requestContainer.transform;

  //List<Item> _bags = new();
  int        _bagsCnt = 0;

  void Awake()
  {
    _bag = GameData.Prefabs.CreateBagStaticItem(new Item.ID(), requestContainer.transform);
  }
  public void Show(List<Item.ID> requests)
  {
    //_bags.AddRange(requests);
    //_bags.ForEach((request) => request.transform.parent = requestContainer.transform);
    _bagsCnt = requests.Count;
    UpdateLayout();
    _actObj.ActivateObject();
    onShow?.Invoke(this);
  }
  public void Remove(Item.ID id)
  {
    // Item item = _bags.Find((req) => Item.ID.Eq(req.id, id));
    // if(item)
    // {
    //   _bags.Remove(item);
    //   item.gameObject.SetActive(false);
    // }
    _bagsCnt--;
    UpdateLayout();
  }
  void UpdateLayout()
  {
    // for(int q = 0; q < _bags.Count; ++q)
    // {
    //   float x = (-_bags.Count + 1) * 0.5f + q;
    //   _bags[q].transform.localPosition = new Vector3(x * _itemSpacing, 0, 0);
    // }
    // _popupRect.sizeDelta = new Vector2(88 + (_bags.Count * 80 * _itemSpacing), _popupRect.sizeDelta.y);
    _popupRect.sizeDelta = new Vector2(88 + 80 * _itemSpacing, _popupRect.sizeDelta.y);
    //_bags.ForEach((Item item) => item.transform.localScale = Vector3.one * _itemScale);
  }
  public void Hide()
  {
    _actObj.DeactivateObject();
    onHide?.Invoke(this);
  }
}
