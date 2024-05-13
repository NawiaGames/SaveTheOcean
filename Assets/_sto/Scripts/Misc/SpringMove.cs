#if false
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringMove : MonoBehaviour
{
  [SerializeField] GameObject _obj;
  [SerializeField] float _stiffness = 0.25f;
  [SerializeField] float _damping = 0.95f;
  [SerializeField] Vector3 _vminLimit = new Vector3(-1, -1, -1);
  [SerializeField] Vector3 _vmaxLimit = new Vector3(1, 1, 1);
  [SerializeField] float  _velLimitFactor = 1.0f;
  [SerializeField] float  _speedFactor = 1.0f;

  Vector3 _vbase = Vector3.zero;
  Vector3 _vvel = Vector3.zero;
  Vector3 _vforce = Vector3.zero;
  Vector3 _vpos = Vector3.zero;
  Vector3 _voffs = Vector3.zero;

  void Awake()
  {
    if(_obj == null)
      _obj = gameObject;

    _vbase = _obj.transform.localPosition;
  }

  public void Touch(float vvely = -0.25f)
  {
    _vvel += new Vector3(0, vvely, 0);
    _vvel = _vvel.clamp(_vminLimit * _velLimitFactor, _vmaxLimit * _velLimitFactor);
  }
  public bool IsIdle => _vvel.sqrMagnitude < 0.0001f * 0.0001f;

  void Update()
  {
    if(_vvel.sqrMagnitude > 0 || _vforce.sqrMagnitude > 0)
    {
      var _voff = _vpos;
      _vforce = -_stiffness * _voff;
      _vvel += _vforce;
      _vpos += _vvel * Time.deltaTime * _speedFactor;
      _vpos = _vpos.clamp(_vminLimit, _vmaxLimit);
      _vvel *=_damping;
      if(_vvel.magnitude < 0.0001f)
      {
        _vvel.Set(0, 0, 0);
        _vforce.Set(0, 0, 0);
        _vpos.y = 0;
      }
      _obj.transform.localPosition = _vbase + _vpos;
    }
  }
}
#endif