using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TableGrid : MonoBehaviour
{
    public event Action<GridCell> OnBuildingPlaced;
    public event Action<GridCell> OnBuildingRemoved;

    [Serializable]
    public class GridCell
    {
        public Rect rect;
        public GameObject building;
    }

    [SerializeField] private Transform gridLayerContainer;
    [SerializeField] private List<GridCell> gridCells;
    [SerializeField] private LayerMask activeLayerMask;
    private float currentRotation;
    private Camera _camera;
    private GameObject buildingPrefab;

    // Start is called before the first frame update
    private void Awake()
    {
        _camera = Camera.main;
    }

    private void Start()
    {
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
    }

    private void OnDisable()
    {
        HotBar.Instance.OnHotBarButtonClicked -= OnHotBarButtonClicked;
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

        if (!selectedHotBarButton.BuildingPrefab) return;

        buildingPrefab = Instantiate(selectedHotBarButton.BuildingPrefab, gridLayerContainer);
        buildingPrefab.transform.position = new Vector3(0, 0, 0);
        buildingPrefab.transform.rotation = Quaternion.Euler(0, currentRotation, 0);
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

        if (!HotBar.Instance.SelectedHotBarButton) return;

        var ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out var hit, Mathf.Infinity, activeLayerMask) || !hit.collider.CompareTag("Grid"))
        {
            if (buildingPrefab && buildingPrefab.gameObject.activeSelf)
            {
                buildingPrefab.gameObject.SetActive(false);
            }

            return;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (HotBar.Instance.SelectedHotBarButton.BuildingPrefab)
            {
                PlaceBuilding(hit.point);
            }
            else
            {
                ClearBuilding(hit.point);
            }
        }

        if (buildingPrefab)
        {
            RenderPrefab(hit.point);
        }
    }

    private void ClearBuilding(Vector3 position)
    {
        var gridCoord = GridPosToGridCoord(WorldToGridPos(position));
        var testRect = new Rect(gridCoord, Vector2.one);
        if (!GriCellsOccupied(testRect)) return;

        var gridCell = gridCells.Find(cell => cell.rect.Overlaps(testRect));
        Destroy(gridCell.building);
        gridCells.Remove(gridCell);
        OnBuildingRemoved?.Invoke(gridCell);
    }

    private void RenderPrefab(Vector3 position)
    {
        if (!buildingPrefab.gameObject.activeSelf)
        {
            buildingPrefab.gameObject.SetActive(true);
        }

        SetBuildingTransform(buildingPrefab, position);
    }

    private void SetBuildingTransform(GameObject building, Vector3 position)
    {
        var gridCoord = GridPosToGridCoord(WorldToGridPos(position));
        var gridPos = GridCoordToGridCoordPos(gridCoord);
        var selectedHotBarButton = HotBar.Instance.SelectedHotBarButton;
        gridPos.y = selectedHotBarButton.BuildingPrefab.transform.position.y;
        building.transform.localPosition = gridPos; // Local position used to render relative to the grid layer container.
        building.transform.rotation = Quaternion.Euler(0, currentRotation, 0);
    }

    private bool GriCellsOccupied(Rect buildingRect)
    {
        return gridCells.Exists(cell => cell.rect.Overlaps(buildingRect));
    }

    private void PlaceBuilding(Vector3 position)
    {
        var selectedHotBarButton = HotBar.Instance.SelectedHotBarButton;

        var buildingRect = new Rect
        {
            position = GridPosToGridCoord(WorldToGridPos(position)),
            size = selectedHotBarButton.BuildingSize
        };
        if (GriCellsOccupied(buildingRect)) return;

        var instance = Instantiate(selectedHotBarButton.BuildingPrefab, gridLayerContainer);
        var gridCell = new GridCell
        {
            building = instance,
            rect = buildingRect,
        };
        gridCells.Add(gridCell);
        SetBuildingTransform(instance, position);
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
        return new Vector3(gridCoord.x + 0.5f, 0, gridCoord.y + + 0.5f); // Ignore the y-axis.
    }
}