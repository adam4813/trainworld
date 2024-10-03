using UnityEngine;

public class TerrainBuildable : GridBuildable
{
    [SerializeField] private TerrainScriptableObject terrainScriptableObject;
    public TerrainScriptableObject TerrainScriptableObject => terrainScriptableObject;

    protected override Vector2Int GetScriptableObjectSize()
    {
        return Vector2Int.CeilToInt(terrainScriptableObject.terrainSize);
    }
}