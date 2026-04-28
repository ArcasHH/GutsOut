using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DayManager : MonoBehaviour
{
    [Header("Настройки")]
    [SerializeField] private GameObject containerPrefab;
    [SerializeField] private Transform containersParent;

    [Header("UI")]
    [SerializeField] private TMP_Text dayCounterText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private Button endDayButton;

    public int CurrentDay { get; private set; } = 1;
    public int TotalScore { get; private set; } = 0;

    [Header("Награды за день")]
    [SerializeField] private int rewardForOne = 10;
    [SerializeField] private int rewardForTwo = 30;
    [SerializeField] private int rewardForThree = 50;

    private void Awake()
    {
        if (endDayButton != null)
            endDayButton.onClick.AddListener(EndDay);
        
        UpdateUI();
    }

    private void OnEnable() => EventBus.OnInventoryChanged += CheckButtonState;
    private void OnDisable() => EventBus.OnInventoryChanged -= CheckButtonState;

    private void Start() => CheckButtonState();

    /// <summary>
    /// Проверяет, есть ли хотя бы один готовый контейнер.
    /// Ищет ТОЛЬКО активные, не уничтоженные объекты.
    /// </summary>
    private void CheckButtonState()
    {
        bool anyFulfilled = false;
        
        // Получаем только реально существующие в сцене объекты
        var summarizers = FindObjectsOfType<OrganStatsSummarizer>();
        
        foreach (var s in summarizers)
        {
            // Пропускаем уничтоженные или выключенные объекты
            if (s == null || !s.gameObject.activeInHierarchy) continue;
            
            // Проверяем только те, что лежат в нашем родителе
            if (s.transform.IsChildOf(containersParent))
            {
                s.CalculateStats();
                if (s.IsFulfilled)
                {
                    anyFulfilled = true;
                    break;
                }
            }
        }

        if (endDayButton != null)
            endDayButton.interactable = anyFulfilled;
    }

    /// <summary>
    /// Запускает процесс завершения дня через корутину
    /// </summary>
    public void EndDay()
    {
        if (endDayButton != null && !endDayButton.interactable) return;
        StartCoroutine(ProcessEndDay());
    }

    private IEnumerator ProcessEndDay()
    {
        var allSummarizers = FindObjectsOfType<OrganStatsSummarizer>();
        int replacedToday = 0;
        var list = new List<OrganStatsSummarizer>();
        
        // Собираем только валидные контейнеры
        foreach (var s in allSummarizers)
        {
            if (s != null && s.gameObject.activeInHierarchy && s.transform.IsChildOf(containersParent))
                list.Add(s);
        }
        
        foreach (var summarizer in list)
        {
            summarizer.CalculateStats();
            if (summarizer.IsFulfilled)
            {
                ReplaceContainer(summarizer.gameObject);
                replacedToday++;
            }
        }

        // Расчёт награды
        int dailyReward = replacedToday >= 3 ? rewardForThree : 
                          replacedToday == 2 ? rewardForTwo : 
                          replacedToday == 1 ? rewardForOne : 0;

        TotalScore += dailyReward;
        CurrentDay++;
        UpdateUI();
        
        // Обновляем UI статов
        EventBus.OnInventoryChanged?.Invoke();
        
        // ⏳ ЖДЁМ КОНЦА КАДРА. Unity окончательно удалит старые контейнеры из иерархии
        yield return null;
        
        // Теперь проверяем кнопку только по новым контейнерам
        CheckButtonState();
        
        Debug.Log($"[DayManager] День {CurrentDay - 1} завершён. Заменено: {replacedToday}. Награда: +{dailyReward}. Всего очков: {TotalScore}");
    }

    private void ReplaceContainer(GameObject oldContainer)
    {
        Vector3 pos = oldContainer.transform.position;
        Quaternion rot = oldContainer.transform.rotation;
        int index = oldContainer.transform.GetSiblingIndex();

        // Создаём новый контейнер
        GameObject newContainer = Instantiate(containerPrefab, pos, rot, containersParent);
        newContainer.transform.SetSiblingIndex(index);

        // 🔑 Запускаем анимацию деспавна НА DAY MANAGER, чтобы корутина не убила себя
        StartCoroutine(DespawnSequence(oldContainer));
    }

    private IEnumerator DespawnSequence(GameObject container)
    {
        if (container == null) yield break;
        
        var anim = container.GetComponent<ContainerAnimationController>();
        if (anim != null)
            yield return StartCoroutine(anim.AnimateOut());
            
        Destroy(container);
    }

    private void UpdateUI()
    {
        if (dayCounterText != null) dayCounterText.text = $"День: {CurrentDay}";
        if (scoreText != null) scoreText.text = $"Валюта: {TotalScore}";
    }
}