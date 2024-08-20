using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrainTrack : MonoBehaviour
{
    [Serializable]
    public class Path
    {
        public List<Transform> nodes;
    }

    [SerializeField] private TrackScriptableObject trackScriptableObject;
    [SerializeField] private List<Path> paths;
    [SerializeField] private float compareDistance = 1f;
    public Rect rect;

    public TrackScriptableObject TrackScriptableObject => trackScriptableObject;

    public List<Transform> GetClosestPath(Vector3 position)
    {
        /*var path = paths.FirstOrDefault(path =>
            path.nodes.FirstOrDefault(node =>
                Vector3.Distance(new Vector3(node.position.x, 0, node.position.z),
                    new Vector3(position.x, 0, position.z)) < compareDistance)
        );
        return path?.nodes ?? new List<Transform>();*/
        return paths.First().nodes;
    }

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
        foreach (var path in paths)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(path.nodes.First().position, 0.1f);
            // render other nodes except first and last
            for (var i = 1; i < path.nodes.Count - 1; i++)
            {
                Gizmos.color = new Color(Color.yellow.r, Color.yellow.g , Color.yellow.b, .75f);
                Gizmos.DrawSphere(path.nodes[i].position, 0.1f);
            }
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(path.nodes.Last().position, 0.1f);
        }
    }
}