using UnityEngine;
using UnityEngine.UI;

public class PopularityLevelIcon : MonoBehaviour
{
    [SerializeField] private Image popularityIconInactive;
    [SerializeField] private Image popularityIconActive;
    [SerializeField] private int minPopularity;
    private void OnEnable()
    {
        PopularityManager.PopularityChanged += OnPopularityChanged;
    }
    
    private void OnDisable()
    {
        PopularityManager.PopularityChanged -= OnPopularityChanged;
    }

    private void OnPopularityChanged(int value)
    {
        if (value < minPopularity) return;
        
        popularityIconActive.enabled = true;
        popularityIconInactive.enabled = false;
    }
}
