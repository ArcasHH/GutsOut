using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(OrganStatsSummarizer))]
public class HumanCollectioned : MonoBehaviour
{
    [SerializeField] private Button scrButton;
    private OrganStatsSummarizer human;
    private Image img;
    void Start()
    {
        scrButton.gameObject.SetActive(false);
        SubscribeDependencies();
        human = GetComponent<OrganStatsSummarizer>();
        img = GetComponent<Image>();
        scrButton.onClick.AddListener(OnButtonClicked);
    }
    private void SubscribeDependencies()
    {
        EventBus.OnCollectionHumanReady += HumanReady;
    }
    private void UnsubscribeDependencies()
    {
        EventBus.OnCollectionHumanReady -= HumanReady;
    }

    private void HumanReady()
    {
        if (human.IsFulfilled)
        {
            scrButton.gameObject.SetActive(true);
        }
    }
    private void OnButtonClicked()
    {
        img.color = ColorPaletteManager.Instance.CurrentPalette.ButtonClickColor;
        scrButton.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        UnsubscribeDependencies();
    }
}
