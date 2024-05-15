using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTile : MonoBehaviour
{
  [SerializeField] SpriteRenderer _sr;
  [SerializeField] ParticleSystem _ps;
  [SerializeField] Color _hoverColor;
  [SerializeField] Color _dirtyColor;
  [SerializeField] Color _clearColor;

  Vector2 _vgrid = Vector2.zero;

  public Vector2 vgrid{get => _vgrid; set => _vgrid = value;}

  Color _destColor;
  //bool  _hit = false;
  bool  _dirty = false;

  void Awake()
  {
    _sr.color = _clearColor;
    _destColor = _clearColor;

    Item.onSelect += OnItemSelection;
  }
  void OnDestroy()
  {
    Item.onSelect -= OnItemSelection;
  }

  public void Set(bool occupied, bool garbage = true)
  {
    _dirty = occupied;
    _destColor = (occupied)? _dirtyColor : _clearColor;
    if(occupied)
    {
      if(garbage)
      {
      //   this.Invoke(() =>
      //   {
      //     if(_dirty)
      //       _ps.Play();
      //   }, 1);
      }
      else
        _ps.Stop();
    }
    else
    {
      _ps.Stop();
    }
  }
  public void Hover(bool hov)
  {
    if(hov)
      _destColor = _hoverColor;
    else
      _destColor = (_dirty)? _dirtyColor : _clearColor;
  }
  void OnItemSelection(Item sender)
  {
    if(_dirty && !sender.IsInMachine && Vector3.Distance(sender.vgrid, vgrid) < 0.01f)
    {
      if(sender.IsSelected)
        _ps.Stop();
    }
  }

  void Update()
  {
    //if(_clear.activeInHierarchy)
      _sr.color = Color.Lerp(_sr.color, _destColor, Time.deltaTime * 5);
  }
}
