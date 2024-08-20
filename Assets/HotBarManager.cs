using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HotBarManager : MonoBehaviour
{
    [SerializeField] private List<HotBar> hotBars;
    [SerializeField] private HotBarButton deleteButton;
    private int activeHotBarIndex = -1;

    public static HotBarManager Instance { get; private set; }

    public HotBar ActiveHotBar => activeHotBarIndex == -1 ? null : hotBars[activeHotBarIndex];

    private void Awake()
    {
        if (FindObjectsOfType<HotBar>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        deleteButton.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (activeHotBarIndex == -1) return;
            hotBars[activeHotBarIndex].gameObject.SetActive(false);
            activeHotBarIndex = -1;
            HotBar.FireHotBarButtonClicked(deleteButton);
        });
    }

    private void Start()
    {
        for (var i = 0; i < transform.childCount && i < hotBars.Count; i++)
        {
            var index = i;
            var child = transform.GetChild(i);
            if (child.GetComponent<HotBarButton>()) continue;
            var button = child.GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                if (activeHotBarIndex == index) return;
                if (activeHotBarIndex != -1)
                {
                    hotBars[activeHotBarIndex].gameObject.SetActive(false);
                }

                activeHotBarIndex = index;
                hotBars[activeHotBarIndex].gameObject.SetActive(true);
            });
        }
    }
}