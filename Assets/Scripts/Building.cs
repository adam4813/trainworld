using UnityEngine;

public class Building : GridBuildable
{
    [SerializeField] private BuildingScriptableObject buildingScriptableObject;
    public BuildingScriptableObject BuildingScriptableObject => buildingScriptableObject;
}