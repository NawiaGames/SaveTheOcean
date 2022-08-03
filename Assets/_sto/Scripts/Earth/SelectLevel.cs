using UnityEngine;

public class SelectLevel : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    private LevelEarth _currentLevelEarth; 
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            RaycastStart();
    }

    private void RaycastStart()
    {
        var ray = _camera.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out var raycastHit); 
        
        if(raycastHit.collider == null) return;

        var level = raycastHit.collider.GetComponentInParent<LevelEarth>();
        if(level)
           Select(level);
    }

    private void Select(LevelEarth levelEarth)
    {
        if (_currentLevelEarth == null)
        {
            _currentLevelEarth = levelEarth; 
            _currentLevelEarth.LiningGameObject.SetActive(true);
        }
        else if (levelEarth != _currentLevelEarth)
        {
            _currentLevelEarth.LiningGameObject.SetActive(false);
            _currentLevelEarth = levelEarth; 
            _currentLevelEarth.LiningGameObject.SetActive(true);
        }
        
        Debug.Log("Select the Level: " + _currentLevelEarth.IndexLevel);
    }
}
