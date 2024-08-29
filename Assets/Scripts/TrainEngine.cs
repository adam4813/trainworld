using System.Collections.Generic;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

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
    public bool IsMoving => currentPath.Count > 0;

    public float Speed
    {
        get => speed;
        set => speed = value;
    }

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
    }

    public void ClearPath()
    {
        distanceToTarget = 0;
        currentPath.Clear();
        currentTarget = null;
    }
}