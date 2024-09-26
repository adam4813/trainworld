using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;
using Random = UnityEngine.Random;

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

    private void Start()
    {
        // Loop over all children of the train layer container and add them to the grid cells list.
        for (var i = 0; i < trainEngineContainer.childCount; i++)
        {
            var child = trainEngineContainer.GetChild(i);
            if (child.TryGetComponent<TrainEngine>(out var trainEngine))
            {
                trainEngines.Add(trainEngine);
            }
        }
    }

    private void OnEnable()
    {
        GridPlacementSystem.OnBuildingPlaced += OnBuildingPlaced;
        GridPlacementSystem.OnBuildingRemoved += OnBuildingRemoved;
        GridPlacementSystem.OnEnginePickedUp += OnTrainEnginePickedUp;
        GridPlacementSystem.OnEnginePlaced += OnTrainEnginePlaced;
        GridPlacementSystem.OnEngineRemoved += OnTrainEngineRemoved;
        GridPlacementSystem.OnCancelEnginePickup += OnCancelTrainEnginePickup;
    }

    private void OnTrainEngineRemoved(TrainEngine trainEngine)
    {
        trainEngines.Remove(trainEngine);
        Destroy(trainEngine.gameObject);
    }

    private void OnDisable()
    {
        GridPlacementSystem.OnBuildingPlaced -= OnBuildingPlaced;
        GridPlacementSystem.OnBuildingRemoved -= OnBuildingRemoved;
        GridPlacementSystem.OnEnginePickedUp -= OnTrainEnginePickedUp;
        GridPlacementSystem.OnEnginePlaced -= OnTrainEnginePlaced;
        GridPlacementSystem.OnEngineRemoved -= OnTrainEngineRemoved;
        GridPlacementSystem.OnCancelEnginePickup -= OnCancelTrainEnginePickup;
    }

    private void OnCancelTrainEnginePickup(TrainEngine trainEngine)
    {
        trainEngine.gameObject.SetActive(true);
    }

    private void OnTrainEnginePickedUp(TrainEngine trainEngine)
    {
        trainEngine.gameObject.SetActive(false);
        trainEngine.ClearSpline();
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
        trainEngine.transform.localPosition = new Vector3(position.x, -1.25f, position.z);
        SetTrainEngineTrack(trainEngine, position);
    }

    private void OnBuildingPlaced(GridBuildable buildable)
    {
        if (buildable.TryGetComponent<TrainTrack>(out var trainTrack))
        {
            trainTrack.UpdateRect(trainTrack.GetSize());
            trainTracks.Add(trainTrack);
        }
        else if (buildable.TryGetComponent<TrainStation>(out var trainStation))
        {
            trainStations.Add(trainStation);
        }
    }

    private void OnBuildingRemoved(GridBuildable buildable)
    {
        if (buildable.TryGetComponent<TrainTrack>(out var trainTrack))
        {
            trainTracks.Remove(trainTrack);
        }
    }

    private void Update()
    {
        foreach (var trainEngine in trainEngines)
        {
            if (trainEngine.IsMoving || !trainEngine.gameObject.activeSelf) continue;

            var nextEnginePosition = trainEngine.transform.position + trainEngine.EngineOffset();
            SetTrainEngineTrack(trainEngine, nextEnginePosition);
        }
    }

    private void SetTrainEngineTrack(TrainEngine trainEngine, Vector3 nextEnginePosition)
    {
        var track = GetTrack(Vector2Int.RoundToInt(TableGrid.GridPosToGridCoord(nextEnginePosition)));

        if (!track)
        {
            trainEngine.ClearSpline();
        }
        else
        {
            trainEngine.SetSplinePath(track.GetSpline(trainEngine.transform.position));
        }
    }

    private TrainTrack GetTrack(Vector2Int gridCoord)
    {
        return trainTracks.FirstOrDefault(trackCell => trackCell.Rect.Contains(gridCoord));
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