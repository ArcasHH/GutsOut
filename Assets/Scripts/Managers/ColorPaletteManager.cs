using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ColorPaletteManager : MonoBehaviour
{
    public static ColorPaletteManager Instance { get; private set; }

    [System.Serializable]
    public class ColorPalette
    {
        [Header("UI")]
        public Color buttonHoverColor;
        public Color ButtonClickColor;

        [Header("Gameplay")]
        public Color goodOrganColor;
        public Color ordinaryOrganColor;
        public Color badOrganColor;
    }

    [Header("Color Palettes")]
    [SerializeField] private List<ColorPalette> palettes = new List<ColorPalette>();
    [SerializeField] private int currentPaletteIndex = 0;

    public ColorPalette CurrentPalette =>
        palettes != null && palettes.Count > currentPaletteIndex ?
        palettes[currentPaletteIndex] : null;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
