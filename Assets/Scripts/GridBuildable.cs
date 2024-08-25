using UnityEngine;

public abstract class GridBuildable : MonoBehaviour
{
    public Rect Rect;

    public Vector2Int GetPivotPoint()
    {
        return TableGrid.GetRectPivotPoint(Rect, transform.eulerAngles.y);
    }

    public Vector2Int Size => new((int)Rect.size.x, (int)Rect.size.y);

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
        Rect = TableGrid.CreateRotatedRect(transform.position, size, transform.eulerAngles.y);
    }
}