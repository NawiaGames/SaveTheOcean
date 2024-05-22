using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevOptions : MonoBehaviour
{
  public void on_max_dbg()
  {
  #if HAS_LION_GAME_ANALYTICS_SDK
    LionStudios.Suite.Debugging.LionDebugger.Show();
  #endif
  }

  // public void on_btn_finish_level()
  // {
  //   FindObjectOfType<Game>(true).FinishLevel();
  // }
  public void on_btn_00()
  {
    GameState.Econo.stamina += 5;
  }
  public void on_btn_01()
  {
    GameState.Econo.coins += 5;
  }
  public void btn_btn_02()
  {
    GameState.Econo.gems += 5;
  }
  public void on_btn_reset_progress()
  {
    GameLib.DataSystem.DataManager.ResetProgress();
  }
  public void on_btn_cycle_theme()
  {
    FindFirstObjectByType<GameLib.UI.UIColorThemeManager>().ApplyThemeColorToAllSubComponents(colorThemes[(int)Mathf.Repeat(++colorTheme, colorThemes.Length)]);
  }
  [SerializeField] GameLib.UI.UIColorTheme[] colorThemes = new GameLib.UI.UIColorTheme[]{};
  [SerializeField] int colorTheme = 0;

  public void IncreaseQualityLevel() => QualitySettings.IncreaseLevel();
  public void DecreaseQualityLevel() => QualitySettings.DecreaseLevel();
}
