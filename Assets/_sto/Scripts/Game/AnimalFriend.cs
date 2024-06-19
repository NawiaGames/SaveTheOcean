using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalFriend : MonoBehaviour
{
  [SerializeField] Color _color;
  [SerializeField] float _speed = 5;

  MaterialPropertyBlock  _mpb;
  Vector3                _vcenter;
  float                  _radius;
  float                  _ang = 0;
  Vector3                _prevPos;
  void Start()
  {
    _mpb = new();
    var renderer = GetComponentInChildren<MeshRenderer>();
    renderer.GetPropertyBlock(_mpb);
    _mpb.SetColor("_BaseColor", _color);
    renderer.SetPropertyBlock(_mpb);
  }

  public void StartMove(Vector3 vcenter, float radius)
  {
    _vcenter = vcenter;
    _radius = radius;
    _ang = Random.Range(0.0f, 360.0f);
    _speed *= Random.Range(0.75f, 1.25f);
  }

  void Update()
  {

    var off = Quaternion.AngleAxis(_ang, Vector3.up) * (Vector3.forward * _radius);
    off.y = -0.75f + Mathf.Sin(_ang * 0.5f * Mathf.Deg2Rad) * 0.5f;
    transform.position = _vcenter + off;
    transform.forward = transform.position - _prevPos;
    _prevPos = transform.position;
    _ang += _speed * Time.deltaTime;
  }
}
