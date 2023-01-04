using UnityEngine;
using UnityEngine.Advertisements;
 
public class UnityAdsInit : MonoBehaviour //, IUnityAdsInitializationListener
{
	[SerializeField] string _androidGameId;
	[SerializeField] string _iOSGameId;
	private string 					_gameId;

	public static System.Action onInitialized;

	void Awake()
	{
		InitializeAds();
	}

	public void InitializeAds()
	{
	#if UNITY_ANDROID
		_gameId = _androidGameId;
#elif UNITY_IOS
		_gameId = _iOSGameId
#endif

    //Advertisement.Initialize(_gameId, Debug.isDebugBuild, this);
		//Advertisement.Initialize(_gameId, false, this);
	}

	// public void OnInitializationComplete()
	// {
	// 	Debug.Log("Unity Ads initialization complete.");
	// 	onInitialized?.Invoke();
	// }

	// public void OnInitializationFailed(UnityAdsInitializationError error, string message)
	// {
	// 	Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
	// }
}
