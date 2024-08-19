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

        building.rect = gridCell.rect;
        building.rect.size = building.BuildingScriptableObject.BuildingSize;
        if (building.rect.size.x > 1)
        {
            if (gridCell.building.transform.eulerAngles.y is 90 or 180)
            {
                building.rect.x -= 1;
            }
        }

        if (building.rect.size.y > 1)
        {
            if (gridCell.building.transform.eulerAngles.y < 180)
            {
                building.rect.y -= 1;
            }
        }

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
                     position = new Vector2Int((int)building.rect.position.x, (int)building.rect.position.y),
                     size = new Vector2Int((int)building.rect.size.x, (int)building.rect.size.y),
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