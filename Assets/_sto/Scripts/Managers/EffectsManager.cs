using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameLib.Utilities;
using GameLib.UI;

public class EffectsManager : MonoBehaviour
{
  Camera mainCamera = null;
  ObjectShake cameraShakeContainer;
  UIInfoLabelManager infoLblMan,infoLblManDown;

  [Header("CamFX")]
    [SerializeField] ObjectShakePreset objShakePreset;
    [SerializeField] ObjectShakePreset objShakePresetLo;
    [SerializeField] ObjectShakePreset objShakePresetHi;
    [SerializeField] float offsetToCamera = .25f;
  [Header("FX Systems")]
    [SerializeField] ParticleSystem fxWaterSplash = null;
    [SerializeField] ParticleSystem fxHit = null;
    [SerializeField] ParticleSystem fxMagnet = null;

    [Header("FX string")]
    [SerializeField] string _strNoMergeMaxed;
    [SerializeField] string _strNoMergeWrongType;
    [SerializeField] string _strAnimalWrongItem;
    [SerializeField] string _strNoCapacity;
    [SerializeField] string _strNoSplittableItem;
    [SerializeField] string _strNoRoomOnGrid = "no room on grid";
    [SerializeField] string _strEmpty = "empty";
    [SerializeField] string _strFeedingNoRes = "no coins";
    [SerializeField] string _strNotPushedGarbage = "cannot store garbages";
    [SerializeField] string _strNotPushedFood = "cannot store food";
    [SerializeField] string _strCollected = "+{0}";
    [SerializeField] string _strAnimalFeed = "+{0}";
    [SerializeField] string _strAnimalLevelUp = "Level Up!";

    List<GameLib.ObjectFracture> listFractures = new List<GameLib.ObjectFracture>();

    private void Awake() 
    {
      mainCamera = Camera.main;
      cameraShakeContainer = Camera.main.GetComponentInParent<ObjectShake>();
      infoLblMan = FindObjectOfType<UIInfoLabelManager>(true);
      //infoLblManDown = GameObject.Find("infoCanvas2").GetComponent<UIInfoLabelManager>();
    }
    private void OnEnable() 
    {
      Level.onStart += OnLevelStart;
      //Level.onDone += OnLevelDone;
      Level.onFinished += OnLevelFinished;
      Level.onNoRoomOnGrid += OnLevelNoGridRoom;
      Level.onMagnetBeg += OnMagnetBeg;
      Level.onMagnetEnd += OnMagnetEnd;
      Level.onItemCollected += OnItemCollected;

      Item.onShown += OnItemShown;
      Item.onHide += OnItemHide;
      Item.onMerged += OnItemMerged;
      Item.onNoMerged += OnItemNoMerged;
      Item.onPut += OnItemPut;
      Item.onNoPut += OnItemNoPut;
      Item.onDropped += OnItemDropped;

      Animal.onFeed += OnAnimalFeed;
      Animal.onLevelUp += OnAnimalLevelUp;

      SplitMachine.onSplitted += OnSplitMachineSplitted;
      SplitMachine.onDropped += OnSplitMachineDrop;

      RewardChest.onPoped += OnItemPoped;
      RewardChest.onNotPoped += OnItemNotPoped;
      StorageBox.onPoped += OnItemPoped;
      StorageBox.onNotPoped += OnItemNotPoped;
      StorageBox.onPushed += OnItemPushed;
      StorageBox.onNotPushed += OnItemNotPushed;
      FeedingMachine.onPoped += OnItemPoped;
      FeedingMachine.onNotPoped += OnItemNotPoped;      
    }
    private void OnDisable()
    {
      Level.onStart -= OnLevelStart;
      //Level.onDone -= OnLevelDone;
      Level.onFinished -= OnLevelFinished;
      Level.onNoRoomOnGrid -= OnLevelNoGridRoom;
      Level.onMagnetBeg -= OnMagnetBeg;
      Level.onMagnetEnd -= OnMagnetEnd;
      Level.onItemCollected -= OnItemCollected;

      Item.onShown -= OnItemShown;
      Item.onHide -= OnItemHide;
      Item.onMerged -= OnItemMerged;
      Item.onNoMerged -= OnItemNoMerged;
      Item.onPut -= OnItemPut;
      Item.onNoPut -= OnItemNoPut;
      Item.onDropped -= OnItemDropped;

      Animal.onFeed -= OnAnimalFeed;
      Animal.onLevelUp -= OnAnimalLevelUp;

      SplitMachine.onSplitted -= OnSplitMachineSplitted;
      SplitMachine.onDropped -= OnSplitMachineDrop;

      RewardChest.onPoped -= OnItemPoped;
      RewardChest.onNotPoped -= OnItemNotPoped;
      StorageBox.onPoped -= OnItemPoped;
      StorageBox.onNotPoped -= OnItemNotPoped;
      StorageBox.onPushed -= OnItemPushed;
      StorageBox.onNotPushed -= OnItemNotPushed;
      FeedingMachine.onPoped -= OnItemPoped;
      FeedingMachine.onNotPoped -= OnItemNotPoped;
    }

    Vector3 GetFxPosition(Vector3 objectPosition) => objectPosition + (objectPosition - Camera.main.transform.position).normalized * -offsetToCamera;
    void PlayFXAtPosition(ParticleSystem ps, Vector3 worldPosition, int emitCount = 0, bool useCameraOffset = true)
    {
      ps.transform.position = useCameraOffset ? GetFxPosition(worldPosition) : worldPosition;
      if(emitCount > 0)
        ps.Emit(emitCount);
      else
        ps.Play(true);
    }
    void PlayFXAtPosition(ParticleSystem ps, ParticleSystem[] subs, Vector3 worldPosition, int emitCount = 0, bool useCameraOffset = true)
    {
      ps.transform.position = useCameraOffset ? GetFxPosition(worldPosition) : worldPosition;
      if(emitCount > 0)
      {
        ps.Emit(emitCount);
        foreach(var sub in subs)
        {
          sub.transform.position = ps.transform.position;
          sub.Emit(emitCount);
        }
      }
      else
        ps.Play(true);
    }    
    
    void OnLevelStart(Level lvl)
    {

    }

    void OnItemMerged(Item sender)
    {
      //PlayFXAtPosition(fxHit, sender.transform.position);
      PlayFXAtPosition(fxWaterSplash, sender.transform.position, 0, false);
    }
    void OnItemNoMerged(Item sender)
    {
      if(sender.mergeType == Item.MergeType.RejectMaxed)
        infoLblMan.ShowTextPopup(sender.vwpos, _strNoMergeMaxed);
      else if(sender.mergeType == Item.MergeType.RejectWrongType)
        infoLblMan.ShowTextPopup(sender.vwpos, _strNoMergeWrongType);
    }    
    void OnItemShown(Item sender)
    {
      PlayFXAtPosition(fxWaterSplash, sender.gridPos, 0, false);
    }
    void OnItemHide(Item sender)
    {

    }
    void OnItemPut(Item sender)
    {
      PlayFXAtPosition(fxHit, sender.transform.position, 0, false);
      infoLblMan.ShowTextPopup(sender.transform.position + new Vector3(0, 1.0f, 0), UIDefaults.GetStaminaString(-GameData.Econo.staminaPushoutItemCost));
    }
    void OnItemNoPut(Item sender)
    {
      infoLblMan.ShowTextPopup(sender.vwpos, _strAnimalWrongItem);
    }
    void OnItemDropped(Item sender)
    {
      if(!sender.IsInMachine)
        PlayFXAtPosition(fxWaterSplash, sender.gridPos, 0, false);
      else
        PlayFXAtPosition(fxWaterSplash, sender.vwpos, 0, false);
    }
    void OnSplitMachineDrop(SplitMachine sm)
    {
      if(sm.dropResult == SplitMachine.DropResult.NoSplittableItem)
        infoLblMan.ShowTextPopup(sm.dropPosition, _strNoSplittableItem);
      else if(sm.dropResult == SplitMachine.DropResult.NoCapacity)
        infoLblMan.ShowTextPopup(sm.dropPosition, _strNoCapacity);

    }
    void OnSplitMachineSplitted(SplitMachine sm)
    {
      //else if(sm.dropResult == SplitMachine.DropResult.Ok)
      PlayFXAtPosition(fxHit, sm.dropPosition, 50, false);
      PlayFXAtPosition(fxHit, sm.GetSplitSlotPos(0), 50, false);
      PlayFXAtPosition(fxHit, sm.GetSplitSlotPos(1), 50, false);
    }
    void OnItemPoped(MonoBehaviour sender)
    {
      PlayFXAtPosition(fxHit, sender.transform.position + new Vector3(0, 1.0f, 0), 50, false);
    }
    void OnItemNotPoped(MonoBehaviour sender)
    {
      if(sender is FeedingMachine)
        infoLblMan.ShowTextPopup(sender.transform.position + new Vector3(0, 1.0f, 0), _strFeedingNoRes);
      else
        infoLblMan.ShowTextPopup(sender.transform.position + new Vector3(0, 1.0f, 0), _strEmpty);
    }
    void OnItemPushed(MonoBehaviour sender)
    {
      PlayFXAtPosition(fxHit, sender.transform.position + new Vector3(0, 1.0f, 0), 50, false);
    }
    void OnItemNotPushed(MonoBehaviour sender)
    {
      string _str = "";
      if(sender is StorageBox)
      {
        var ps = (sender as StorageBox)?.pushState;
        if(ps == StorageBox.PushState.Garbage)
          _str = _strNotPushedGarbage.Replace('|', '\n');
        else if(ps == StorageBox.PushState.Food)
          _str = _strNotPushedFood.Replace('|', '\n');
      }
      infoLblMan.ShowTextPopup(sender.transform.position + new Vector3(-1, 1.0f, 0), _str);
    }
    void OnItemCollected(Item item)
    {
      string str = "";
      int amount = GameData.Econo.GetResCount(item.id);
      if(item.id.kind == Item.Kind.Stamina)
        str = UIDefaults.GetStaminaString(amount);
      else if(item.id.kind == Item.Kind.Coin)
        str = UIDefaults.GetCoinsString(amount);  
      else if(item.id.kind == Item.Kind.Gem)
        str = UIDefaults.GetGemsString(amount);
      var s = string.Format(_strCollected, str);
      infoLblMan.ShowTextPopup(item.transform.position + new Vector3(0, 1.0f, 0), s);
  }
    void OnLevelNoGridRoom(Level sender)
    {
      infoLblMan.ShowTextPopup(Vector3.zero, _strNoRoomOnGrid);
    }

    void OnItemDestroy(Item sender)
    {

    }
    void OnAnimalFeed(Animal sender)
    {
      infoLblMan.ShowTextPopup(sender.transform.position + new Vector3(0, 0, 0), string.Format(_strAnimalFeed, (int)sender.lastkCal));
    }
    void OnAnimalLevelUp(Animal sender)
    {
      infoLblMan.ShowTextPopup(sender.transform.position + new Vector3(0, 2, 0), _strAnimalLevelUp);
    }
    void OnMagnetBeg(Vector3 vpos)
    {
      fxMagnet.transform.position = vpos;
      if(fxMagnet.isStopped)
        PlayFXAtPosition(fxMagnet, vpos, 0, false);
    }
    void OnMagnetEnd(bool clear)
    {
      fxMagnet.Stop();
      if(clear)
        fxMagnet.Clear();
    }
    void OnLevelFinished(Level lvl) 
    {
      //cameraShakeContainer.Shake(objShakePresetHi);
    }
}
