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

    [Header("Токен обновления")]
    [SerializeField] private int tokenBaseCost = 20;
    [SerializeField] private int tokenCostIncrease = 5;
    [SerializeField] private Transform tokenSlot;
    [SerializeField] private GameObject tokenPrefab;
    [SerializeField] private TMP_Text tokenCostText;

    private int currentTokenCost;
    private GameObject currentTokenInstance;
    private bool tokenUsedThisDay = false;

    public static DayManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        currentTokenCost = tokenBaseCost;
        if (endDayButton != null) endDayButton.onClick.AddListener(EndDay);

        UpdateUI();
        UpdateTokenCostUI();

        if (tokenPrefab != null && tokenSlot != null && currentTokenInstance == null)
            SpawnToken();
    }

    private void OnEnable() => EventBus.OnInventoryChanged += CheckButtonState;
    private void OnDisable() => EventBus.OnInventoryChanged -= CheckButtonState;

    private void Start() => CheckButtonState();

    private void CheckButtonState()
    {
        bool anyFulfilled = false;
        var summarizers = FindObjectsOfType<OrganStatsSummarizer>();
        foreach (var s in summarizers)
        {
            if (s == null || !s.gameObject.activeInHierarchy || !s.transform.IsChildOf(containersParent)) continue;
            s.CalculateStats();
            if (s.IsFulfilled) { anyFulfilled = true; break; }
        }
        if (endDayButton != null) endDayButton.interactable = anyFulfilled;
    }

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
        
        foreach (var s in allSummarizers)
            if (s != null && s.gameObject.activeInHierarchy && s.transform.IsChildOf(containersParent)) list.Add(s);
        
        foreach (var summarizer in list)
        {
            summarizer.CalculateStats();
            if (summarizer.IsFulfilled) { ReplaceContainer(summarizer.gameObject); replacedToday++; }
        }

        int dailyReward = replacedToday >= 3 ? rewardForThree : replacedToday == 2 ? rewardForTwo : replacedToday == 1 ? rewardForOne : 0;
        TotalScore += dailyReward;
        CurrentDay++;
        UpdateUI();
        
        EventBus.OnInventoryChanged?.Invoke();
        yield return null;
        CheckButtonState();

        // 🔑 Если токен был использован сегодня: повышаем цену и спавним новый
        if (tokenUsedThisDay)
        {
            currentTokenCost += tokenCostIncrease;
            UpdateTokenCostUI();
            SpawnToken();
        }
        
        // Сбрасываем флаг и обновляем видимость для нового дня
        tokenUsedThisDay = false;
        UpdateTokenVisibility();

        Debug.Log($"[DayManager] День {CurrentDay - 1} завершён. Заменено: {replacedToday}. Награда: +{dailyReward}. Очки: {TotalScore}");
    }

    private void ReplaceContainer(GameObject oldContainer)
    {
        Vector3 pos = oldContainer.transform.position;
        Quaternion rot = oldContainer.transform.rotation;
        int index = oldContainer.transform.GetSiblingIndex();

        GameObject newContainer = Instantiate(containerPrefab, pos, rot, containersParent);
        newContainer.transform.SetSiblingIndex(index);

        var anim = oldContainer.GetComponent<ContainerAnimationController>();
        if (anim != null) StartCoroutine(DespawnSequence(oldContainer));
        else Destroy(oldContainer);
    }

    private IEnumerator DespawnSequence(GameObject container)
    {
        if (container == null) yield break;
        var anim = container.GetComponent<ContainerAnimationController>();
        if (anim != null) yield return StartCoroutine(anim.AnimateOut());
        Destroy(container);
    }

    // 🔑 ОБНОВЛЁННАЯ ЛОГИКА ТОКЕНА
    public bool HandleTokenDrop(GameObject dropTarget, DraggableItemController token)
    {
        if (dropTarget == null || TotalScore < currentTokenCost || tokenUsedThisDay)
        {
            Debug.Log(tokenUsedThisDay ? "[Token] Уже использован сегодня!" : "[Token] Недостаточно очков или неверная цель.");
            return false;
        }

        GameObject containerRoot = FindContainerRoot(dropTarget);
        if (containerRoot == null) return false;

        TotalScore -= currentTokenCost;
        UpdateUI();
        
        tokenUsedThisDay = true;
        UpdateTokenVisibility(); // 🔑 Мгновенно скрываем токен и цену

        if (token != null)
        {
            Destroy(token.gameObject);
            currentTokenInstance = null; // Очищаем ссылку, чтобы UpdateVisibility не ломался
        }
        
        ReplaceContainer(containerRoot);
        return true;
    }

    private void SpawnToken()
    {
        if (tokenPrefab == null || tokenSlot == null) return;
        if (currentTokenInstance != null) Destroy(currentTokenInstance);
        
        currentTokenInstance = Instantiate(tokenPrefab, tokenSlot);
        var rt = currentTokenInstance.GetComponent<RectTransform>();
        if (rt != null) rt.anchoredPosition = Vector2.zero;
        
        UpdateTokenVisibility(); // 🔑 Показываем свежезаспавненный токен
    }

    // 🔑 УПРАВЛЕНИЕ ВИДИМОСТЬЮ ТОКЕНА И ЦЕНЫ
    private void UpdateTokenVisibility()
    {
        bool show = !tokenUsedThisDay;
        
        if (currentTokenInstance != null)
            currentTokenInstance.SetActive(show);
            
        if (tokenCostText != null)
            tokenCostText.gameObject.SetActive(show);
    }

    private void UpdateTokenCostUI()
    {
        if (tokenCostText != null) tokenCostText.text = $"💰 {currentTokenCost}";
    }

    private GameObject FindContainerRoot(GameObject obj)
    {
        Transform current = obj.transform;
        while (current != null)
        {
            if (current.GetComponent<OrganStatsSummarizer>() != null || current.GetComponent<ContainerAnimationController>() != null)
                return current.gameObject;
            current = current.parent;
        }
        return null;
    }

    private void UpdateUI()
    {
        if (dayCounterText != null) dayCounterText.text = $"День: {CurrentDay}";
        if (scoreText != null) scoreText.text = $"💰 Очки: {TotalScore}";
    }
}