using UnityEngine;

[CreateAssetMenu(fileName = "Terrain", menuName = "Building/Terrain", order = 1)]
public class TerrainScriptableObject : ScriptableObject
{
    public TerrainBuildable terrainBuildablePrefab;
    public Vector2 terrainSize;
    public Sprite sprite;
    public int bonus;
}