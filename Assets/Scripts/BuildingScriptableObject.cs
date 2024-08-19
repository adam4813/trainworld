using UnityEngine;

[CreateAssetMenu(fileName = "Building", menuName = "Building/Building", order = 0)]
public class BuildingScriptableObject : ScriptableObject
{
    [SerializeField] private GameObject buildingPrefab;
    [SerializeField] private Vector2 buildingSize;
    [SerializeField] private Sprite sprite;
    [SerializeField] private int popularity;

    public GameObject BuildingPrefab => buildingPrefab;
    public Vector2 BuildingSize => buildingSize;
    public int Popularity => popularity;
}