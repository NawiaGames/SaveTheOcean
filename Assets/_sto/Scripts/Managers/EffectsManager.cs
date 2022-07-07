using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLib.Utilities;
using GameLib.UI;

public class EffectsManager : MonoBehaviour
{
  [Header("CamFX")]
    [SerializeField] ObjectShakePreset objShakePreset;
    [SerializeField] ObjectShakePreset objShakePresetLo;
    [SerializeField] ObjectShakePreset objShakePresetHi;
    [SerializeField] float offsetToCamera = .25f;
  [Header("FX Systems")]
    [SerializeField] ParticleSystem fxSparks = null;
    //[SerializeField] ParticleSystem fxItemCompleted = null;
    [SerializeField] ParticleSystem fxConfettiIngame = null;
    [SerializeField] ParticleSystem fxConfettiLevel = null;
    [SerializeField] ParticleSystem fxPaintSplat = null;
    [SerializeField] int ballFracturesEmitCnt = 1;
    [SerializeField] ParticleSystem fxBallFracturesSub = null;
    [SerializeField] int ballFracturesSubEmitCnt = 1;
    [SerializeField] ParticleSystem fxHit = null;
    //[SerializeField] int fxBombDestroyEmitCnt = 5;
    [SerializeField] ParticleSystem fxPainter = null;

  [Header("FX string")]
    [SerializeField] string strPushedOut = "{0}!";
    [SerializeField] string strBallsMatched = "{0}!";
    [SerializeField] string strItemExplo = "{0}!";


    ParticleSystem fxConfetti;
    ParticleSystem[] fxPainterSubs;

    ObjectShake cameraShakeContainer;
    UIInfoLabelManager infoLblMan,infoLblManDown;

    List<GameLib.ObjectFracture> listFractures = new List<GameLib.ObjectFracture>();

    private void Awake() 
    {
      cameraShakeContainer = Camera.main.GetComponentInParent<ObjectShake>();
      //infoLblMan = GameObject.Find("infoCanvas").GetComponent<UIInfoLabelManager>();
      //infoLblManDown = GameObject.Find("infoCanvas2").GetComponent<UIInfoLabelManager>();
      fxPainterSubs = fxPainter.GetComponentsInChildren<ParticleSystem>();
    }
    private void OnEnable() 
    {
      Level.onStart += OnLevelStart;
      Level.onDone += OnLevelDone;
      Level.onFinished += OnLevelFinished;
    }
    private void OnDisable()
    {
      Level.onStart -= OnLevelStart;
      Level.onDone -= OnLevelDone;
      Level.onFinished -= OnLevelFinished;
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
      //_lvl = lvl;
    }
    void OnItemExplo(Item sender)
    {
      //infoLblMan.ShowTextPopup(sender.transform.position, string.Format(strItemExplo, sender.Points), sender.color);
    }
    // void OnItemsHit(Item itemA, Item itemB)
    // {
    //   if(itemA && itemB)
    //     PlayFXAtPosition(fxHit, (itemA.transform.position + itemB.transform.position) * 0.5f, 30);
    // }
    // void OnItemPainted(Item sender)
    // {
    //   var psmain = fxPaintSplat.main;
    //   psmain.startColor = sender.color;
    //   PlayFXAtPosition(fxPaintSplat, sender.transform.position, 1);      
    // }
    // void OnItemsMatched(Level.Match3 match)
    // {
    //   Vector3 v = match.MidPos();
    //   v += new Vector3(Random.Range(-0.25f, 0.25f), 0, Random.Range(-0.25f, 0.25f));
    //   //PlayFXAtPosition(fxPaintSplat, v, 5);
    //   infoLblMan.ShowTextPopup(v, string.Format(strBallsMatched, match.Points), match.GetColor());
    //   cameraShakeContainer.Shake(objShakePresetLo);
    // }
    void OnItemDestroy(Item sender)
    {
      // if(!sender.IsPainter)
      // {
      //   var psmain = fxBallFractures.main;
      //   psmain.startColor = sender.color;
      //   PlayFXAtPosition(fxBallFractures, sender.transform.position, ballFracturesEmitCnt, false);
      //   psmain = fxBallFracturesSub.main;
      //   psmain.startColor = sender.color;
      //   PlayFXAtPosition(fxBallFracturesSub, sender.transform.position, ballFracturesSubEmitCnt, false);
      // }
      // else
      // {
      //   var psmain = fxPainter.main;
      //   psmain.startColor = sender.color;
      //   PlayFXAtPosition(fxPainter, fxPainterSubs, sender.transform.position, 1);
      // }
      //var emitParams = new ParticleSystem.EmitParams();
      //emitParams.position = sender.transform.position;
      //fxBallFractures.Emit(emitParams, 4);
    }
    void OnMoveLblChanged(GameObject go)
    {
      //infoLblMan.ShowTextPopupUI(go.transform.position + new Vector3(Random.Range(-1.0f, 1.0f) * 100, 0,Random.Range(-1.0f, 1.0f)*100) , "asdad");
      //infoLblManDown.ShowTextPopupUI(go.transform.position + new Vector3(Random.Range(-1.0f, 1.0f) * 25, 0, Random.Range(-1.0f, 1.0f) * 10), string.Format("+{0}", GameData.Points.moveLeft));
    }
    void OnFx00(object sender)
    {
      //PlayFXAtPosition(fxConfettiIngame, sender.transform.position);
      //PlayFXAtPosition(fxSparks, sender.transform.position);
      //PlayFXAtPosition(fxItemCompleted, sender.transform.position);
      //infoLblMan.ShowTextPopup(sender.transform.position, sender.GetSatisfactionString());
    }
    void OnFx01(object sender)
    {
      //infoLblMan.ShowTextPopup(sender.transform.position, customerWrongItemType, Color.red);
    }
    void OnFx02(object sender)
    {
      //infoLblMan.ShowTextPopup(item.vPos, GameData.Satisfy.GetSatisfyString(3), Color.green);
      //SparksFX(item.vPos);
    }
    void OnLevelCombo(int events_cnt)
    {
      //infoLblBigMan.ShowTextPopup(Vector3.zero, strGreetings.get_random(), Color.white);
    }
    void OnLevelDone(Level lvl)
    {
      if(lvl.Succeed)
        fxConfettiLevel.Play();
    }
    void OnLevelFinished(Level lvl) 
    {
      // if(lvl.Succeed)
      //   fxConfettiLevel.Play();
      cameraShakeContainer.Shake(objShakePresetHi);
      //_lvl = null;
    }
}
