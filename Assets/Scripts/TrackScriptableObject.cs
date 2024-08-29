using UnityEngine;

public enum Direction
{
    Forward,
    Right,
    Backwards,
    Left
}
    
public enum TrackType
{
    Straight,
    Curve
}

[CreateAssetMenu(fileName = "Building", menuName = "Building/Track", order = 0)]
public class TrackScriptableObject : ScriptableObject
{
    public TrainTrack trackPrefab;
    public Vector2 trackSize;
    public TrackType trackType;
    public Sprite sprite;
    public bool forwardConnection;
    public bool rightConnection;
    public bool backwardsConnection;
    public bool leftConnection;
    
    public bool HasConnection(Direction direction)
    {
        return direction switch
        {
            Direction.Forward => forwardConnection,
            Direction.Right => rightConnection,
            Direction.Backwards => backwardsConnection,
            Direction.Left => leftConnection,
            _ => false
        };
    }
}