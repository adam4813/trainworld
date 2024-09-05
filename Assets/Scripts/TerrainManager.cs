using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TerrainManager : MonoBehaviour, ISaveable
{
    [SerializeField] private List<TerrainBuildable> terrainBuildables;
    [SerializeField] private TableGrid tableGrid;

    private void OnEnable()
    {
        GridPlacementSystem.OnBuildingPlaced += OnBuildingPlaced;
        GridPlacementSystem.OnBuildingRemoved += OnBuildingRemoved;
    }

    private void OnDisable()
    {
        GridPlacementSystem.OnBuildingPlaced -= OnBuildingPlaced;
        GridPlacementSystem.OnBuildingRemoved -= OnBuildingRemoved;
    }

    private void OnBuildingPlaced(GridBuildable buildable)
    {
        if (!buildable.TryGetComponent<TerrainBuildable>(out var terrain)) return;

        terrain.UpdateRect(terrain.GetSize());

        terrainBuildables.Add(terrain);
    }

    private void OnBuildingRemoved(GridBuildable buildable)
    {
        if (buildable.TryGetComponent<TerrainBuildable>(out var building))
        {
            terrainBuildables.Remove(building);
        }
    }

    public void PopulateSaveData(SaveData saveData)
    {
        foreach (var terrainData in terrainBuildables.Select(terrain => new GridSaveData.GridCellSaveData()
                 {
                     pivotPoint = terrain.GetPivotPoint(),
                     size = terrain.GetSize(),
                     terrainScriptableObject = terrain.TerrainScriptableObject,
                     yRotation = terrain.transform.eulerAngles.y
                 }))
        {
            saveData.GridSave.gridCells.Add(terrainData);
        }
    }

    public void LoadFromSaveData(SaveData saveData)
    {
    }
}