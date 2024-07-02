using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPLbl = TMPro.TextMeshProUGUI;
using GameLib.UI;

public class UIStatusBar : MonoBehaviour
{
  [Header("Refs")]
  [SerializeField] UIInfoBox _stamina;
  [SerializeField] UIInfoBox _coins;
  [SerializeField] UIInfoBox _gems;
  [SerializeField] Transform _movesContainer;
  [SerializeField] TMPLbl    _moveLbl;
  [Header("Settings")]
  [SerializeField] float _moveSpeed = 5.0f;
  [SerializeField] float _moveDelay = 0.05f;

  Camera uiCam = null;

  int _staminaDisp = 0;
  int _coinsDisp = 0;
  int _gemsDisp = 0;

  struct Move
  {
    public Vector3          vdst;
    public Vector2          vdstUI;
    public List<Vector2>    vscatters;
    public List<GameObject> objects;
    public List<float>      delays;
  }
  List<Move> moves = new List<Move>();


  void Awake()
  {
    GameState.Econo.onStaminaChanged += OnStaminaChanged;//OnStaminaChanged;
    GameState.Econo.onCoinsChanged += OnCoinsChanged; //OnCoinsChanged;
    GameState.Econo.onGemsChanged += OnGemsChanged;// OnGemsChanged;

    uiCam = GameObject.Find("uiCam").GetComponent<Camera>(); //System.Array.Find(FindObjectsOfType<Camera>(true), (cam) => cam.gameObject.tag.Equals("Untagged"));

    SetupRes();
  }
  void OnDestroy()
  {
    GameState.Econo.onStaminaChanged += OnStaminaChanged;//OnStaminaChanged;
    GameState.Econo.onCoinsChanged += OnCoinsChanged; //OnCoinsChanged;
    GameState.Econo.onGemsChanged += OnGemsChanged;// OnGemsChanged;
  }

  public void Show()
  {
    GetComponent<UIPanel>().ActivatePanel();
    SetupRes();
  }
  void SetupRes()
  {
    _staminaDisp = GameState.Econo.stamina;
    _coinsDisp = GameState.Econo.coins;
    _gemsDisp = GameState.Econo.gems;
    UpdateResDisp();
  }
  void UpdateDisp()
  {
    _staminaDisp = (int)Mathf.MoveTowards(_staminaDisp, GameState.Econo.stamina, 1);
    _coinsDisp = (int)Mathf.MoveTowards(_coinsDisp, GameState.Econo.coins, 1);
    _gemsDisp = (int)Mathf.MoveTowards(_gemsDisp, GameState.Econo.gems, 1);
    UpdateResDisp();
  }
  void UpdateResDisp()
  {
    _stamina.resValue = _staminaDisp; //GameState.Econo.stamina;
    _coins.resValue = _coinsDisp; //GameState.Econo.coins;
    _gems.resValue = _gemsDisp; //GameState.Econo.gems;
  }

  void OnStaminaChanged(int val) => SetupRes();
  void OnGemsChanged(int val) => SetupRes();
  void OnCoinsChanged(int val) => SetupRes();

  // public void MoveCollected(Item item, int amount)
  // {
  //   Move move = new Move();
  //   Vector3 vdstPos  = item.id.kind switch
  //   {
  //     Item.Kind.Stamina => _stamina.transform.position,
  //     Item.Kind.Coin => _coins.transform.position,
  //     Item.Kind.Gem => _gems.transform.position,
  //     _ => Vector2.zero
  //   };

  //   float dist = Mathf.Abs(uiCam.transform.position.z - vdstPos.z);
  //   move.vdst = uiCam.ScreenToWorldPoint(new Vector3(vdstPos.x, vdstPos.y, dist));// -Camera.main.nearClipPlane + dist + 32));

  //   move.vdstUI = vdstPos;
  //   move.objects = new List<GameObject>();
  //   move.delays = new List<float>();
  //   move.objects.AddRange(GameData.Prefabs.CreateStaticItemModels(item.id, _movesContainer, amount));
  //   for(int q = 0; q < amount; ++q)
  //   {
  //     move.objects[q].transform.position = item.vwpos + new Vector3(Random.Range(-0.125f, 0.125f), 0, Random.Range(-0.125f, 0.125f));
  //     move.vscatters.Add(item.vwpos + (Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up) * new Vector3(0, Random.Range(0.01f, 0.125f), 1.0f)).normalized * Random.Range(0.1f, 1.5f));
  //     move.delays.Add(-_moveDelay * q);
  //   }
  //   moves.Add(move);
  // }
  // void ProcessMoves()
  // {
  //   for(int q = 0; q < moves.Count;)
  //   {
  //     Move move = moves[q];
  //     for(int o = 0; o < move.objects.Count;)
  //     {
  //       move.delays[o] += Time.deltaTime;
  //       if(move.delays[o] > 0)
  //       {
  //         move.objects[o].transform.position = Vector3.Lerp(move.objects[o].transform.position, move.vdst, Time.deltaTime * _moveSpeed);
  //         if(Vector3.Distance(move.objects[o].transform.position, move.vdst) < 0.33f)
  //         {
  //           Destroy(move.objects[o].gameObject);
  //           move.objects.RemoveAt(o);
  //           move.delays.RemoveAt(o);
  //           --o;
  //           UpdateDisp();
  //         }
  //       }
  //       ++o;
  //     }
  //     if(move.objects.Count > 0)
  //       moves[q] = move;
  //     else
  //       moves.RemoveAt(q--);
  //     ++q;
  //   }
  // }
  public void MoveCollectedUI(Item item, int amount)
  {
    Move move = new Move();
    Vector3 vdstPos = item.id.kind switch
    {
      Item.Kind.Stamina => _stamina.GetComponent<RectTransform>().position,
      Item.Kind.Coin => _coins.GetComponent<RectTransform>().position,
      Item.Kind.Gem => _gems.GetComponent<RectTransform>().position,
      _ => Vector3.zero
    };

    var v = GetComponent<UIFitToSafeFrame>().GetSafeAreaAnchors(uiCam);
    move.vdstUI = (Vector2)uiCam.WorldToViewportPoint(vdstPos) + v.anchorMin;
    move.objects = new List<GameObject>();
    move.vscatters = new();
    move.delays = new List<float>();

    for(int q = 0; q < amount; ++q)
    {
      var lbl = Instantiate(_moveLbl, _movesContainer);
      lbl.GetComponent<RectTransform>().anchorMin = UIManager.GetViewportPosition(item.vwpos);
      lbl.GetComponent<RectTransform>().anchorMax = lbl.GetComponent<RectTransform>().anchorMin;
      lbl.text = UIDefaults.GetResSymbol(item.id);
      lbl.alpha = 0;
      move.objects.Add(lbl.gameObject);
      move.vscatters.Add(UIManager.GetViewportPosition(item.vwpos + (Quaternion.AngleAxis(Random.Range(0, 360), Vector3.up) * new Vector3(0, Random.Range(0,0.125f), 1.0f)).normalized * Random.Range(1.0f, 1.5f)));
      move.delays.Add(-_moveDelay * q);
    }
    moves.Add(move);
  }
  void ProcessMovesUI()
  {
    for(int q = 0; q < moves.Count;)
    {
      Move move = moves[q];
      for(int o = 0; o < move.objects.Count;)
      {
        if(move.delays[o] <= 0)
        {
          move.objects[o].GetComponent<TMPLbl>().alpha = 1;
          var rc = move.objects[o].GetComponent<RectTransform>();
          rc.anchorMin = Vector3.Lerp(rc.anchorMin, move.vscatters[o], Time.deltaTime * _moveSpeed * 0.75f);
          rc.anchorMax = rc.anchorMin;
          if(Vector2.Distance(rc.anchorMin, move.vscatters[o]) < 0.01f)
            move.delays[o] += Time.deltaTime;
        }
        //move.delays[o] += Time.deltaTime;
        if(move.delays[o] > 0)
        {
          move.objects[o].GetComponent<TMPLbl>().alpha = 1;
          var rc = move.objects[o].GetComponent<RectTransform>();
          rc.anchorMin = Vector3.Lerp(rc.anchorMin, move.vdstUI, Time.deltaTime * _moveSpeed * 1.25f);
          rc.anchorMax = rc.anchorMin;
          if(Vector3.Distance(rc.anchorMin, move.vdstUI) < 0.005f)
          {
            Destroy(move.objects[o].gameObject);
            move.objects.RemoveAt(o);
            move.delays.RemoveAt(o);
            --o;
            UpdateDisp();
          }
        }
        ++o;
      }
      if(move.objects.Count > 0)
        moves[q] = move;
      else
        moves.RemoveAt(q--);
      ++q;
    }
  }

  void Update()
  {
    ProcessMovesUI();
    _stamina.progressVal = GameState.Econo.GetStaminaRefillPerc();
  }
}
