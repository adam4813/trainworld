using UnityEngine;

[CreateAssetMenu(fileName = "Cargo", menuName = "Cargo")]
public class CargoScriptableObject : ScriptableObject
{
    [SerializeField] private string cargoName;
    [SerializeField] private int cargoValue;

    public string CargoName => cargoName;
    public int CargoValue => cargoValue;
}