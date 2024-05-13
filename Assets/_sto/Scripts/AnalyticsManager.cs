using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if USE_BYTEBREW
using ByteBrewSDK;
#endif
#if USE_FACEBOOK
using Facebook.Unity;
#endif
public class AnalyticsManager : MonoBehaviour
{
    public enum AnalyticsEnvironment { Test, Production }
    [SerializeField] AnalyticsEnvironment activeAnalyticsEnvironment = AnalyticsEnvironment.Test;

    private void Start()
    {
      #if USE_FACEBOOK
        FB.Init();
      #endif
      #if USE_BYTEBREW
        ByteBrew.InitializeByteBrew();
      #endif
        Debug.Log("Analytics System | Initialized");
    }

    //TODO:
    // - log store entry
    // - log main nav menu navigation (section visited)
    // - log first bad merge -
    // - log fist / second log out - how menu levels completed
    // - log if user returned after notification
    // - log separate feeding events

  #if USE_BYTEBREW
    private void OnEnable()
    {
        Level.onStart += LogLevelStartData;
        Level.onFinished += LogLevelSummarySuccess;
        Game.onLevelRestart += LogLevelSummaryFail;
        //Animal.onFeed += LogAnimalFeed;
        Animal.onLevelUp += LogAnimalLevelUp;
    }
    private void OnDisable() {
        Level.onStart -= LogLevelStartData;
        Level.onFinished -= LogLevelSummarySuccess;
        Game.onLevelRestart -= LogLevelSummaryFail;
        //Animal.onFeed -= LogAnimalFeed;
        Animal.onLevelUp -= LogAnimalLevelUp;
    }
    void LogLevelStartData(Level sender) => LogEvent(ByteBrewProgressionTypes.Started, sender);
    void LogLevelSummarySuccess(Level sender) => LogEvent(ByteBrewProgressionTypes.Completed, sender);
    void LogLevelSummaryFail(Level sender) => LogEvent(ByteBrewProgressionTypes.Failed, sender);
    //void LogAnimalFeed(Animal sender) => ByteBrew.NewCustomEvent("AnimaFed", sender.type.ToString());
    void LogAnimalLevelUp(Animal sender) => ByteBrew.NewCustomEvent("AnimalLevelUp", sender.type.ToString());

    void LogEvent(ByteBrewProgressionTypes progressionType, Level sender)
    {
        ByteBrew.NewProgressionEvent(progressionType, GetEventEnvironmentName(activeAnalyticsEnvironment, sender), GetLevelID(sender));

        // if (activeAnalyticsEnvironment == AnalyticsEnvironment.Production) return;

        Debug.Log(
            "<color=cyan> Analytics Event " + _div +
            GetEventEnvironmentName(activeAnalyticsEnvironment, sender) + _div +
            GetProgressionTypeName(progressionType) + _div +
            GetLevelID(sender)
            + "</color>"
        );
    }

    #region formatters
        const string _div = " | ";
        string GetProgressionTypeName(ByteBrewProgressionTypes progressionType) => progressionType.ToString();
        string GetEventEnvironmentName(AnalyticsEnvironment environment, Level level = null)
        {
            if(environment == AnalyticsEnvironment.Test)
              return "TestEnvironment";
            return level.GetMode().ToString();
        }

        string GetLevelID(Level level) => level.GetMode() switch{
            Level.Mode.Feeding => level.visitsCnt.ToString("000"),
            Level.Mode.Clearing => level.visitsCnt.ToString("000"),
            _ => level.locationIdx.ToString("0000")
        };
    #endregion
  #endif
}
