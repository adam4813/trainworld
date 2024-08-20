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

    [Serializable]
    public class GridCell
    {
        public Rect rect;
        public GameObject building;
    }

    [SerializeField] private Transform gridLayerContainer;
    [SerializeField] private List<GridCell> gridCells;
    [SerializeField] private LayerMask activeLayerMask;
    [SerializeField] private TrainEngine trainEnginePrefab;
    private float currentRotation;
    private Camera _camera;
    private GameObject buildingPrefab;
    private TrainEngine trainEngineDrag;
    private TrainEngine pickedUpTrainEngine;
    private bool isDraggingEngine;

    // Start is called before the first frame update
    private void Awake()
    {
        _camera = Camera.main;
        trainEngineDrag = Instantiate(trainEnginePrefab, transform);
        trainEngineDrag.gameObject.SetActive(false);
    }

    private void Start()
    {
        // Make the engine prefab visual only
        trainEngineDrag.gameObject.tag = "Untagged";
        trainEngineDrag.gameObject.layer = 2;

        // Loop over all children of the grid layer container and add them to the grid cells list.
        for (var i = 0; i < gridLayerContainer.childCount; i++)
        {
            var child = gridLayerContainer.GetChild(i);
            var gridCell = new GridCell
            {
                building = child.gameObject,
                rect = new Rect
                {
                    position = GridPosToGridCoord(WorldToGridPos(child.position)),
                    size = new Vector2(1, 1),
                },
            };
            gridCells.Add(gridCell);
            OnBuildingPlaced?.Invoke(gridCell);
        }
    }

    private void OnEnable()
    {
        HotBar.Instance.OnHotBarButtonClicked += OnHotBarButtonClicked;
        HotBarMenuOption.OnHotBarMenuOptionClicked += OnHotBarMenuOptionClicked;
    }

    private void OnDisable()
    {
        HotBar.Instance.OnHotBarButtonClicked -= OnHotBarButtonClicked;
        HotBarMenuOption.OnHotBarMenuOptionClicked -= OnHotBarMenuOptionClicked;
    }

    private void OnHotBarMenuOptionClicked(HotBarMenuOption hotBarMenuOption)
    {
        if (hotBarMenuOption.MenuOptionType == MenuOptionType.Build)
        {
        }
    }

    private void OnHotBarButtonClicked(HotBarButton obj)
    {
        currentRotation = 0; // Move to reset only when hot bar button is selected.

        var selectedHotBarButton = HotBar.Instance.SelectedHotBarButton;
        if (!selectedHotBarButton) return;

        if (buildingPrefab)
        {
            Destroy(buildingPrefab);
        }

        if (!selectedHotBarButton.BuildingPrefab)
        {
            buildingPrefab = null;
            return;
        }

        buildingPrefab = Instantiate(selectedHotBarButton.BuildingPrefab, gridLayerContainer);
        var yPos = selectedHotBarButton.BuildingPrefab.transform.position.y;
        buildingPrefab.transform.position = new Vector3(0, yPos, 0);
        buildingPrefab.transform.rotation = Quaternion.Euler(0, currentRotation, 0);
    }

    private void PlaceBuildingFromHotBar(Vector3 position)
    {
        var selectedHotBarButton = HotBar.Instance.SelectedHotBarButton;
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

        var selectedHotBarButton = HotBar.Instance.SelectedHotBarButton;

        if (!selectedHotBarButton) return;

        var ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out var hit, Mathf.Infinity, activeLayerMask) ||
            !(hit.collider.CompareTag("Grid") || hit.collider.CompareTag("TrainEngine")) ||
            EventSystem.current.IsPointerOverGameObject()) // Check if the mouse is over a UI element.
        {
            if (buildingPrefab && buildingPrefab.gameObject.activeSelf)
            {
                buildingPrefab.gameObject.SetActive(false);
            }

            if (trainEnginePrefab && trainEnginePrefab.gameObject.activeSelf)
            {
                trainEngineDrag.gameObject.SetActive(false);
            }

            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (isDraggingEngine)
            {
                PlaceEngine();
                trainEngineDrag.gameObject.SetActive(false);
            }
            else if (hit.collider.CompareTag("TrainEngine"))
            {
                isDraggingEngine = true;
                pickedUpTrainEngine = hit.collider.GetComponent<TrainEngine>();
                OnEnginePickedUp?.Invoke(pickedUpTrainEngine);
            }
            else if (selectedHotBarButton.BuildingPrefab)
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

        if (buildingPrefab)
        {
            var yPos = selectedHotBarButton.BuildingPrefab.transform.position.y;
            RenderPrefab(hit.point, yPos);
        }
    }

    private void RenderEngine(Vector3 position)
    {
        if (!trainEngineDrag.gameObject.activeSelf)
        {
            trainEngineDrag.gameObject.SetActive(true);
        }

        var gridCoord = GridPosToGridCoord(WorldToGridPos(position));
        var gridPos = GridCoordToGridCoordPos(gridCoord);
        var gridCell = gridCells.FirstOrDefault(gridCell => gridCell.rect.Contains(gridCoord));
        if (gridCell != null)
        {
            var trainTrack = gridCell.building.GetComponent<TrainTrack>();
            // Local position used to render relative to the grid layer container.
            trainEngineDrag.transform.localPosition = gridPos;
            trainEngineDrag.transform.rotation = Quaternion.Euler(0,
                gridCell.building.transform.eulerAngles.y +
                (trainTrack.TrackScriptableObject.TrackType == TrackType.Curve ? 45 : 0),
                0);
        }
        else
        {
            // Local position used to render relative to the grid layer container.
            trainEngineDrag.transform.localPosition = new Vector3(position.x, 0f, position.z);
            trainEngineDrag.transform.rotation = Quaternion.Euler(0, currentRotation, 0);
        }
    }

    private void PlaceEngine()
    {
        // Check if the engine is being placed on a track.
        var gridCoord = GridPosToGridCoord(WorldToGridPos(trainEngineDrag.transform.position));
        var gridPos = GridCoordToGridCoordPos(gridCoord);
        var gridCell = gridCells.FirstOrDefault(gridCell => gridCell.rect.Contains(gridCoord));
        if (gridCell == null) return;
        var trainTrack = gridCell.building.GetComponent<TrainTrack>();
        if (!trainTrack) return;
        
        trainEngineDrag.gameObject.SetActive(false);
        isDraggingEngine = false;
        var yRotation = gridCell.building.transform.eulerAngles.y +
                        (trainTrack.TrackScriptableObject.TrackType == TrackType.Curve ? 45 : 0);
        OnEnginePlaced?.Invoke(pickedUpTrainEngine ? pickedUpTrainEngine : Instantiate(trainEnginePrefab), gridPos,
            yRotation);
    }

    private void ClearBuilding(Vector3 position)
    {
        var gridCoord = GridPosToGridCoord(WorldToGridPos(position));
        var testRect = new Rect(gridCoord, Vector2.one);
        if (!GriCellsOccupied(testRect)) return;

        var gridCell = gridCells.Find(cell => cell.rect.Overlaps(testRect));
        ClearGridCell(gridCell);
        gridCells.Remove(gridCell);
    }

    private void ClearGridCell(GridCell gridCell)
    {
        Destroy(gridCell.building);
        OnBuildingRemoved?.Invoke(gridCell);
    }

    private void RenderPrefab(Vector3 position, float yPos)
    {
        if (!buildingPrefab.gameObject.activeSelf)
        {
            buildingPrefab.gameObject.SetActive(true);
        }

        SetBuildingTransform(buildingPrefab, position, yPos);
    }

    private void SetBuildingTransform(GameObject building, Vector3 position, float yPos)
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
        return gridCells.Exists(cell => cell.rect.Overlaps(buildingRect));
    }

    private void PlaceBuilding(Vector3 position, GameObject prefab, Vector2 size, float yPos)
    {
        var buildingRect = new Rect
        {
            position = GridPosToGridCoord(WorldToGridPos(position)),
            size = size
        };
        if (GriCellsOccupied(buildingRect)) return;

        var instance = Instantiate(prefab, gridLayerContainer);
        var gridCell = new GridCell
        {
            building = instance,
            rect = buildingRect,
        };
        gridCells.Add(gridCell);
        SetBuildingTransform(instance, position, yPos);
        OnBuildingPlaced?.Invoke(gridCell);
    }

    // Convert the world position to grid local position., within the grid layer container.
    private Vector3 WorldToGridPos(Vector3 worldPos)
    {
        var gridPos = gridLayerContainer.InverseTransformPoint(worldPos);
        return new Vector3(gridPos.x, 0, gridPos.z); // Ignore the y-axis.
    }

    // Convert the grid position to grid coordinates.
    public Vector2 GridPosToGridCoord(Vector3 gridPos)
    {
        return new Vector2(Mathf.Floor(gridPos.x), Mathf.Floor(gridPos.z));
    }

    // Convert the grid coordinates to grid local position, within the grid layer container.
    private Vector3 GridCoordToGridCoordPos(Vector2 gridCoord)
    {
        return new Vector3(gridCoord.x + 0.5f, 0, gridCoord.y + +0.5f); // Ignore the y-axis.
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
            var prefab = gridCellSaveData.trackScriptableObject?.TrackPrefab ??
                         gridCellSaveData.buildingScriptableObject?.BuildingPrefab;
            if (!prefab) continue;

            var position = new Vector3(gridCellSaveData.position.x, 0, gridCellSaveData.position.y);
            var size = new Vector2(gridCellSaveData.size.x, gridCellSaveData.size.y);
            currentRotation = gridCellSaveData.yRotation;
            PlaceBuilding(position, prefab, size, prefab.transform.position.y);
        }
    }
}