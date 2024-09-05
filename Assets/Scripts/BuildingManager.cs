using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuildingManager : MonoBehaviour, ISaveable
{
    [SerializeField] private List<Building> buildings;
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
        if (!buildable.TryGetComponent<Building>(out var building)) return;

        building.UpdateRect(building.GetSize());

        buildings.Add(building);
    }

    private void OnBuildingRemoved(GridBuildable buildable)
    {
        if (buildable.TryGetComponent<Building>(out var building))
        {
            buildings.Remove(building);
        }
    }

    public void PopulateSaveData(SaveData saveData)
    {
        foreach (var buildingData in buildings.Select(building => new GridSaveData.GridCellSaveData()
                 {
                     pivotPoint = building.GetPivotPoint(),
                     size = building.GetSize(),
                     buildingScriptableObject = building.BuildingScriptableObject,
                     yRotation = building.transform.eulerAngles.y
                 }))
        {
            saveData.GridSave.gridCells.Add(buildingData);
        }
    }

    public void LoadFromSaveData(SaveData saveData)
    {
    }
}