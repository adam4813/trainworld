using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

[RequireComponent(typeof(AudioSource))]
public class GridPlacementSystem : MonoBehaviour, ISaveable
{
    public static event Action<GridBuildable> OnBuildingPlaced;
    public static event Action<GridBuildable> OnBuildingRemoved;
    public static event Action<TrainEngine, Vector3, float> OnEnginePlaced;
    public static event Action<TrainEngine> OnEnginePickedUp;
    public static event Action<TrainEngine> OnEngineRemoved;
    public static event Action<TrainEngine> OnCancelEnginePickup;
    
    [SerializeField] private AudioClip placeBuildingSound;
    [SerializeField] private AudioClip placeEngineSound;
    [SerializeField] private AudioClip removeBuildingSound;
    [SerializeField] private Transform gridLayerContainer;
    [SerializeField] private LayerMask activeLayerMask;
    [SerializeField] private TableGrid tableGrid;
    private float currentRotation;
    private Camera mainCamera;
    private GridBuildable gridBuildingPrefab;
    private GameObject trainEngineDrag;
    private TrainEngine pickedUpTrainEngine;
    private bool isDraggingEngine;
    private AudioSource audioSource;
    
    private void Awake()
    {
        mainCamera = Camera.main;
        audioSource = GetComponent<AudioSource>();
    }
    
    private void Start()
    {
        // Loop over all children of the grid layer container and add them to the grid cells list.
        for (var i = 0; i < gridLayerContainer.childCount; i++)
        {
            var child = gridLayerContainer.GetChild(i);
            if (!child.TryGetComponent<GridBuildable>(out var gridBuildable)) continue;
            
            var gridCell = new TableGrid.GridCell
            {
                building = gridBuildable
            };
            tableGrid.AddGridCell(gridCell);
            OnBuildingPlaced?.Invoke(gridBuildable);
        }
    }
    
    private void OnEnable()
    {
        HotBar.OnHotBarButtonClicked += OnHotBarButtonClicked;
    }

    private void OnDisable()
    {
        HotBar.OnHotBarButtonClicked -= OnHotBarButtonClicked;
    }

    private void InstantiatedDragEngine(TrainEngine prefab)
    {
        if (trainEngineDrag)
        {
            Destroy(trainEngineDrag);
        }

        isDraggingEngine = true;
        trainEngineDrag = Instantiate(prefab, gridLayerContainer.transform).gameObject;
        trainEngineDrag.gameObject.tag = "Untagged";
        trainEngineDrag.gameObject.layer = 2;
        trainEngineDrag.gameObject.SetActive(false);
    }

    private void OnHotBarButtonClicked(HotBarButton selectedHotBarButton)
    {
        currentRotation = 0; // Move to reset only when hot bar button is selected.

        if (isDraggingEngine || trainEngineDrag)
        {
            if (pickedUpTrainEngine)
            {
                OnCancelEnginePickup?.Invoke(pickedUpTrainEngine);
                pickedUpTrainEngine = null;
            }

            if (trainEngineDrag)
            {
                Destroy(trainEngineDrag);
            }

            isDraggingEngine = false;
        }

        if (gridBuildingPrefab)
        {
            Destroy(gridBuildingPrefab.gameObject);
        }

        if (selectedHotBarButton?.EnginePrefab)
        {
            InstantiatedDragEngine(selectedHotBarButton.EnginePrefab);
            return;
        }

        if (!selectedHotBarButton?.BuildingPrefab)
        {
            gridBuildingPrefab = null;
            return;
        }

        gridBuildingPrefab = Instantiate(selectedHotBarButton.BuildingPrefab, gridLayerContainer);
        var yPos = selectedHotBarButton.BuildingPrefab.transform.position.y;
        gridBuildingPrefab.transform.position = new Vector3(0, yPos, 0);
        gridBuildingPrefab.transform.rotation = Quaternion.Euler(0, currentRotation, 0);
    }

    private void PlaceBuildingFromHotBar(Vector3 position)
    {
        var selectedHotBarButton = HotBarManager.Instance.ActiveHotBar?.SelectedHotBarButton;
        if (!selectedHotBarButton) return;
        var yPos = selectedHotBarButton.BuildingPrefab.transform.position.y;
        PlaceBuilding(position, selectedHotBarButton.BuildingPrefab, yPos, currentRotation);
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            currentRotation += 90;
            if (currentRotation >= 360)
            {
                currentRotation = 0;
            }
        }

        var ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out var hit, Mathf.Infinity, activeLayerMask) ||
            !(hit.collider.CompareTag("Grid") || hit.collider.CompareTag("TrainEngine")) ||
            EventSystem.current.IsPointerOverGameObject()) // Check if the mouse is over a UI element.
        {
            if (gridBuildingPrefab && gridBuildingPrefab.gameObject.activeSelf)
            {
                gridBuildingPrefab.gameObject.SetActive(false);
            }

            if (trainEngineDrag && trainEngineDrag.gameObject.activeSelf)
            {
                trainEngineDrag.gameObject.SetActive(false);
            }

            return;
        }

        var selectedHotBarButton = HotBarManager.Instance?.ActiveHotBar?.SelectedHotBarButton;

        if (Keyboard.current.deleteKey.wasPressedThisFrame && isDraggingEngine)
        {
            RemoveEngine(pickedUpTrainEngine);
            Destroy(trainEngineDrag);
            isDraggingEngine = false;
            pickedUpTrainEngine = null;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (isDraggingEngine && CanPlaceEngine(hit.point))
            {
                if (pickedUpTrainEngine)
                {
                    PlaceEngine(pickedUpTrainEngine, trainEngineDrag.transform.position);
                    pickedUpTrainEngine = null;
                }
                else
                {
                    if (selectedHotBarButton?.EnginePrefab)
                    {
                        var instance = Instantiate(selectedHotBarButton.EnginePrefab);
                        instance.Speed = instance.TrainEngineScriptableObject.engineSpeed;
                        PlaceEngine(instance, hit.point);
                    }
                }
            }
            else if (hit.collider.CompareTag("TrainEngine"))
            {
                pickedUpTrainEngine = hit.collider.GetComponent<TrainEngine>();
                if (!pickedUpTrainEngine) return;
                if (gridBuildingPrefab)
                {
                    Destroy(gridBuildingPrefab.gameObject);
                }

                InstantiatedDragEngine(pickedUpTrainEngine.TrainEngineScriptableObject.enginePrefab);
                OnEnginePickedUp?.Invoke(pickedUpTrainEngine);
            }
            else if (selectedHotBarButton?.BuildingPrefab)
            {
                PlaceBuildingFromHotBar(hit.point);
            }
            else if (HotBarManager.Instance?.IsDeleteButtonSelected == true)
            {
                ClearBuilding(hit.point);
            }
        }

        if (isDraggingEngine)
        {
            RenderDragEngine(hit.point);
            return;
        }

        if (selectedHotBarButton && gridBuildingPrefab)
        {
            var yPos = selectedHotBarButton.BuildingPrefab.transform.position.y;
            RenderDragPrefab(hit.point, yPos, gridBuildingPrefab.GetSize());
        }
    }

    private bool CanPlaceEngine(Vector3 position)
    {
        var gridCell = tableGrid.FindGridCell(position);
        if (gridCell == null) return false;
        var trainTrack = gridCell.building.GetComponent<TrainTrack>();
        return trainTrack;
    }

    private void RemoveEngine(TrainEngine trainEngine)
    {
        OnEngineRemoved?.Invoke(trainEngine);
    }

    private void RenderDragEngine(Vector3 position)
    {
        if (!trainEngineDrag.gameObject.activeSelf)
        {
            trainEngineDrag.gameObject.SetActive(true);
        }

        var gridCell = tableGrid.FindGridCell(position);
        var trainTrack = gridCell?.building.GetComponent<TrainTrack>();
        if (trainTrack)
        {
            // Local position used to render relative to the grid layer container.
            var gridCoord = TableGrid.GridPosToGridCoord(tableGrid.WorldToGridPos(position));
            var gridPos = TableGrid.GridCoordToGridCoordPos(gridCoord);
            trainEngineDrag.transform.localPosition = gridPos;
            trainEngineDrag.transform.rotation = Quaternion.Euler(0,
                gridCell.building.transform.eulerAngles.y +
                (trainTrack.TrackScriptableObject.trackType == TrackType.Curve ? 45 : 0),
                0);
        }
        else
        {
            // Local position used to render relative to the grid layer container.
            trainEngineDrag.transform.localPosition = new Vector3(position.x, 0f, position.z);
            trainEngineDrag.transform.rotation = Quaternion.Euler(0, currentRotation, 0);
        }
    }

    private void PlaceEngine(TrainEngine trainEngine, Vector3 position)
    {
        // Check if the engine is being placed on a track.
        var gridCell = tableGrid.FindGridCell(position);
        if (gridCell == null) return;
        var trainTrack = gridCell.building.GetComponent<TrainTrack>();
        if (!trainTrack) return;

        audioSource.PlayOneShot(placeEngineSound);
        isDraggingEngine = false;
        Destroy(trainEngineDrag);
        var yRotation = gridCell.building.transform.eulerAngles.y +
                        (trainTrack.TrackScriptableObject.trackType == TrackType.Curve ? 45 : 0);
        var gridCoord = TableGrid.GridPosToGridCoord(tableGrid.WorldToGridPos(position));
        var gridPos = TableGrid.GridCoordToGridCoordPos(gridCoord);
        OnEnginePlaced?.Invoke(trainEngine, gridPos, yRotation);
    }

    private void ClearBuilding(Vector3 position)
    {
        var gridCell = tableGrid.FindGridCell(position);
        if (gridCell == null) return;

        audioSource.PlayOneShot(removeBuildingSound);
        tableGrid.ClearGridCell(gridCell);
        OnBuildingRemoved?.Invoke(gridCell.building);
    }

    private void RenderDragPrefab(Vector3 position, float yPos, Vector2 size)
    {
        if (!gridBuildingPrefab.gameObject.activeSelf)
        {
            gridBuildingPrefab.gameObject.SetActive(true);
        }

        SetBuildingTransform(gridBuildingPrefab, position, yPos);
#if UNITY_EDITOR
        gridBuildingPrefab.UpdateRect(size);
#endif
    }

    private void SetBuildingTransform(GridBuildable building, Vector3 position, float yPos)
    {
        var worldPivotPoint = tableGrid.GetPivotPoint(position, building.GetSize(), currentRotation);
        worldPivotPoint.y = yPos;
        building.transform.localPosition =
            worldPivotPoint; // Local position used to render relative to the grid layer container.
        building.transform.rotation = Quaternion.Euler(0, currentRotation, 0);
    }

    private void PlaceBuilding(Vector3 position, GridBuildable prefab, float yPos, float yRotation)
    {
        var worldPivotPoint = tableGrid.GetPivotPoint(position, prefab.GetSize(), yRotation);
        var buildingRect = CreateBuildingRect(worldPivotPoint, prefab.GetSize(), yRotation);
        if (tableGrid.GriCellsOccupied(buildingRect)) return;

        audioSource.PlayOneShot(placeBuildingSound);

        var building = Instantiate(prefab, gridLayerContainer);
        var gridCell = new TableGrid.GridCell
        {
            building = building,
        };
        tableGrid.AddGridCell(gridCell);
        SetBuildingTransform(building, position, yPos);
        OnBuildingPlaced?.Invoke(building);
    }

    public static Rect CreateBuildingRect(Vector3 pivotPoint, Vector2 size, float yRotation)
    {
        return new Rect
        {
            position = TableGrid.GridPosToGridCoord(pivotPoint) - TableGrid.GetRectOffset(size, yRotation),
            size = size
        };
    }
    public void PopulateSaveData(SaveData saveData)
    {
    }

    public void LoadFromSaveData(SaveData saveData)
    {
        tableGrid.ClearAllCells();

        foreach (var gridCellSaveData in saveData.GridSave.gridCells)
        {
            GridBuildable gridBuildable = gridCellSaveData.terrainScriptableObject?.terrainBuildablePrefab ??
                                          gridCellSaveData.trackScriptableObject?.trackPrefab ??
                                          gridCellSaveData.buildingScriptableObject?.buildingPrefab
                                              ?.GetComponent<GridBuildable>();
            if (!gridBuildable) continue;

            var position = new Vector3(gridCellSaveData.pivotPoint.x, 0, gridCellSaveData.pivotPoint.y);
            PlaceBuilding(position, gridBuildable, gridBuildable.transform.position.y, gridCellSaveData.yRotation);
        }
    }
}