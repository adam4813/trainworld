using UnityEngine;

public class TerrainBuildable : GridBuildable
{
    [SerializeField] private TerrainScriptableObject terrainScriptableObject;
    public TerrainScriptableObject TerrainScriptableObject => terrainScriptableObject;

    public override Vector2Int GetSize()
    {
        return new Vector2Int((int)terrainScriptableObject.terrainSize.x, (int)terrainScriptableObject.terrainSize.y);
    }
}