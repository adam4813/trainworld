using UnityEngine;
using UnityEngine.UI;

public class HotBarButton : MonoBehaviour
{
    [SerializeField] private Sprite sprite;
    [SerializeField] private Image image;
    [SerializeField] private GridBuildable prefab;
    [SerializeField] private TrainEngine enginePrefab;
    private AudioSource audioSource;
    
    public GridBuildable BuildingPrefab => prefab;
    public TrainEngine EnginePrefab => enginePrefab;

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