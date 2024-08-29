using UnityEngine;

public class Building : GridBuildable
{
    [SerializeField] private BuildingScriptableObject buildingScriptableObject;
    public BuildingScriptableObject BuildingScriptableObject => buildingScriptableObject;

    public override Vector2Int GetSize()
    {
        return new Vector2Int((int)buildingScriptableObject.buildingSize.x,
            (int)buildingScriptableObject.buildingSize.y);
    }
}