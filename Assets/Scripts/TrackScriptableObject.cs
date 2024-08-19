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
    [SerializeField] private GameObject trackPrefab;
    [SerializeField] private Vector2 trackSize;
    [SerializeField] private TrackType trackType;
    [SerializeField] private Sprite sprite;
    [SerializeField] private bool forwardConnection;
    [SerializeField] private bool rightConnection;
    [SerializeField] private bool backwardsConnection;
    [SerializeField] private bool leftConnection;

    public GameObject TrackPrefab => trackPrefab;
    public Vector2 TrackSize => trackSize;
    public TrackType TrackType => trackType;
    public Sprite Sprite => sprite;
    
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