using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TableGrid : MonoBehaviour
{
    [Serializable]
    public class GridCell
    {
        public GridBuildable building;
    }

    [SerializeField] private Transform gridLayerContainer;
    [SerializeField] private List<GridCell> gridCells;

    public void ClearGridCell(GridCell gridCell)
    {
        Destroy(gridCell.building.gameObject);
        gridCells.Remove(gridCell);
    }

    public bool GriCellsOccupied(Rect buildingRect)
    {
        return gridCells.Exists(cell =>
            cell.building.Rect.Overlaps(buildingRect) && !cell.building.TryGetComponent<TrainStation>(out _));
    }

    public Vector3 GetPivotPoint(Vector3 position, Vector2 size, float yRotation)
    {
        return GridCoordToGridCoordPos(GridPosToGridCoord(WorldToGridPos(position)) + GetRectOffset(size, yRotation));
    }

    public static Vector2Int GetRectOffset(Vector2 size, float yRotation)
    {
        var isCenteredX = size.x % 2 == 0 && size.x > 1 && yRotation is 90 or 180 or -180 or -90;
        var isCenteredY = size.y % 2 == 0 && size.y > 1 && yRotation is 0 or 90;
        return new Vector2Int(
            !isCenteredX ? Mathf.FloorToInt(size.x / 2f) : 0,
            !isCenteredY ? Mathf.FloorToInt(size.y / 2f) : 0
        );
    }

    // Convert the world position to grid local position., within the grid layer container.
    public Vector3 WorldToGridPos(Vector3 worldPos)
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
    public static Vector3 GridCoordToGridCoordPos(Vector2 gridCoord)
    {
        return new Vector3(gridCoord.x + 0.5f, 0, gridCoord.y + 0.5f); // Ignore the y-axis.
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

    public GridCell FindGridCell(Vector3 worldPosition)
    {
        var gridCoord = GridPosToGridCoord(WorldToGridPos(worldPosition));
        return gridCells.FirstOrDefault(cell => cell.building.Rect.Contains(gridCoord));
    }

    public void AddGridCell(GridCell gridCell)
    {
        gridCells.Add(gridCell);
    }

    public void ClearAllCells()
    {
        foreach (var gridCell in gridCells)
        {
            ClearGridCell(gridCell);
        }

        gridCells.Clear();
    }
}