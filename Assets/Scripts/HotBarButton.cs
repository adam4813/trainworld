using UnityEngine;
using UnityEngine.UI;

public class HotBarButton : MonoBehaviour
{
    [SerializeField] private Sprite sprite;
    [SerializeField] private Image image;
    [SerializeField] private Vector2 buildingSize;
    
    public BuildingScriptableObject buildingScriptableObject;
    public TrackScriptableObject trackScriptableObject;

    private bool isSelected;
    public Vector2 BuildingSize =>
        (buildingScriptableObject ? buildingScriptableObject.BuildingSize : trackScriptableObject?.TrackSize) ?? new Vector2();
    public GameObject BuildingPrefab => buildingScriptableObject ? buildingScriptableObject.BuildingPrefab : trackScriptableObject?.TrackPrefab;

    private void Awake()
    {
        image.sprite = sprite;
    }
    
    public void Select()
    {
        isSelected = true;
        GetComponent<Image>().color = Color.green;
    }
    
    public void Deselect()
    {
        isSelected = false;
        GetComponent<Image>().color = Color.white;
    }
}