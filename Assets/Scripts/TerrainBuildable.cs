using UnityEngine;

public class TerrainBuildable : GridBuildable
{
    [SerializeField] private TerrainScriptableObject terrainScriptableObject;
    public TerrainScriptableObject TerrainScriptableObject => terrainScriptableObject;
}