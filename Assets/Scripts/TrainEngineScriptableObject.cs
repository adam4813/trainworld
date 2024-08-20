using UnityEngine;

[CreateAssetMenu(fileName = "TrainEngine", menuName = "Train/Engine", order = 1)]
public class TrainEngineScriptableObject : ScriptableObject
{
    [SerializeField] private TrainEngine enginePrefab;
    [SerializeField] private float engineSize;
    [SerializeField] private float engineSpeed;
    
    public TrainEngine EnginePrefab => enginePrefab;
}