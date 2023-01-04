using UnityEngine;
using UnityEngine.Advertisements;
 
public class UnityAdsRewarded : MonoBehaviour //, IUnityAdsLoadListener, IUnityAdsShowListener
{
	[SerializeField] string _androidAdUnitId = "Rewarded_Android";
	[SerializeField] string _iOsAdUnitId = "Rewarded_iOS";
	string _adUnitId;
  bool   _adReady = true;


	static UnityAdsRewarded _this = null;
	
	public static System.Action<string> onCompleted;

  public static bool IsReady() => _this._adReady;
	public static void Show() => _this.ShowAd();

	void Awake()
	{
		_this = this;

	#if UNITY_ANDROID
		_adUnitId = _androidAdUnitId;
	#elif UNITY_IOS
		_adUnitId = _iOsAdUnitId;
	#endif

		UnityAdsInit.onInitialized += LoadAd;
	}
	void OnDestroy()
	{
		UnityAdsInit.onInitialized -= LoadAd;
	}

	void LoadAd()
	{
	// IMPORTANT! Only load content AFTER initialization (in this example, initialization is handled in a different script).
    _adReady = false;
    Debug.Log("Loading Ad: " + _adUnitId);
    //Advertisement.Load(_adUnitId, this);
	}
  void ShowAd()
  {
    //Advertisement.Show(_adUnitId, this);
  }

	// public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState) 
	// { 
	// 	if(showCompletionState == UnityAdsShowCompletionState.COMPLETED)
	// 	{
	// 		onCompleted?.Invoke(adUnitId);
	// 	}
  //   LoadAd();
	// }
	// Implement Load Listener and Show Listener interface methods: 
	// public void OnUnityAdsAdLoaded(string adUnitId)
	// {
  //   Debug.Log("Loaded Ad: " + _adUnitId);
  //   _adReady = true;
	// }

	// public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
	// {
	// 	Debug.Log($"Error loading Ad Unit: {adUnitId} - {error.ToString()} - {message}");
  //   // Optionally execute code if the Ad Unit fails to load, such as attempting to try again.
  //   LoadAd();
	// }

	// public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
	// {
	// 	Debug.Log($"Error showing Ad Unit {adUnitId}: {error.ToString()} - {message}");
  //   // Optionally execute code if the Ad Unit fails to show, such as loading another ad.
  //   if(error != UnityAdsShowError.NO_CONNECTION)
  //     onCompleted?.Invoke(adUnitId);
  //   LoadAd();
	// }

	public void OnUnityAdsShowStart(string adUnitId) { }
	public void OnUnityAdsShowClick(string adUnitId) { }
}
