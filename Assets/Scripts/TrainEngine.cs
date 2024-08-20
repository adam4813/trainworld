using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
        path.RemoveAt(0);
        foreach (var node in path)
        {
            var position = new Vector3(node.position.x, transform.position.y, node.position.z);
            if (Vector3.Dot(transform.forward, position) < 0) continue;
            if (Vector3.Distance(transform.position, position) < .1f) continue;
            
            currentPath.Enqueue(new PathNode
            {
                Position = position,
            });
        }
        UpdateCurrentTarget();
    }

    private void UpdateCurrentTarget()
    {
        if (distanceToTarget > 0) return;

        var targetOffset = transform.forward * engineSize / 4f;

        if (currentTarget != null)
        {
            transform.position = currentTarget.Position + targetOffset;
        }

        currentTarget = currentPath.Count > 0 ? currentPath.Dequeue() : null;
        if (currentTarget != null)
        {
            //transform.rotation = Quaternion.Euler(0f, currentTarget.StartingRotationAngle, 0f);
            transform.LookAt(currentTarget.Position + targetOffset);
            var nextTargetOffset = transform.forward * engineSize / 4f;
            distanceToTarget = Vector3.Distance(transform.position - nextTargetOffset, currentTarget.Position);
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
        transform.position += Mathf.Clamp(distanceTravelled, 0f, distanceToTarget) * transform.forward;
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
}