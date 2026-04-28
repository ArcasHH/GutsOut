using UnityEngine;
using UnityEngine.UI;

public class UIScreenManager : MonoBehaviour
{
    [Header("Панели")]
    [Tooltip("Основная панель с контейнерами дня, кнопкой завершения и т.д.")]
    [SerializeField] private GameObject mainGamePanel;
    [Tooltip("Панель с коллекционными контейнерами")]
    [SerializeField] private GameObject collectionPanel;

    [Header("Кнопка переключения")]
    [SerializeField] private Button toggleButton;
    [SerializeField] private string textMain = "Collection";
    [SerializeField] private string textCollection = "Return";

    private bool isCollectionOpen = false;

    private void Awake()
    {
        if (toggleButton != null)
        {
            var btnText = toggleButton.GetComponentInChildren<UnityEngine.UI.Text>();
            if (btnText != null) btnText.text = textMain;
            toggleButton.onClick.AddListener(ToggleScreens);
        }

        if (mainGamePanel != null) mainGamePanel.SetActive(true);
        if (collectionPanel != null) collectionPanel.SetActive(false);
    }

    private void ToggleScreens()
    {
        isCollectionOpen = !isCollectionOpen;

        if (mainGamePanel != null) mainGamePanel.SetActive(!isCollectionOpen);
        if (collectionPanel != null) collectionPanel.SetActive(isCollectionOpen);

        if (toggleButton != null)
        {
            var btnText = toggleButton.GetComponentInChildren<UnityEngine.UI.Text>();
            if (btnText != null) btnText.text = isCollectionOpen ? textCollection : textMain;
        }
    }
}