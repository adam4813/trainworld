using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(SplineContainer))]
public class TrainTrack : GridBuildable
{
    public class TrackSplinePath
    {
        public SplineContainer SplineContainer;
        public bool IsReversed;
    }
    
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private TrackScriptableObject trackScriptableObject;
    public TrackScriptableObject TrackScriptableObject => trackScriptableObject;

    public override Vector2Int GetSize()
    {
        return new Vector2Int((int)trackScriptableObject.trackSize.x, (int)trackScriptableObject.trackSize.y);
    }

    public TrackSplinePath GetSpline(Vector3 startingPosition)
    {
        var firstKnot = splineContainer.Spline.Knots.First().Position;
        var firstKnotPosition = transform.TransformPoint(firstKnot);
        var firstDistance = Vector3.Distance(startingPosition, firstKnotPosition);

        var lastKnot = splineContainer.Spline.Knots.Last().Position;
        var lastKnotPosition = transform.TransformPoint(lastKnot);
        var lastDistance = Vector3.Distance(startingPosition, lastKnotPosition);

        return new TrackSplinePath
        {
            SplineContainer = splineContainer,
            IsReversed = lastDistance < firstDistance
        };
    }
}