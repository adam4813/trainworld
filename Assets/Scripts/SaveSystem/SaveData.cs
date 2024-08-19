using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct GridSaveData
{
    [Serializable]
    public struct GridCellSaveData
    {
        public Vector2Int position;
        public Vector2Int size;
        public float yRotation;
        public TrackScriptableObject trackScriptableObject;
        public BuildingScriptableObject buildingScriptableObject;
    }

    public List<GridCellSaveData> gridCells;
}

[Serializable]
public struct TrainSaveData
{
    public Vector3 position;
    public float yRotation;
    public float speed;
}

[Serializable]
public class SaveData
{
    public GridSaveData GridSave = new() { gridCells = new List<GridSaveData.GridCellSaveData>() };
    public List<TrainSaveData> TrainListSave = new();

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public void LoadFromJson(string json)
    {
        JsonUtility.FromJsonOverwrite(json, this);
    }
}

public interface ISaveable
{
    void PopulateSaveData(SaveData saveData);
    void LoadFromSaveData(SaveData saveData);
}