using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPLbl = TMPro.TextMeshProUGUI;
using GameLib;
using GameLib.Utilities;
using Unity.VisualScripting;

public class RewardChest2 : MonoBehaviour
{
  [SerializeField] GameObject   _content;
  [SerializeField] Transform    _chestLid;
  [SerializeField] Rigidbody    _rb;

  public static System.Action<RewardChest2> onReward, onShow, onPoped, onDropped;

  public static int layerMask = 0;

  float      _lidAngle = 0;
  bool       _opened = false;
  int        _resCnt => GameState.Chest.staminaCnt + GameState.Chest.coinsCnt + GameState.Chest.gemsCnt;

  public int  level => GameState.Chest.rewardLevel;
  public bool isActive => _content.activeInHierarchy;
  public bool isOpen => isActive && _opened;
  public bool rewardClaimed {get;set;} = false;

  void Awake()
  {
    layerMask = LayerMask.GetMask(LayerMask.LayerToName(gameObject.layer));
    _content.SetActive(false);
    GetComponent<Collider>().enabled = false;
  }
  void OnDestroy()
  {

  }

  public void Show()
  {
    _lidAngle = 0;
    _content.SetActive(true);
    GetComponent<Collider>().enabled = true;
    _rb.MovePosition(transform.position);
    GameState.Chest.shown = true;
  }
  public void OpenLid()
  {
    if(_lidAngle == 0)
      StartCoroutine(coOpenLid());
  }
  IEnumerator coOpenLid()
  {
    onPoped?.Invoke(this);
    while(true)
    {
      yield return null;
      UpdateLid(90);
      if(_lidAngle >= 80)
      {
        _opened = true;
        if(_lidAngle >= 90)
          break;
      }
    }
  }
  public void Hide()
  {
    if(_opened && !rewardClaimed)
      StartCoroutine(coCloseLid());
    rewardClaimed = true;
  }
  IEnumerator coCloseLid()
  {
    yield return new WaitForSeconds(0.5f);
    while(true)
    {
      yield return null;
      UpdateLid(0);
      if(_lidAngle <= 0)
      {
        break;
      }
    }
    onPoped?.Invoke(this);
    Destroy(gameObject);
  }
  void UpdateLid(float angleTo)
  {
    _lidAngle = Mathf.Lerp(_lidAngle, angleTo, Time.deltaTime * 5);
    if(Mathf.Abs(angleTo - _lidAngle) < 1)
      _lidAngle = angleTo;
    _chestLid.localRotation = Quaternion.AngleAxis(_lidAngle, Vector3.right);
  }
  // void OnItemMerged(Item item)
  // {
  //   if(GameState.Chest.shown)
  //     GameState.Econo.rewards += 1;
  // }
  // void OnRewardChanged(float rewardPoints)
  // {
  //   var rewardProgress = GameData.Econo.GetRewardProgress(rewardPoints);
  //   if(rewardProgress.lvl > GameState.Chest.rewardLevel)
  //   {
  //     onReward?.Invoke(this);
  //     GameState.Chest.rewardLevel = rewardProgress.lvl;
  //     GameState.Chest.AddRewards();
  //   }
  // }

  [SerializeField] float _buoyancy  = 1.0f;
  [SerializeField] float _damp_over = 0.0f;
  [SerializeField] float _damp_under = 0.5f;
  [SerializeField] float _sinkAmp = 0.25f;
  [SerializeField] float _sinkTime = 10f;
  [SerializeField] float _sinkOffs = 0.25f;
  float _sink = 0;
  bool  _abovewater = false;
  void FixedUpdate()
  {
    if(!isActive)
      return;

    if(transform.position.y < -0.25f)
    {
      _rb.AddForce(Vector3.up * 10);
    }
    else
    {
      if(!_abovewater)
        onDropped?.Invoke(this);
      _abovewater = true;

      _sink = _sinkOffs;//-Mathf.Sin((Time.time + _sinkTimeOffs) * Mathf.Deg2Rad * _sinkTime) * _sinkAmp - _sinkOffs;
      var depth = _sink + transform.position.y;
      Vector3 buoyantForce = new Vector3(0, _buoyancy * -depth * 50, 0);
      _rb?.AddForce(buoyantForce);
      _rb?.AddForce(-_rb.velocity * ((depth < 0)? _damp_under : _damp_over));

      var posy = Mathf.Clamp(transform.position.y, -3f, 2f);
      transform.position = new Vector3(transform.position.x, posy, transform.position.z);
    }
  }
}
