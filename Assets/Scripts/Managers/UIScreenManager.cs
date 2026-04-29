using UnityEngine;
using UnityEngine.UI;

public class UIScreenManager : MonoBehaviour
{
    [Header("Panels")]
    [Tooltip("MainGame")]
    [SerializeField] private GameObject[] panelArray;
    [Tooltip("CollectionPanel")]
    [SerializeField] private GameObject collectionPanel;

    [Header("Collection")]
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

        if (panelArray.Length > 0) SetActivePanelsPanels(true);
        if (collectionPanel != null) collectionPanel.SetActive(false);
    }

    private void ToggleScreens()
    {
        isCollectionOpen = !isCollectionOpen;

        if (panelArray.Length > 0) SetActivePanelsPanels(!isCollectionOpen);
        if (collectionPanel != null) collectionPanel.SetActive(isCollectionOpen);

        if (toggleButton != null)
        {
            var btnText = toggleButton.GetComponentInChildren<UnityEngine.UI.Text>();
            if (btnText != null) btnText.text = isCollectionOpen ? textCollection : textMain;
        }
    }

    private void SetActivePanelsPanels(bool is_active)
    {
        foreach (var panel in panelArray)
        {
            panel.SetActive(is_active);
        }
    }
}