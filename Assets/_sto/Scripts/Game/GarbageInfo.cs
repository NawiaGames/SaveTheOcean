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

  [SerializeField] float _itemSpacing = 1.25f;
  [SerializeField] float _itemScale = 1.0f;

  public static int layer { get; private set; } = 0;
  public static int layerMask { get; private set; } = 0;

  public static System.Action<GarbageInfo> onGoodItem, onWrongItem;
  public static System.Action<GarbageInfo> onShow, onHide;

  public Transform itemContainer => requestContainer.transform;
  List<Item> _requestedItems = new List<Item>();
  
  void Awake()
  {
  }
  public void Show(List<Item> requests)
  {
    _requestedItems.AddRange(requests);
    _requestedItems.ForEach((request) => request.transform.parent = requestContainer.transform);
    UpdateLayout();
    _actObj.ActivateObject();
    onShow?.Invoke(this);
  }
  public void Remove(Item.ID id)
  {
    Item item = _requestedItems.Find((req) => Item.ID.Eq(req.id, id));
    if(item)
    {
      _requestedItems.Remove(item);
      item.gameObject.SetActive(false);
    }
    UpdateLayout();
  }
  void UpdateLayout()
  {
    for(int q = 0; q < _requestedItems.Count; ++q)
    {
      float x = (-_requestedItems.Count + 1) * 0.5f + q;
      _requestedItems[q].transform.localPosition = new Vector3(x * _itemSpacing, 0, 0);
    }
    _popupRect.sizeDelta = new Vector2(88 + (_requestedItems.Count * 80 * _itemSpacing), _popupRect.sizeDelta.y);

    _requestedItems.ForEach((Item item) => item.transform.localScale = Vector3.one * _itemScale);
  }
  public void Hide()
  {
    _actObj.DeactivateObject();
    onHide?.Invoke(this);
  }
}
