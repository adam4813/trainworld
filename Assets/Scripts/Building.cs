using UnityEngine;

public class Building : GridBuildable
{
    [SerializeField] private BuildingScriptableObject buildingScriptableObject;
    public BuildingScriptableObject BuildingScriptableObject => buildingScriptableObject;

    protected override Vector2Int GetScriptableObjectSize()
    {
        return Vector2Int.CeilToInt(buildingScriptableObject.buildingSize);
    }
}