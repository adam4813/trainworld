using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuildingManager : MonoBehaviour, ISaveable
{
    [SerializeField] private List<Building> buildings;
    [SerializeField] private TableGrid tableGrid;

    private void OnEnable()
    {
        tableGrid.OnBuildingPlaced += OnBuildingPlaced;
        tableGrid.OnBuildingRemoved += OnBuildingRemoved;
    }

    private void OnDisable()
    {
        tableGrid.OnBuildingPlaced -= OnBuildingPlaced;
        tableGrid.OnBuildingRemoved -= OnBuildingRemoved;
    }

    private void OnBuildingPlaced(TableGrid.GridCell gridCell)
    {
        if (!gridCell.building.TryGetComponent<Building>(out var building)) return;

        building.UpdateRect(building.BuildingScriptableObject.BuildingSize);

        buildings.Add(building);
    }

    private void OnBuildingRemoved(TableGrid.GridCell gridCell)
    {
        if (gridCell.building.TryGetComponent<Building>(out var building))
        {
            buildings.Remove(building);
        }
    }

    public void PopulateSaveData(SaveData saveData)
    {
        foreach (var buildingData in buildings.Select(building => new GridSaveData.GridCellSaveData()
                 {
                     pivotPoint = building.GetPivotPoint(),
                     size = building.Size,
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