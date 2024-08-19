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
            currentPath.Enqueue(new PathNode
            {
                Position = new Vector3(node.position.x, transform.position.y, node.position.z),
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
        transform.position += distanceTravelled * transform.forward;
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