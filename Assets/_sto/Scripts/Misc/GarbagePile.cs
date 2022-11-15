using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GameLib;

public class GarbagePile : MonoBehaviour
{
    public static System.Action<Transform> onGarbageRemoved;
    [SerializeField] Vector2 pileSize = Vector2.one;
    [SerializeField] int maxPileCount = 100;
    int invisibleItemCount = 0;
    [SerializeField] GameObject elementPrefab = null;
    [SerializeField] Vector2 elementSizeRandom = Vector2.up; 
    [SerializeField] Vector2 areaClamp = new Vector2(1, 5);
    float _areaMultiplier(int itemCount) => Mathf.Clamp((Mathf.RoundToInt(itemCount/100f) + 1), areaClamp.x, areaClamp.y);
    [SerializeField] ParticleSystem trashPopFX = null;
    int  _garbageType = 0;
    
    Queue<GameObject> _pileQueue = new Queue<GameObject>();
    
    Vector3 GetRandomPointInArea(float radius, float height) => new Vector3(Random.Range(-radius, radius), Random.Range(0f, height), Random.Range(-radius, radius));

    private void OnEnable() {
        //Level.onStart += GeneratePile;
        Level.onUnderwaterSpawn += ItemSpawn;
        //Item.onPut += ItemPut;
    }
    private void OnDisable() {
        //Level.onStart -= GeneratePile;
        Level.onUnderwaterSpawn -= ItemSpawn;
        //Item.onPut -= ItemPut;
    }
    public void Init(int garbageType, int garbageCnt)
    {
      _garbageType = garbageType;
      GeneratePile(garbageCnt);
    }
    void ItemSpawn(Item item)
    {
      if(!item.id.IsSpecial && item.id.type == _garbageType)
      {
        PopTrash();
      }
    }
    void PopTrash(object sender) => PopTrash();

    void GeneratePile(int quantity){
        _pileQueue = new Queue<GameObject>();
        var _pileRadius = _areaMultiplier(quantity) * pileSize.x;

        for (int i = 0; i < Mathf.Clamp(quantity,0 ,maxPileCount); i++){
            var spawnOffset = GetRandomPointInArea(_pileRadius, pileSize.y);

            var newSpawn = Instantiate(
                elementPrefab, 
                transform.position + spawnOffset,
                Quaternion.LookRotation(Random.onUnitSphere),
                this.transform);
                newSpawn.transform.localScale = Vector3.one * Random.Range(elementSizeRandom.x, elementSizeRandom.y);
            _pileQueue.Enqueue(newSpawn);
        }
        invisibleItemCount = quantity < maxPileCount ?  0  : quantity - maxPileCount;
    }

    public void PopTrash(){
        if (invisibleItemCount-- > 0) {
            trashPopFX?.PlayAtPosition(_pileQueue.ToArray().OrderBy(x => System.Guid.NewGuid()).First().transform.position);
            // trashPopFX?.PlayAtPosition(transform.position + GetRandomPointInArea(pileSize.x * _pileRadius, pileSize.y), Vector3.up);
            return;
        }

        if (!_pileQueue.TryDequeue(out GameObject lastObject)) return;

        lastObject.gameObject.SetActive(false);
        onGarbageRemoved?.Invoke(lastObject.transform);
        trashPopFX?.PlayAtPosition(lastObject.transform.position, Vector3.up);
    }
}
