using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ads : MonoBehaviour
{
  static Ads _this = null;

  [SerializeField] AdsProvider _adsProvider;

  void Awake()
  {
    _this = this;
    var providers = GetComponentsInChildren<AdsProvider>();
    Debug.Log("");
  }

  public static class Rewarded
  {
    public static System.Action<string, bool> onRequest;
    public static System.Action<string>       onShow, onClicked, onRewarded;
    public static System.Action               onAvailable;

    public  static bool IsAvailable() => _this?._adsProvider?.RewardedIsAvailable() ?? false;
    public  static void Show(string placement = "") //, Action<string, bool> callback)
    {
      onShow?.Invoke(placement);
      _this?._adsProvider?.RewardedShow(placement);
    }
    public static bool TryShow(string placement)//, Action<string, bool> callback)
    {
      bool avail = false;
      if(IsAvailable())
      {
        onRequest?.Invoke(placement, true);
        Show(placement);
        avail = true;
      }
      else
        onRequest?.Invoke(placement, false);

      return avail;
    }
  }
  public static class Interstitial
  {
    public  static System.Action<string, bool> onRequest;
    public  static System.Action<string> onShow, onClicked, onClosed;
    public  static System.Action         onAvailable;

    public  static bool IsAvailable() => _this?._adsProvider?.IntersIsAvailable() ?? false;
    public  static void Show(string placement)
    {
      onShow?.Invoke(placement);
      _this._adsProvider.IntersShow(placement);
    }
    public static bool TryShow(string placement)
    {
      bool avail = false;
      if(IsAvailable())
      {
        onRequest?.Invoke(placement, true);
        Show(placement);
        avail = true;
      }
      else
        onRequest?.Invoke(placement, false);

      return avail;
    }
  }
  public static class Banner
  {
    public enum Type
    {
      Normal,
      Large,
    };

    public  static Action onLoaded, onShow, onHide, onDestroy;

  //   public  static bool   isOnTopEdge => _this._bannerPos == IronSourceBannerPosition.TOP;
  //   public  static bool   isOnBtmEdge => _this._bannerPos == IronSourceBannerPosition.BOTTOM;
    public  static bool   isLoaded {get; private set;} = false;
  //   //dp to pixel => dp * dpi / 160
  //   public  static int    GetSize(Type type) => Mathf.RoundToInt(type switch {Type.Normal => 50, Type.Large => 90, _ => 90} * (Screen.dpi/160.0f));
  //   public  static int    GetSize() => GetSize(_this?._bannerType ?? Type.Large);

  //   static IronSourceBannerSize GetSizeType(Type type)  => _mapType.GetValueOrDefault(type, IronSourceBannerSize.LARGE);

  //   public static void Loaded(IronSourceAdInfo adInfo)
  //   {
  //     isLoaded = true;
  //     onLoaded?.Invoke();
  //     Show();
  //   }
    public static void Load(string placement = null, bool force = false)
    {
      if(!isLoaded || force)
      {
        // _mapType
        //string str = (string.IsNullOrEmpty(placement) || string.IsNullOrWhiteSpace(placement))? "DefaultBanner" : placement;
        //IronSource.Agent.loadBanner(GetSizeType(_this._bannerType), _this._bannerPos, str);
        _this._adsProvider.BannerLoad(placement, force);
      }
      else
        Show();
    }
    public static void Show()
    {
      onShow?.Invoke();
      _this._adsProvider.BannerShow();
      //IronSource.Agent.displayBanner();
    }
    public static void Hide(bool destroy)
    {
      onHide?.Invoke();
      //IronSource.Agent.hideBanner();
      _this._adsProvider.BannerHide();
      if(destroy)
        Destroy();
    }
    public static void Destroy()
    {
      onDestroy?.Invoke();
      _this._adsProvider.BannerDestroy();
      //IronSource.Agent.destroyBanner();
    }
  }

  static void Log(string str) => Debug.Log("<color=green>ADS:" + str + "</color>");
}

public class AdsProvider : MonoBehaviour
{
  public virtual bool IntersIsAvailable(string placement = null) => false;
  //public abstract bool IntersTryShow(string placement, Action<string, bool> callback);
  public virtual void IntersShow(string placement){}

  public virtual bool RewardedIsAvailable(string placement = null) => false;
  //public abstract bool RewardedTryShow(string placement, Action<string, bool> callback);
  public virtual void RewardedShow(string placement){}

  public virtual void BannerLoad(string placement, bool force){}
  public virtual void BannerShow(){}
  public virtual void BannerHide(){}
  public virtual void BannerDestroy(){}
}
