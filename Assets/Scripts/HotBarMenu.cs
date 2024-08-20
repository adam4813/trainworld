using UnityEngine;
using UnityEngine.UI;

public class HotBarMenu : MonoBehaviour
{
    [SerializeField] private GameObject hotBarMenuPanel;
    [SerializeField] private Button hotBarMenuButton;
    // Start is called before the first frame update
    void Start()
    {
        //hotBarMenuButton.onClick.AddListener(() => hotBarMenuPanel.SetActive(!hotBarMenuPanel.activeSelf));
    }

    private void OnEnable()
    {
        HotBarMenuOption.OnHotBarMenuOptionClicked += OnHotBarMenuOptionClicked;
    }
    
    private void OnDisable()
    {
        HotBarMenuOption.OnHotBarMenuOptionClicked -= OnHotBarMenuOptionClicked;
    }

    private void OnHotBarMenuOptionClicked(HotBarMenuOption _)
    {
        hotBarMenuPanel.SetActive(false);
    }
}
