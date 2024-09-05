using UnityEngine;

public abstract class GridBuildable : MonoBehaviour
{
    public Rect Rect { get; private set; }
    public abstract Vector2Int GetSize();

    public Vector2Int GetPivotPoint()
    {
        return Vector2Int.RoundToInt(Rect.position) + TableGrid.GetRectOffset(Rect.size, transform.eulerAngles.y);
    }

    private void OnDrawGizmos()
    {
        // draw the track rect
        Debug.DrawLine(new Vector3(Rect.xMin, transform.position.y, Rect.yMin),
            new Vector3(Rect.xMax, transform.position.y, Rect.yMin), Color.red);
        Debug.DrawLine(new Vector3(Rect.xMax, transform.position.y, Rect.yMin),
            new Vector3(Rect.xMax, transform.position.y, Rect.yMax), Color.red);
        Debug.DrawLine(new Vector3(Rect.xMax, transform.position.y, Rect.yMax),
            new Vector3(Rect.xMin, transform.position.y, Rect.yMax), Color.red);
        Debug.DrawLine(new Vector3(Rect.xMin, transform.position.y, Rect.yMax),
            new Vector3(Rect.xMin, transform.position.y, Rect.yMin), Color.red);
    }

    [ContextMenu("Update Rect")]
    public void UpdateRect(Vector2 size)
    {
        Rect = GridPlacementSystem.CreateBuildingRect(transform.position, size, transform.eulerAngles.y);
    }
}