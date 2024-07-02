using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ads : MonoBehaviour
{
  static Ads _this = null;

  [SerializeField] AdsProvider _adsProvider;

  [SerializeField] Banner.Size _size;
  [SerializeField] Banner.Edge _edge;

  void Awake()
  {
    _this = this;
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
    public enum Size
    {
      Normal,
      Large,
    }
    public enum Edge
    {
      Top,
      Bottom,
    }

    public  static Action onLoaded, onShow, onHide, onDestroy;

    public  static Size   size => _this._size;
    public  static Edge   edge => _this._edge;
    public  static bool   IsOnTopEdge => edge == Edge.Top;
    public  static bool   IsOnBtmEdge => edge == Edge.Bottom;
    public  static bool   isLoaded {get; private set;} = false;

    //dp to pixel => dp * dpi / 160
    public  static int    GetSizePx(Size s) => Mathf.RoundToInt(s switch {Size.Normal => 50, Size.Large => 90, _ => 90} * (Screen.dpi/160.0f));

    public static void    Loaded(bool successed)
    {
      isLoaded = successed;
      if(successed)
      {
        onLoaded?.Invoke();
        Show();
      }
    }

    public static void Load(string placement = null, bool force = false)
    {
      if(!isLoaded || force)
        _this._adsProvider.BannerLoad(placement, force);
      else
        Show();
    }
    public static void Show()
    {
      onShow?.Invoke();
      _this._adsProvider.BannerShow();
    }
    public static void Hide(bool destroy)
    {
      onHide?.Invoke();
      _this._adsProvider.BannerHide();
      if(destroy)
        Destroy();
    }
    public static void Destroy()
    {
      onDestroy?.Invoke();
      _this._adsProvider.BannerDestroy();
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
  public virtual bool BannerIsLoaded(string placement) => true;
  public virtual void BannerShow(){}
  public virtual void BannerHide(){}
  public virtual void BannerDestroy(){}
}
