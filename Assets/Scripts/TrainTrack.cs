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
}