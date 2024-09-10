using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainEngine : MonoBehaviour
{
    private class PathNode
    {
        public Vector3 Position;
    }

    [SerializeField] private float speed = 1;
    [SerializeField] private float engineSize = 1;
    [SerializeField] private TrainEngineScriptableObject trainEngineScriptableObject;
    public TrainEngineScriptableObject TrainEngineScriptableObject => trainEngineScriptableObject;

    private float distanceToTarget;
    private readonly Queue<PathNode> currentPath = new();
    private PathNode currentTarget;
    [SerializeField] private TrainStation dockedStation;
    [SerializeField] private TrainCargo cargo;
    public bool IsMoving => currentPath.Count > 0 || dockedStation != null;

    public float Speed
    {
        get => speed;
        set => speed = value;
    }

    public TrainCargo Cargo => cargo;
    public Direction Direction
    {
        get
        {
            var forward = transform.forward;
            if (Mathf.Abs(forward.x) > Mathf.Abs(forward.z))
            {
                return forward.x > 0 ? Direction.Right : Direction.Left;
            }

            return forward.z > 0 ? Direction.Forward : Direction.Backwards;
        }
    }

    public TrainTrack.Path CurrentPath { get; set; }

    public void SetPath(List<Transform> path)
    {
        foreach (var node in path)
        {
            var position = new Vector3(node.position.x, transform.position.y, node.position.z);
            currentPath.Enqueue(new PathNode
            {
                Position = position,
            });
        }

        UpdateCurrentTarget();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out TrainStation trainStation)) return;

        if (!dockedStation)
        {
            StartCoroutine(DockAtStation(trainStation));
        }
    }

    public Vector3 EngineOffset()
    {
        return transform.forward * engineSize / 4f;
    }

    private void UpdateCurrentTarget()
    {
        if (distanceToTarget > 0) return;

        var targetOffset = EngineOffset();

        if (currentTarget != null)
        {
            transform.position = currentTarget.Position + targetOffset;
        }

        do
        {
            currentTarget = currentPath.Count > 0 ? currentPath.Dequeue() : null;
        } while (currentTarget != null &&
                 Vector3.Dot(transform.forward, (currentTarget.Position + targetOffset) - transform.position) < -.75f);

        if (currentTarget != null)
        {
            transform.LookAt(currentTarget.Position + targetOffset);
            distanceToTarget = Vector3.Distance(transform.position - EngineOffset(), currentTarget.Position);
        }
        else
        {
            distanceToTarget = 0;
        }
    }

    private void Update()
    {
        UpdateCurrentTarget();

        if (currentTarget == null) return;

        var distanceTravelled = Speed * Time.deltaTime;
        distanceToTarget -= distanceTravelled;
        transform.position = Vector3.MoveTowards(transform.position,
            currentTarget.Position + EngineOffset(),
            Mathf.Clamp(distanceTravelled, 0f, distanceToTarget * 2f));
    }

    private void OnDrawGizmos()
    {
        foreach (var path in currentPath)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(path.Position, 0.2f);
        }

        if (currentTarget != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(currentTarget.Position, 0.2f);
        }
        
        if (CurrentPath != null)
        {
            foreach (var path in CurrentPath.nodes)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(path.position, 0.2f);
            }
        }
    }

    public void ClearPath()
    {
        distanceToTarget = 0;
        currentPath.Clear();
        currentTarget = null;
    }

    public TrainCargo UnloadCargo()
    {
        var tempCargo = cargo;
        cargo = null;
        return tempCargo;
    }

    public void LoadCargo(TrainCargo stationCargo)
    {
        cargo = stationCargo;
    }

    private IEnumerator DockAtStation(TrainStation trainStation)
    {
        while (trainStation.DockedTrainEngine && trainStation.DockedTrainEngine != this)
        {
            yield return new WaitForSeconds(1f);
        }

        trainStation.OnTrainDocked(this);
        dockedStation = trainStation;
    }

    public void FinishedDocking(TrainStation trainStation)
    {
        trainStation.OnTrainDeparted(this);
        dockedStation = null;
    }
}