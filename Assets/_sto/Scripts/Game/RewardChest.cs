using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPLbl = TMPro.TextMeshProUGUI;

public class RewardChest : MonoBehaviour
{
  [SerializeField] Slider _slider;
  [SerializeField] TMPLbl _lblResCnt;
  [SerializeField] GameObject _infoContainer;
  [SerializeField] Transform _chestLid;

  float _rewardPointsMov = 0;
  float _lidAngle = 0;
  public static int layerMask = 0;

  void Awake()
  {
    GameState.Econo.onRewardProgressChanged += OnRewardChanged;
    _rewardPointsMov = GameState.Econo.rewards;
    OnRewardChanged(_rewardPointsMov);

    layerMask = LayerMask.GetMask(LayerMask.LayerToName(gameObject.layer));

    _lidAngle = (_resCnt == 0) ? 0 : 90;
  }
  void OnDestroy()
  {
    GameState.Econo.onRewardProgressChanged -= OnRewardChanged;
  }

  int _resCnt => GameState.Econo.Chest.staminaCnt + GameState.Econo.Chest.coinsCnt + GameState.Econo.Chest.gemsCnt;

  void SetupSlider()
  {
    UpdateSlider();
  }
  void UpdateInfo()
  {
    _lblResCnt.text = $"{_resCnt}";
    _infoContainer.gameObject.SetActive(_resCnt > 0);
  }
  void UpdateSlider()
  {
    var rewardProgress = GameData.Econo.GetRewardProgress(_rewardPointsMov);
    _slider.value = rewardProgress.progress_points;
    _slider.minValue = rewardProgress.progress_range_lo;
    _slider.maxValue = rewardProgress.progress_range_hi;
  }
  void UpdateLid()
  {
    float angleTo = (_resCnt == 0) ? 0 : 90;
    _lidAngle = Mathf.Lerp(_lidAngle, angleTo, Time.deltaTime * 5);
    _chestLid.localRotation = Quaternion.AngleAxis(_lidAngle, Vector3.right);
  }  
  void OnRewardChanged(float rewardPoints)
  {
    UpdateSlider();
    UpdateInfo();
    
    var rewardProgress = GameData.Econo.GetRewardProgress(rewardPoints);
    if(rewardProgress.lvl > GameState.Econo.Chest.rewardLevel)
    {
      this.Invoke(()=>
      {
        GameState.Econo.Chest.rewardLevel = rewardProgress.lvl;
        GameState.Econo.Chest.AddRewards();
        UpdateInfo();
      }, 0.25f);
    }
  }

  public Item.ID? Pop()
  {
    var id = GameState.Econo.Chest.PopRes();
    UpdateInfo();
    return id;  
  }
  void Update()
  {
    _rewardPointsMov = Mathf.Lerp(_rewardPointsMov, GameState.Econo.rewards, Time.deltaTime * 4);
    UpdateSlider();
    UpdateLid();
  }
}
