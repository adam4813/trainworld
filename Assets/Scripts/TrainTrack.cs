using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrainTrack : GridBuildable
{
    [Serializable]
    public class Path
    {
        public List<Transform> nodes;
        public Vector2Int inputGridCoord;
        public Vector2Int outputGridCoord;
    }

    [SerializeField] private TrackScriptableObject trackScriptableObject;
    [SerializeField] private List<Path> paths;
    [SerializeField] private float compareDistance = 1f;

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

    public override Vector2Int GetSize()
    {
        return new Vector2Int((int)trackScriptableObject.trackSize.x, (int)trackScriptableObject.trackSize.y);
    }

    public Path GetPath(Vector2Int inputGridCoord)
    {
        // TODO: Update when split paths are implemented and output needs to be considered
        return paths.FirstOrDefault(path => Rect.position + path.inputGridCoord == inputGridCoord);
    }

    private Vector2Int LocalGridToWorldGridCoord(Vector2Int localGridCoord)
    {
        var rotatedLocalGridCoord = transform.rotation.eulerAngles.y switch
        {
            // (y, -x)
            90 or -270 => new Vector2Int(localGridCoord.y, 0 - localGridCoord.x),
            // (y,x)
            180 or -180 => new Vector2Int(localGridCoord.y, localGridCoord.x),
            // (-y, x)
            270 or -90 => new Vector2Int(0 - localGridCoord.y, localGridCoord.x),
            _ => localGridCoord
        };

        return Vector2Int.RoundToInt(Rect.position + rotatedLocalGridCoord);
    }

    private void OnDrawGizmos()
    {
        foreach (var path in paths)
        {
            var start = LocalGridToWorldGridCoord(path.inputGridCoord);
            var end = LocalGridToWorldGridCoord(path.outputGridCoord);
            Debug.DrawLine(new Vector3(start.x, transform.position.y + 0.1f, start.y),
                new Vector3(end.x, transform.position.y + 0.1f, end.y), Color.magenta);
        }
    }
}