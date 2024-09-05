using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrackManager : MonoBehaviour, ISaveable
{
    [SerializeField] private List<TrainTrack> trainTracks;
    [SerializeField] private List<TrainEngine> trainEngines;
    [SerializeField] private List<TrainStation> trainStations;
    [SerializeField] private Transform trainEngineContainer;
    [SerializeField] private TableGrid tableGrid;

    public static TrackManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        tableGrid.OnBuildingPlaced += OnBuildingPlaced;
        tableGrid.OnBuildingRemoved += OnBuildingRemoved;
        tableGrid.OnEnginePickedUp += OnTrainEnginePickedUp;
        tableGrid.OnEnginePlaced += OnTrainEnginePlaced;
        tableGrid.OnEngineRemoved += OnTrainEngineRemoved;
        tableGrid.OnCancelEnginePickup += OnCancelTrainEnginePickup;
    }

    private void OnTrainEngineRemoved(TrainEngine trainEngine)
    {
        trainEngines.Remove(trainEngine);
        Destroy(trainEngine.gameObject);
    }

    private void OnDisable()
    {
        tableGrid.OnBuildingPlaced -= OnBuildingPlaced;
        tableGrid.OnBuildingRemoved -= OnBuildingRemoved;
        tableGrid.OnEnginePickedUp -= OnTrainEnginePickedUp;
        tableGrid.OnEnginePlaced -= OnTrainEnginePlaced;
        tableGrid.OnEngineRemoved -= OnTrainEngineRemoved;
        tableGrid.OnCancelEnginePickup -= OnCancelTrainEnginePickup;
    }

    private void OnCancelTrainEnginePickup(TrainEngine trainEngine)
    {
        trainEngine.gameObject.SetActive(true);
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
        if (gridCell.building.TryGetComponent<TrainTrack>(out var trainTrack))
        {
            trainTrack.UpdateRect(trainTrack.GetSize());
            trainTracks.Add(trainTrack);
        }
        else if (gridCell.building.TryGetComponent<TrainStation>(out var trainStation))
        {
            trainStations.Add(trainStation);
        }
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
            var engineGridCoord = TableGrid.GridPosToGridCoord(enginePosition);
            var currentTrack = trainTracks.FirstOrDefault(trackCell => trackCell.Rect.Contains(engineGridCoord));

            var nextEnginePosition = enginePosition + engine.EngineOffset() * 2f;
            var nextEngineGridCoord = TableGrid.GridPosToGridCoord(nextEnginePosition);
            var nextTrack = trainTracks.FirstOrDefault(trackCell => trackCell.Rect.Contains(nextEngineGridCoord));

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
                     speed = trainEngine.Speed,
                     trainEngineScriptableObject = trainEngine.TrainEngineScriptableObject
                 }))
        {
            saveData.TrainListSave.Add(trainSaveData);
        }

        foreach (var trainTackSaveData in trainTracks.Select(trainTrack => new GridSaveData.GridCellSaveData()
                 {
                     pivotPoint = trainTrack.GetPivotPoint(),
                     size = trainTrack.GetSize(),
                     trackScriptableObject = trainTrack.TrackScriptableObject,
                     yRotation = trainTrack.transform.eulerAngles.y
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
            var trainEngine = Instantiate(trainSaveData.trainEngineScriptableObject.enginePrefab, trainEngineContainer);
            trainEngines.Add(trainEngine);

            trainEngine.transform.position = trainSaveData.position;
            trainEngine.transform.rotation = Quaternion.Euler(0f, trainSaveData.yRotation, 0f);
            trainEngine.Speed = trainSaveData.speed;
            trainEngine.gameObject.SetActive(true);
        }
    }

    public TrainStation GetNextStation(TrainStation currentStation, TrainTrack connectedTrack)
    {
        // Todo: Implement this method
        return trainStations[Random.Range(0, trainStations.Count)];
    }
}