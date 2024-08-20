using TMPro;
using UnityEngine;

public class PopularityDisplay : MonoBehaviour
{
    [SerializeField] TMP_Text popularityText;

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
        popularityText.text = value.ToString();
    }
}
