using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class TableGrid : MonoBehaviour, ISaveable
{
    public event Action<GridCell> OnBuildingPlaced;
    public event Action<GridCell> OnBuildingRemoved;

    public event Action<TrainEngine, Vector3, float> OnEnginePlaced;
    public event Action<TrainEngine> OnEnginePickedUp;
    public event Action<TrainEngine> OnEngineRemoved;

    public event Action<TrainEngine> OnCancelEnginePickup;

    [Serializable]
    public class GridCell
    {
        public GridBuildable building;
    }

    [SerializeField] private AudioClip placeBuildingSound;
    [SerializeField] private AudioClip placeEngineSound;
    [SerializeField] private AudioClip removeBuildingSound;
    [SerializeField] private Transform gridLayerContainer;
    [SerializeField] private List<GridCell> gridCells;
    [SerializeField] private LayerMask activeLayerMask;
    private float currentRotation;
    private Camera mainCamera;
    private GridBuildable gridBuildingPrefab;
    private GameObject trainEngineDrag;
    private TrainEngine pickedUpTrainEngine;
    private bool isDraggingEngine;
    private AudioSource audioSource;

    // Start is called before the first frame update
    private void Awake()
    {
        mainCamera = Camera.main;
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        // Make the engine prefab visual only

        // Loop over all children of the grid layer container and add them to the grid cells list.
        for (var i = 0; i < gridLayerContainer.childCount; i++)
        {
            var child = gridLayerContainer.GetChild(i);
            var gridCell = new GridCell
            {
                building = child.GetComponent<GridBuildable>()
            };
            gridCells.Add(gridCell);
            OnBuildingPlaced?.Invoke(gridCell);
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
        trainEngineDrag = Instantiate(prefab, transform).gameObject;
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
        PlaceBuilding(position, selectedHotBarButton.BuildingPrefab, selectedHotBarButton.BuildingSize, yPos);
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
            if (isDraggingEngine)
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
                        instance.Speed = selectedHotBarButton.trainEngineScriptableObject.engineSpeed;
                        PlaceEngine(instance, hit.point);
                    }
                }
            }
            else if (hit.collider.CompareTag("TrainEngine"))
            {
                var trainEngine = hit.collider.GetComponent<TrainEngine>();
                if (!trainEngine) return;
                if (gridBuildingPrefab)
                {
                    Destroy(gridBuildingPrefab.gameObject);
                }

                InstantiatedDragEngine(trainEngine.TrainEngineScriptableObject.enginePrefab);
                pickedUpTrainEngine = hit.collider.GetComponent<TrainEngine>();
                OnEnginePickedUp?.Invoke(pickedUpTrainEngine);
            }
            else if (selectedHotBarButton?.BuildingPrefab)
            {
                PlaceBuildingFromHotBar(hit.point);
            }
            else
            {
                ClearBuilding(hit.point);
            }
        }

        if (isDraggingEngine)
        {
            RenderEngine(hit.point);
            return;
        }

        if (selectedHotBarButton && gridBuildingPrefab)
        {
            var yPos = selectedHotBarButton.BuildingPrefab.transform.position.y;
            RenderPrefab(hit.point, yPos, gridBuildingPrefab.GetSize());
        }
    }

    private void RemoveEngine(TrainEngine trainEngine)
    {
        OnEngineRemoved?.Invoke(trainEngine);
    }

    private void RenderEngine(Vector3 position)
    {
        if (!trainEngineDrag.gameObject.activeSelf)
        {
            trainEngineDrag.gameObject.SetActive(true);
        }

        var gridCoord = GridPosToGridCoord(WorldToGridPos(position));
        var gridPos = GridCoordToGridCoordPos(gridCoord);
        var gridCell = gridCells.FirstOrDefault(gridCell => gridCell.building.Rect.Contains(gridCoord));
        var trainTrack = gridCell?.building.GetComponent<TrainTrack>();
        if (trainTrack)
        {
            // Local position used to render relative to the grid layer container.
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
        var gridCoord = GridPosToGridCoord(WorldToGridPos(position));
        var gridCell = gridCells.FirstOrDefault(gridCell => gridCell.building.Rect.Contains(gridCoord));
        if (gridCell == null) return;
        var trainTrack = gridCell.building.GetComponent<TrainTrack>();
        if (!trainTrack) return;

        audioSource.PlayOneShot(placeEngineSound);
        isDraggingEngine = false;
        Destroy(trainEngineDrag);
        var yRotation = gridCell.building.transform.eulerAngles.y +
                        (trainTrack.TrackScriptableObject.trackType == TrackType.Curve ? 45 : 0);
        var gridPos = GridCoordToGridCoordPos(gridCoord);
        OnEnginePlaced?.Invoke(trainEngine, gridPos, yRotation);
    }

    private void ClearBuilding(Vector3 position)
    {
        var gridCoord = GridPosToGridCoord(WorldToGridPos(position));
        var gridCell = gridCells.Find(cell => cell.building.Rect.Contains(gridCoord));
        if (gridCell == null) return;

        audioSource.PlayOneShot(removeBuildingSound);
        ClearGridCell(gridCell);
        gridCells.Remove(gridCell);
    }

    private void ClearGridCell(GridCell gridCell)
    {
        Destroy(gridCell.building.gameObject);
        OnBuildingRemoved?.Invoke(gridCell);
    }

    private void RenderPrefab(Vector3 position, float yPos, Vector2 size)
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
        var gridCoord = GridPosToGridCoord(WorldToGridPos(position));
        var gridPos = GridCoordToGridCoordPos(gridCoord);
        gridPos.y = yPos;
        building.transform.localPosition =
            gridPos; // Local position used to render relative to the grid layer container.
        building.transform.rotation = Quaternion.Euler(0, currentRotation, 0);
    }

    private bool GriCellsOccupied(Rect buildingRect)
    {
        return gridCells.Exists(cell => cell.building.Rect.Overlaps(buildingRect));
    }

    private void PlaceBuilding(Vector3 position, GridBuildable prefab, Vector2 size, float yPos)
    {
        audioSource.PlayOneShot(placeBuildingSound);
        var buildingRect = CreateRotatedRect(WorldToGridPos(position), size, currentRotation);
        if (GriCellsOccupied(buildingRect)) return;

        var building = Instantiate(prefab, gridLayerContainer);
        var gridCell = new GridCell
        {
            building = building,
        };
        gridCells.Add(gridCell);
        SetBuildingTransform(building, position, yPos);
        OnBuildingPlaced?.Invoke(gridCell);
    }

    public static Rect CreateRotatedRect(Vector3 pivotPoint, Vector2 size, float yRotation)
    {
        return new Rect
        {
            position = GridPosToGridCoord(pivotPoint) - GetRectOffset(size, yRotation),
            size = size
        };
    }

    public static Vector2Int GetRectPivotPoint(Rect rect, float yRotation)
    {
        return Vector2Int.RoundToInt(rect.position) + GetRectOffset(rect.size, yRotation);
    }

    private static Vector2Int GetRectOffset(Vector2 size, float yRotation)
    {
        var isCenteredX = size.x % 2 == 0 && size.x > 1 && yRotation is 90 or 180 or -180 or -90;
        var isCenteredY = size.y % 2 == 0 && size.y > 1 && yRotation is 0 or 90;
        return new Vector2Int(
            !isCenteredX ? Mathf.FloorToInt(size.x / 2f) : 0,
            !isCenteredY ? Mathf.FloorToInt(size.y / 2f) : 0
        );
    }

    // Convert the world position to grid local position., within the grid layer container.
    private Vector3 WorldToGridPos(Vector3 worldPos)
    {
        var gridPos = gridLayerContainer.InverseTransformPoint(worldPos);
        return new Vector3(gridPos.x, 0, gridPos.z); // Ignore the y-axis.
    }

    // Convert the grid position to grid coordinates.
    public static Vector2 GridPosToGridCoord(Vector3 gridPos)
    {
        return new Vector2(Mathf.Floor(gridPos.x), Mathf.Floor(gridPos.z));
    }

    // Convert the grid coordinates to grid local position, within the grid layer container.
    private static Vector3 GridCoordToGridCoordPos(Vector2 gridCoord)
    {
        return new Vector3(gridCoord.x + 0.5f, 0, gridCoord.y + 0.5f); // Ignore the y-axis.
    }

    public void PopulateSaveData(SaveData saveData)
    {
    }

    public void LoadFromSaveData(SaveData saveData)
    {
        foreach (var gridCell in gridCells)
        {
            ClearGridCell(gridCell);
        }

        gridCells.Clear();
        foreach (var gridCellSaveData in saveData.GridSave.gridCells)
        {
            GridBuildable gridBuildable = gridCellSaveData.terrainScriptableObject?.terrainBuildablePrefab ??
                                          gridCellSaveData.trackScriptableObject?.trackPrefab ??
                                          gridCellSaveData.buildingScriptableObject?.buildingPrefab
                                              ?.GetComponent<GridBuildable>();
            if (!gridBuildable) continue;

            var position = new Vector3(gridCellSaveData.pivotPoint.x, 0, gridCellSaveData.pivotPoint.y);
            var size = new Vector2(gridCellSaveData.size.x, gridCellSaveData.size.y);
            currentRotation = gridCellSaveData.yRotation;
            PlaceBuilding(position, gridBuildable, size, gridBuildable.transform.position.y);
        }
    }

    private void OnDrawGizmos()
    {
        foreach (var rect in gridCells.Select(gridCell => gridCell.building.Rect))
        {
            // draw the track rect
            Debug.DrawLine(new Vector3(rect.xMin, transform.position.y, rect.yMin),
                new Vector3(rect.xMax, transform.position.y, rect.yMin), Color.yellow);
            Debug.DrawLine(new Vector3(rect.xMax, transform.position.y, rect.yMin),
                new Vector3(rect.xMax, transform.position.y, rect.yMax), Color.yellow);
            Debug.DrawLine(new Vector3(rect.xMax, transform.position.y, rect.yMax),
                new Vector3(rect.xMin, transform.position.y, rect.yMax), Color.yellow);
            Debug.DrawLine(new Vector3(rect.xMin, transform.position.y, rect.yMax),
                new Vector3(rect.xMin, transform.position.y, rect.yMin), Color.yellow);
        }
    }
}