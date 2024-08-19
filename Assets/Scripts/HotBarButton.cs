using UnityEngine;
using UnityEngine.UI;

public class HotBarButton : MonoBehaviour
{
    [SerializeField] private Sprite sprite;
    [SerializeField] private Image image;
    [SerializeField] private Vector2 buildingSize;
    
    public BuildingScriptableObject buildingScriptableObject;
    public TrackScriptableObject trackScriptableObject;

    public Vector2 BuildingSize =>
        (buildingScriptableObject ? buildingScriptableObject.BuildingSize : trackScriptableObject?.TrackSize) ?? new Vector2();
    public GameObject BuildingPrefab => buildingScriptableObject ? buildingScriptableObject.BuildingPrefab : trackScriptableObject?.TrackPrefab;

    private void Awake()
    {
        image.sprite = sprite;
    }
    
    public void Select()
    {
        GetComponent<Image>().color = Color.green;
    }
    
    public void Deselect()
    {
        GetComponent<Image>().color = Color.white;
    }
}