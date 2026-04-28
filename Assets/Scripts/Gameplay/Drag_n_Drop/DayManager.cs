using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class DayManager : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private GameObject containerPrefab;
    [SerializeField] private Transform containersParent;

    [Header("UI")]
    [SerializeField] private TMP_Text dayCounterText;
    [SerializeField] private TMP_Text replacedCounterText;
    [SerializeField] private Button endDayButton;

    public int CurrentDay { get; private set; } = 1;
    public int TotalReplacedCount { get; private set; } = 0;

    private void Awake()
    {
        if (endDayButton != null)
            endDayButton.onClick.AddListener(EndDay);
        
        UpdateUI();
    }

    private void OnEnable() => EventBus.OnInventoryChanged += CheckButtonState;
    private void OnDisable() => EventBus.OnInventoryChanged -= CheckButtonState;

    private void Start() => CheckButtonState();

    private void CheckButtonState()
    {
        bool anyFulfilled = false;
        var summarizers = containersParent.GetComponentsInChildren<OrganStatsSummarizer>();
        
        foreach (var s in summarizers)
        {
            if (s == null) continue;
            s.CalculateStats();
            if (s.IsFulfilled)
            {
                anyFulfilled = true;
                break;
            }
        }

        if (endDayButton != null)
            endDayButton.interactable = anyFulfilled;
    }

    public void EndDay()
    {
        if (endDayButton != null && !endDayButton.interactable) return;

        var allSummarizers = containersParent.GetComponentsInChildren<OrganStatsSummarizer>();
        int replacedToday = 0;

        var list = new List<OrganStatsSummarizer>(allSummarizers);
        
        foreach (var summarizer in list)
        {
            if (summarizer == null) continue;
            
            summarizer.CalculateStats();
            if (summarizer.IsFulfilled)
            {
                ReplaceContainer(summarizer.gameObject);
                replacedToday++;
            }
        }

        TotalReplacedCount += replacedToday;
        CurrentDay++;
        
        UpdateUI();
        
        EventBus.OnInventoryChanged?.Invoke();
        CheckButtonState();
        
        Debug.Log($"[DayManager] День {CurrentDay - 1} завершён. Заменено сегодня: {replacedToday}. Всего: {TotalReplacedCount}");
    }

    private void ReplaceContainer(GameObject oldContainer)
    {
        Vector3 pos = oldContainer.transform.position;
        Quaternion rot = oldContainer.transform.rotation;
        int index = oldContainer.transform.GetSiblingIndex();

        GameObject newContainer = Instantiate(containerPrefab, pos, rot, containersParent);
        newContainer.transform.SetSiblingIndex(index);

        var newSummarizer = newContainer.GetComponent<OrganStatsSummarizer>();
        if (newSummarizer != null)
        {
            newSummarizer.RandomRequireStats();
            newSummarizer.CalculateStats();
        }

        Destroy(oldContainer);
    }

    private void UpdateUI()
    {
        if (dayCounterText != null) dayCounterText.text = $"День: {CurrentDay}";
        if (replacedCounterText != null) replacedCounterText.text = $"Всего заменено: {TotalReplacedCount}";
    }
}