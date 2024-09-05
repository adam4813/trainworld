using UnityEngine;
using UnityEngine.UI;

public class HotBarButton : MonoBehaviour
{
    [SerializeField] private Sprite sprite;
    [SerializeField] private Image image;

    private AudioSource audioSource;

    public TrainEngineScriptableObject trainEngineScriptableObject;

    [SerializeField] private GridBuildable prefab;

    public GridBuildable BuildingPrefab => prefab;

    public Vector2 BuildingSize => prefab?.GetSize() ?? new Vector2();

    public TrainEngine EnginePrefab => trainEngineScriptableObject?.enginePrefab;

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