using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(OrganStatsSummarizer))]
public class HumanCollectioned : MonoBehaviour
{
    [SerializeField] private Button scrButton;
    private OrganStatsSummarizer human;
    private Image img;

    [NonSerialized] public bool is_sacrificed = false;
    void Start()
    {
        scrButton.gameObject.SetActive(false);
        SubscribeDependencies();
        human = GetComponent<OrganStatsSummarizer>();
        img = GetComponent<Image>();
        scrButton.onClick.AddListener(OnButtonClicked);
        is_sacrificed = false;
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
        if (human.IsFulfilled && !is_sacrificed)
        {
            scrButton.gameObject.SetActive(true);
        }
    }
    private void OnButtonClicked()
    {
        img.color = ColorPaletteManager.Instance.CurrentPalette.ButtonClickColor;
        scrButton.gameObject.SetActive(false);
        is_sacrificed = true;
        EventBus.TriggerSacrificedButtonPressed();
    }

    private void OnDestroy()
    {
        UnsubscribeDependencies();
    }
}
