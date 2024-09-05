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
        SetPopularity(popularity + GetBuildingPopularity(buildable));
    }

    private void OnBuildingRemoved(GridBuildable buildable)
    {
        SetPopularity(popularity - GetBuildingPopularity(buildable));
    }
    
    private static int GetBuildingPopularity(GridBuildable buildable)
    {
        if (!buildable.TryGetComponent<Building>(out var building)) return 0;

        var buildingScriptableObject = building.BuildingScriptableObject;
        return buildingScriptableObject.popularity;
    }

    private void SetPopularity(int value)
    {
        popularity = value;
        PopularityChanged?.Invoke(popularity);
    }
}