using UnityEngine;

public class Building : MonoBehaviour
{
    [SerializeField] private BuildingScriptableObject buildingScriptableObject;
    public BuildingScriptableObject BuildingScriptableObject => buildingScriptableObject;
    public Rect rect;

    private void OnDrawGizmos()
    {
        // draw the track rect
        Debug.DrawLine(new Vector3(rect.xMin, transform.position.y, rect.yMin),
            new Vector3(rect.xMax, transform.position.y, rect.yMin), Color.red);
        Debug.DrawLine(new Vector3(rect.xMax, transform.position.y, rect.yMin),
            new Vector3(rect.xMax, transform.position.y, rect.yMax), Color.red);
        Debug.DrawLine(new Vector3(rect.xMax, transform.position.y, rect.yMax),
            new Vector3(rect.xMin, transform.position.y, rect.yMax), Color.red);
        Debug.DrawLine(new Vector3(rect.xMin, transform.position.y, rect.yMax),
            new Vector3(rect.xMin, transform.position.y, rect.yMin), Color.red);
    }
}