using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Track : MonoBehaviour, ISaveable
{
    [SerializeField] private List<TrainTrack> trainTracks;
    [SerializeField] private List<TrainEngine> trainEngines;
    [SerializeField] private TrainEngine trainEnginesPrefab;
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
        if (!gridCell.building.TryGetComponent<TrainTrack>(out var trainTrack)) return;

        trainTrack.rect = gridCell.rect;
        trainTrack.rect.size = trainTrack.TrackScriptableObject.TrackSize;
        if (trainTrack.rect.size.x > 1)
        {
            if (gridCell.building.transform.eulerAngles.y is 90 or 180)
            {
                trainTrack.rect.x -= 1;
            }
        }

        if (trainTrack.rect.size.y > 1)
        {
            if (gridCell.building.transform.eulerAngles.y < 180)
            {
                trainTrack.rect.y -= 1;
            }
        }

        trainTracks.Add(trainTrack);
    }

    private void OnBuildingRemoved(TableGrid.GridCell gridCell)
    {
        if (gridCell.building.TryGetComponent<TrainTrack>(out var trainTrack))
        {
            trainTracks.Remove(trainTrack);
        }
    }

    private void Update()
    {
        foreach (var engine in trainEngines)
        {
            if (engine.IsMoving) continue;

            var enginePosition = engine.transform.position;
            var engineGridCoord = tableGrid.GridPosToGridCoord(enginePosition);
            var currentTrack = trainTracks.FirstOrDefault(trackCell => trackCell.rect.Contains(engineGridCoord));

            var nextEnginePosition = enginePosition + engine.transform.forward;
            var nextEngineGridCoord = tableGrid.GridPosToGridCoord(nextEnginePosition);
            var nextTrack = trainTracks.FirstOrDefault(trackCell => trackCell.rect.Contains(nextEngineGridCoord));

            if (!currentTrack || !nextTrack || currentTrack == nextTrack) continue;

            var path = nextTrack.GetClosestPath(nextEnginePosition);
            if (path.Count < 2) continue;

            path.Sort((pathA, pathB) =>
                Vector3.Distance(pathA.position, nextEnginePosition) >
                Vector3.Distance(pathB.position, nextEnginePosition)
                    ? 1
                    : -1
            );
            
            engine.SetPath(path);
        }
    }

    public void PopulateSaveData(SaveData saveData)
    {
        foreach (var trainSaveData in trainEngines.Select(trainEngine => new TrainSaveData
                 {
                     position = trainEngine.transform.position,
                     yRotation = trainEngine.transform.eulerAngles.y,
                     speed = trainEngine.Speed
                 }))
        {
            saveData.TrainListSave.Add(trainSaveData);
        }
    }

    public void LoadFromSaveData(SaveData saveData)
    {
        foreach (var trainEngine in trainEngines)
        {
            Destroy(trainEngine.gameObject);
        }

        trainEngines.Clear();
        foreach (var trainSaveData in saveData.TrainListSave)
        {
            var trainEngine = Instantiate(trainEnginesPrefab, transform);
            trainEngines.Add(trainEngine);

            trainEngine.transform.position = trainSaveData.position;
            trainEngine.transform.eulerAngles = new Vector3(0, trainSaveData.yRotation, 0);
            trainEngine.Speed = trainSaveData.speed;
            trainEngine.gameObject.SetActive(true);
        }
    }
}