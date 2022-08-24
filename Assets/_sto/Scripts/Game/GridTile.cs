using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTile : MonoBehaviour
{
  [SerializeField] GameObject _dirty;
  [SerializeField] GameObject _clear;
  [SerializeField] ParticleSystem _ps;
  [SerializeField] Color _hoverColor;

  SpriteRenderer _sp;

  Vector2 _vgrid = Vector2.zero;

  public Vector2 vgrid{get => _vgrid; set => _vgrid = value;}

  Color _baseClearColor;
  Color _destClearColor;
  bool  _hit = false;

  void Awake()
  {
    _sp = _clear.GetComponent<SpriteRenderer>();
    _baseClearColor = _clear.GetComponent<SpriteRenderer>().color;
    _destClearColor = _baseClearColor;
  }

  public void Set(bool act)
  {
    _dirty.SetActive(act);
    _clear.SetActive(!act);
    if(act)
      this.Invoke(() => this._ps.Play(), 1);
    else
    {
      _destClearColor = _baseClearColor;
      _ps.Stop();
    }
  }
  public void Hover(bool hov)
  {
    _destClearColor = (hov)? _hoverColor : _baseClearColor;
  }

  void Update()
  {
    if(_clear.activeInHierarchy)
      _sp.color = Color.Lerp(_sp.color, _destClearColor, Time.deltaTime * 5);
  }
}
