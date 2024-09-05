using System;
using UnityEngine;

public class PopularityManager : MonoBehaviour
{
    // Action when popularity changes
    public static Action<int> PopularityChanged;

    [SerializeField] private int popularity;
    [SerializeField] private TableGrid tableGrid;

    private void Start()
    {
        PopularityChanged?.Invoke(popularity);
    }

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

        var buildingScriptableObject = building.BuildingScriptableObject;
        SetPopularity(popularity + buildingScriptableObject.popularity);
    }

    private void OnBuildingRemoved(GridBuildable buildable)
    {
        if (!buildable.TryGetComponent<Building>(out var building)) return;

        var buildingScriptableObject = building.BuildingScriptableObject;
        SetPopularity(popularity - buildingScriptableObject.popularity);
    }

    private void SetPopularity(int value)
    {
        popularity = value;
        PopularityChanged?.Invoke(popularity);
    }
}