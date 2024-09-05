using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HotBar : MonoBehaviour
{
    public static event Action<HotBarButton> OnHotBarButtonClicked;

    [SerializeField] private List<HotBarButton> hotBarButtons;
    public HotBarButton SelectedHotBarButton { get; private set; }

    private void Awake()
    {
        // Add child game object with compoent HotBarButton to the hotBarButtons list
        foreach (Transform child in transform)
        {
            if (!child.TryGetComponent<HotBarButton>(out var hotBarButton)) continue;
            hotBarButtons.Add(hotBarButton);
        }
    }

    private void OnEnable()
    {
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
        FireHotBarButtonClicked(SelectedHotBarButton);
    }

    public static void FireHotBarButtonClicked(HotBarButton button)
    {
        OnHotBarButtonClicked?.Invoke(button);
    }
}