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
        public Color outlineColor;

        [Header("Gameplay")]
        public Color cursedOrganColor;
        public Color badOrganColor;
        public Color ordinaryOrganColor;
        public Color goodOrganColor;
        public Color rareOrganColor;
        public Color legendaryOrganColor;
        public Color epicOrganColor;
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
