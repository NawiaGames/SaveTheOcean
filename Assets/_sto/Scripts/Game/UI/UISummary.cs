using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GameLib.UI;
using TMPLbl = TMPro.TextMeshProUGUI;

public class UISummary : MonoBehaviour
{
  public static System.Action onShow, onBtnPlay;

  [SerializeField] UIPanel    winContainer;
  [SerializeField] UIPanel    navNextPanel;
  [SerializeField] GameObject navWinRestartPanel;
  [SerializeField] UIPanel    failContainer;
  [SerializeField] UIPanel    navRestartPanel;
  [SerializeField] UIPanel    rewardContainer;
  [SerializeField] UIPanel    navClaimPanel;

  [SerializeField] Slider  slider;
  [SerializeField] UITwoState[] stars;
  [SerializeField] TMPLbl  levelInfo;
  [SerializeField] TMPLbl  scores;
  [SerializeField] string  strScoresFmt = "score: {0}";

  bool  updateSlider = false;
  float destValue = 0;

  public void Show(Level level)
  {
    levelInfo.text = "Level " + (level.LevelIdx + 1);
    destValue = level.LevelIdx+1;
    scores.text = string.Format(strScoresFmt, level.Points.ToString());

    updateSlider = false;
    for(int q = 0; q < stars.Length; ++q)
      stars[q].SetState(level.Stars > q);

    onShow?.Invoke();

    // if(_uiRewards == null)
    //   _uiRewards = rewardContainer.GetComponentsInChildren<UIReward>(true);
    // showReward = (level.LevelIdx == range.end && GameData.Rewards.ToClaim(level.LevelIdx));

    winContainer.gameObject.SetActive(level.Succeed);
    navWinRestartPanel.SetActive(level.Succeed && level.Stars < 3);
    failContainer.gameObject.SetActive(!level.Succeed);
    GetComponent<UIPanel>().ActivatePanel();
    if(level.Succeed)
    {
      winContainer.ActivatePanel();
      StartCoroutine(Sequence(level));
    }
    else
    {
      failContainer.ActivatePanel();
      navRestartPanel.ActivatePanel();   
    }
  }
  void Hide()
  {
    updateSlider = false;
    GetComponent<UIPanel>().DeactivatePanel();
  }
  IEnumerator Sequence(Level level)
  {
    yield return new WaitForSeconds(1.0f);
    updateSlider = true;
    yield return new WaitForSeconds(0.25f);
    if(level.Succeed)
    {
      navNextPanel.ActivatePanel();
      yield return new WaitForSeconds(0.25f);
      if(level.Stars < 3)
        navWinRestartPanel.SetActive(true);
    }
    else
      navRestartPanel.ActivatePanel();
  }
  public void OnBtnRestart()
  {
    Hide();
    FindObjectOfType<Game>().RestartLevel();
  }
  public void OnBtnPlay()
  {
    Hide();
    FindObjectOfType<Game>().NextLevel();
    onBtnPlay?.Invoke();
  }
  void Update()
  {
    if(updateSlider)
      slider.value = Mathf.MoveTowards(slider.value, destValue, Time.deltaTime);
  }
}
