using UnityEngine;

public class TrainTrack : GridBuildable
{
    [SerializeField] private TrackScriptableObject trackScriptableObject;
    [SerializeField] private float compareDistance = 1f;

    public TrackScriptableObject TrackScriptableObject => trackScriptableObject;

    public override Vector2Int GetSize()
    {
        return new Vector2Int((int)trackScriptableObject.trackSize.x, (int)trackScriptableObject.trackSize.y);
    }
}