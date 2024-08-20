using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrackManager : MonoBehaviour, ISaveable
{
    [SerializeField] private List<TrainTrack> trainTracks;
    [SerializeField] private List<TrainEngine> trainEngines;
    [SerializeField] private Transform trainEngineContainer;
    [SerializeField] private TrainEngine trainEnginesPrefab;
    [SerializeField] private TableGrid tableGrid;
    private void OnEnable()
    {
        tableGrid.OnBuildingPlaced += OnBuildingPlaced;
        tableGrid.OnBuildingRemoved += OnBuildingRemoved;
        tableGrid.OnEnginePickedUp += OnTrainEnginePickedUp;
        tableGrid.OnEnginePlaced += OnTrainEnginePlaced;
    }

    private void OnDisable()
    {
        tableGrid.OnBuildingPlaced -= OnBuildingPlaced;
        tableGrid.OnBuildingRemoved -= OnBuildingRemoved;
        tableGrid.OnEnginePickedUp -= OnTrainEnginePickedUp;
        tableGrid.OnEnginePlaced -= OnTrainEnginePlaced;
    }

    private void OnTrainEnginePickedUp(TrainEngine trainEngine)
    {
        trainEngine.gameObject.SetActive(false);
        trainEngine.ClearPath();
    }

    private void OnTrainEnginePlaced(TrainEngine trainEngine, Vector3 position, float yRotation)
    {
        if (trainEngines.Contains(trainEngine))
        {
            trainEngine.gameObject.SetActive(true);
        }
        else
        {
            trainEngines.Add(trainEngine);
            trainEngine.transform.parent = trainEngineContainer;
        }

        trainEngine.transform.rotation = Quaternion.Euler(0f, yRotation, 0f);
        trainEngine.transform.localPosition = new Vector3(position.x, -1.25f, position.z) + trainEngine.EngineOffset();
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
            if (engine.IsMoving || !engine.gameObject.activeSelf) continue;

            var enginePosition = engine.transform.position;
            var engineGridCoord = tableGrid.GridPosToGridCoord(enginePosition);
            var currentTrack = trainTracks.FirstOrDefault(trackCell => trackCell.rect.Contains(engineGridCoord));

            var nextEnginePosition = enginePosition + engine.EngineOffset() * 2f;
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
        foreach (var trainTackSaveData in trainTracks.Select(trainTrack =>
                 {
                     var rect = trainTrack.rect;
                     if (rect.size.x > 1)
                     {
                         if (trainTrack.transform.eulerAngles.y is 90 or 180)
                         {
                             rect.x += 1;
                         }
                     }

                     if (rect.size.y > 1)
                     {
                         if (trainTrack.transform.eulerAngles.y < 180)
                         {
                             rect.y += 1;
                         }
                     }
                     return new GridSaveData.GridCellSaveData()
                     {
                         position = new Vector2Int((int)rect.position.x, (int)rect.position.y),
                         size = new Vector2Int((int)rect.size.x, (int)rect.size.y),
                         trackScriptableObject = trainTrack.TrackScriptableObject,
                         yRotation = trainTrack.transform.eulerAngles.y
                     };
                 }))
        {
            saveData.GridSave.gridCells.Add(trainTackSaveData);
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
            var trainEngine = Instantiate(trainEnginesPrefab, trainEngineContainer);
            trainEngines.Add(trainEngine);

            trainEngine.transform.position = trainSaveData.position;
            trainEngine.transform.rotation = Quaternion.Euler(0f, trainSaveData.yRotation, 0f);
            trainEngine.Speed = trainSaveData.speed;
            trainEngine.gameObject.SetActive(true);
        }
    }
}