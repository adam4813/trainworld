using UnityEngine;
using UnityEngine.UI;

public class HotBarButton : MonoBehaviour
{
    [SerializeField] private Sprite sprite;
    [SerializeField] private Image image;

    private AudioSource audioSource;

    public BuildingScriptableObject buildingScriptableObject;
    public TrackScriptableObject trackScriptableObject;
    public TrainEngineScriptableObject trainEngineScriptableObject;

    public GameObject BuildingPrefab => buildingScriptableObject?.BuildingPrefab ?? trackScriptableObject?.TrackPrefab;

    public Vector2 BuildingSize =>
        buildingScriptableObject?.BuildingSize ?? trackScriptableObject?.TrackSize ?? new Vector2();

    public TrainEngine EnginePrefab => trainEngineScriptableObject?.EnginePrefab;

    private void Awake()
    {
        image.sprite = sprite;
        audioSource = GetComponent<AudioSource>();
        GetComponent<Button>().onClick.AddListener(() => audioSource.Play());
    }

    public void Select()
    {
        image.color = Color.green;
    }

    public void Deselect()
    {
        image.color = Color.white;
    }
}