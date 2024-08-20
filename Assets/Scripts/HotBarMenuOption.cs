using System;
using UnityEngine;
using UnityEngine.UI;

public enum MenuOptionType
{
    Build,
    Destroy,
    Select
}

public class HotBarMenuOption : MonoBehaviour
{
    public static event Action<HotBarMenuOption> OnHotBarMenuOptionClicked;
    [SerializeField] private BuildingScriptableObject buildingScriptableObject;
    [SerializeField] private TrackScriptableObject trackScriptableObject;
    [SerializeField] private MenuOptionType menuOptionType;
    [SerializeField] private Button menuOptionButton;
    
    public BuildingScriptableObject BuildingScriptableObject => buildingScriptableObject;
    public TrackScriptableObject TrackScriptableObject => trackScriptableObject;
    public MenuOptionType MenuOptionType => menuOptionType;
    
    // Start is called before the first frame update
    void Start()
    {
        //menuOptionButton.onClick.AddListener(() => OnHotBarMenuOptionClicked?.Invoke(this));    
    }
}
