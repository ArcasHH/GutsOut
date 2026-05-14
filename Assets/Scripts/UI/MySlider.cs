using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class MySlider : MonoBehaviour
{
    private Slider slider;
    [SerializeField] private Image bgImage;

    private void Start()
    {
        slider = GetComponent<Slider>();
        SetupSliderColors();
    }

    private void SetupSliderColors()
    {
        ColorBlock colors = slider.colors;

        colors.normalColor = Color.white;
        colors.highlightedColor = ColorPaletteManager.Instance.CurrentPalette.buttonHoverColor;
        colors.pressedColor = ColorPaletteManager.Instance.CurrentPalette.buttonHoverColor;
        colors.selectedColor = ColorPaletteManager.Instance.CurrentPalette.cursedOrganColor;
        colors.disabledColor = ColorPaletteManager.Instance.CurrentPalette.ButtonClickColor;

        slider.colors = colors;
    }
}
