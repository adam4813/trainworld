using UnityEngine;

[CreateAssetMenu(fileName = "TrainEngine", menuName = "Train/Engine", order = 1)]
public class TrainEngineScriptableObject : ScriptableObject
{
    public TrainEngine enginePrefab;
    public float engineSize;
    public float engineSpeed;
}