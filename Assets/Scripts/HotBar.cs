using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HotBar : MonoBehaviour
{
    // Add event when hotbar button is clicked
    public event Action<HotBarButton> OnHotBarButtonClicked;
    [SerializeField] private List<HotBarButton> hotBarButtons;
    public HotBarButton SelectedHotBarButton { get; private set; }

    public static HotBar Instance { get; private set; }

    private void Awake()
    {
        if (FindObjectsOfType<HotBar>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // Add child game object with compoent HotBarButton to the hotBarButtons list
        foreach (Transform child in transform)
        {
            if (!child.TryGetComponent<HotBarButton>(out var hotBarButton)) continue;
            hotBarButtons.Add(hotBarButton);
        }

        SetSelectedHotBarButton(hotBarButtons[0]);
    }

    void Start()
    {
        foreach (var button in hotBarButtons)
        {
            button.GetComponent<Button>().onClick.AddListener(() => SetSelectedHotBarButton(button));
        }
    }

    private void SetSelectedHotBarButton(HotBarButton button)
    {
        SelectedHotBarButton?.Deselect();
        SelectedHotBarButton = button;
        SelectedHotBarButton.Select();
        OnHotBarButtonClicked?.Invoke(SelectedHotBarButton);
    }
}