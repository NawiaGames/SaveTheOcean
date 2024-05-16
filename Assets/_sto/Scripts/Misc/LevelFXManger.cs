using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLib;
public class LevelFXManger : MonoBehaviour
{
    [SerializeField] ParticleSystem confettiEmitter = null;
    [SerializeField] ParticleSystem[] levelSummaryConfetti = new ParticleSystem[]{};

    private void OnEnable() {
        //RewardChest.onReward += ThrowConfetti;
        Level.onFinished += ThrowLevelConfetti;
    }
    private void OnDisable() {
        //RewardChest.onReward -= ThrowConfetti;
        Level.onFinished -= ThrowLevelConfetti;
    }

    void ThrowConfetti<T>(T sender) where T : MonoBehaviour {
        confettiEmitter.PlayAtPosition(sender.transform.position, sender.transform.up);
    }
    void ThrowLevelConfetti(Object sender){
        foreach (var ps in levelSummaryConfetti)
            ps.Play();
    }
}
