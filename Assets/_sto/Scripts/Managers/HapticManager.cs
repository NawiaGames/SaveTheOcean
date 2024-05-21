using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLib.Haptics;

public class HapticManager : MonoBehaviour
{
  private void Awake() {
    if (GameLib.Haptics.HapticsSystem.Instance == null) this.enabled = false;
  }
  void OnEnable()
  {
    Level.onStart += VibMed;
    Level.onFinished += VibMed;
    Level.onItemHovered += OnItemsHovered;
    Level.onAnimalHovered += OnAnimalHovered;
    Level.onShipHovered += OnShipHovered;
    Item.onMerged += VibMed;
    Item.onNoMerged += VibMed;
    Item.onPut += VibMed;
    Item.onNoPut += VibMed;
    Animal.onFeed += VibMed;

    //RewardChest.onPoped += VibMed;
    //RewardChest.onNotPoped += VibMed;
    //RewardChest.onNotPushed += VibMed;
    StorageBox.onPoped += VibMed;
    StorageBox.onNotPoped += VibMed;
    StorageBox.onPushed += VibMed;
    StorageBox.onNotPushed += VibMed;
    FeedingMachine.onPoped += VibMed;
    FeedingMachine.onNotPoped += VibMed;
  }
  void OnDisable()
  {
    Level.onStart -= VibMed;
    Level.onFinished -= VibMed;
    Level.onItemHovered -= OnItemsHovered;
    Level.onAnimalHovered -= OnAnimalHovered;
    Level.onShipHovered -= OnShipHovered;
    Item.onMerged -= VibMed;
    Item.onNoMerged -= VibMed;
    Item.onPut -= VibMed;
    Item.onNoPut -= VibMed;
    Animal.onFeed -= VibMed;

    //RewardChest.onPoped -= VibMed;
    //RewardChest.onNotPoped -= VibMed;
    //RewardChest.onNotPushed -= VibMed;
    StorageBox.onPoped -= VibMed;
    StorageBox.onNotPoped -= VibMed;
    StorageBox.onPushed -= VibMed;
    StorageBox.onNotPushed -= VibMed;
    FeedingMachine.onPoped -= VibMed;
    FeedingMachine.onNotPoped -= VibMed;
  }

  void OnItemsHovered(Level lvl)
  {
    if(lvl.hoverItemMatch)
      VibMed(null);
    else
      VibLo(null);
  }
  void OnAnimalHovered(Level lvl)
  {
    if(lvl.hoverItemMatch)
      VibMed(null);
    else
      VibLo(null);
  }
  void OnShipHovered(Level lvl)
  {
    if(lvl.hoverItemMatch)
      VibMed(null);
    else
      VibLo(null);
  }

  void VibLo0() => VibLo(null);
  void VibMed0() => VibMed(null);
  void VibHi0() => VibHi(null);
  void VibLo2(object obj0, object obj1) => VibLo(null);
  void VibMed2(object obj0, object obj1) => VibMed(null);
  void VibHi2(object obj0, object obj1) => VibHi(null);


  void VibLo(object sender)
  {
    HapticsSystem.Vibrate(HapticsSystem.HapticFeedback.Tick);
  }
  void VibMed(object sender)
  {
    HapticsSystem.Vibrate(HapticsSystem.HapticFeedback.Medium);
  }
  void VibHi(object sender)
  {
    HapticsSystem.Vibrate(HapticsSystem.HapticFeedback.High);
  }
}
