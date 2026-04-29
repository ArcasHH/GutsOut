using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

public class UIScreenManager : MonoBehaviour
{
    [Header("Panels")]
    [Tooltip("MainGame")]
    [SerializeField] private GameObject[] panelArray;
    [SerializeField] private GameObject winPanel;
    [Tooltip("CollectionPanel")]
    [SerializeField] private GameObject collectionPanel;
    [SerializeField] private GameObject collectionObjects;

    [Header("Collection")]
    [SerializeField] private Button toggleButton;
    [SerializeField] private string textMain = "Collection";
    [SerializeField] private string textCollection = "Return";

    private bool isCollectionOpen = false;

    private HumanCollectioned[] humanCollectionArray;

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

        humanCollectionArray = collectionObjects.GetComponentsInChildren<HumanCollectioned>();
        HideWinScreen();
        SubscribeDependencies();
    }

    private void SubscribeDependencies()
    {
        EventBus.OnSacrificedButtonPressed += CheckWinCondition;

    }
    private void UnsubscribeDependencies()
    {
        EventBus.OnSacrificedButtonPressed -= CheckWinCondition;

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

    private void CheckWinCondition()
    {
        foreach(HumanCollectioned child in humanCollectionArray)
        {
            if (!child.is_sacrificed)
                return;
        }
        ShowWinScreen();
    }

    private void ShowWinScreen()
    {
        winPanel.SetActive(true);
    }
    private void HideWinScreen()
    {
        winPanel.SetActive(false);
    }

    private void OnDestroy()
    {
        UnsubscribeDependencies();
    }
}