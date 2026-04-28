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
    private bool isBusy = false; // 🔒 Глобальная блокировка во время переходов

    public static DayManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        currentTokenCost = tokenBaseCost;

        if (endDayButton != null)
            endDayButton.onClick.AddListener(EndDay);

        UpdateUI();
        UpdateTokenCostUI();

        if (tokenPrefab != null && tokenSlot != null && currentTokenInstance == null)
            SpawnToken();
    }

    private void OnEnable() => EventBus.OnInventoryChanged += RequestButtonUpdate;
    private void OnDisable() => EventBus.OnInventoryChanged -= RequestButtonUpdate;

    private void Start() => RequestButtonUpdate();

    /// <summary>
    /// 🔑 Безопасный вызов проверки. Игнорируется во время смены дня.
    /// </summary>
    public void RequestButtonUpdate()
    {
        if (isBusy) return;
        ForceUpdateButtonState();
    }

    private void ForceUpdateButtonState()
    {
        bool anyFulfilled = false;
        var summarizers = containersParent.GetComponentsInChildren<OrganStatsSummarizer>(true);

        foreach (var s in summarizers)
        {
            if (s == null || !s.gameObject.activeInHierarchy) continue;

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
        if (isBusy) return; // 🔒 Мгновенная защита от спама

        isBusy = true;
        if (endDayButton != null) endDayButton.interactable = false;

        StartCoroutine(ProcessDayCycle());
    }

    private IEnumerator ProcessDayCycle()
    {
        // 1️⃣ Находим готовые контейнеры
        var summarizers = containersParent.GetComponentsInChildren<OrganStatsSummarizer>(true);
        var toReplace = new List<GameObject>();

        foreach (var s in summarizers)
        {
            if (s == null || !s.gameObject.activeInHierarchy) continue;
            s.CalculateStats();
            if (s.IsFulfilled) toReplace.Add(s.gameObject);
        }

        int replacedCount = toReplace.Count;

        // 2️⃣ Запускаем асинхронную замену для каждого
        var despawnTasks = new List<Coroutine>();
        foreach (var old in toReplace)
            despawnTasks.Add(StartCoroutine(ReplaceContainerAsync(old)));

        // 3️⃣ Ждём завершения всех анимаций исчезновения
        foreach (var task in despawnTasks)
            yield return task;

        // 4️⃣ Начисление наград и смена дня
        int dailyReward = replacedCount >= 3 ? rewardForThree :
                          replacedCount == 2 ? rewardForTwo :
                          replacedCount == 1 ? rewardForOne : 0;

        TotalScore += dailyReward;
        CurrentDay++;
        UpdateUI();

        // 5️⃣ Логика токена
        if (tokenUsedThisDay)
        {
            currentTokenCost += tokenCostIncrease;
            UpdateTokenCostUI();
            SpawnToken();
        }

        tokenUsedThisDay = false;
        UpdateTokenVisibility();
        EventBus.OnInventoryChanged?.Invoke();

        // 6️⃣ 🔑 КРИТИЧЕСКАЯ СИНХРОНИЗАЦИЯ: Ждём 2 кадра, чтобы Unity полностью проинициализировала новые UI-объекты
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        // 7️⃣ Обновляем состояние кнопки на готовых данных
        ForceUpdateButtonState();

        // 8️⃣ Разблокировка
        isBusy = false;
        Debug.Log($"[DayManager] День {CurrentDay - 1} завершён. Заменено: {replacedCount}. Награда: +{dailyReward}. Очки: {TotalScore}");
    }

    private IEnumerator ReplaceContainerAsync(GameObject oldContainer)
    {
        if (oldContainer == null) yield break;

        Vector3 pos = oldContainer.transform.position;
        Quaternion rot = oldContainer.transform.rotation;
        int index = oldContainer.transform.GetSiblingIndex();

        // Создаём новый сразу (он запустит Awake/Start/анимацию появления)
        GameObject newContainer = Instantiate(containerPrefab, pos, rot, containersParent);
        newContainer.transform.SetSiblingIndex(index);

        // Ждём анимацию исчезновения старого
        if (oldContainer.TryGetComponent<ContainerAnimationController>(out var anim))
            yield return StartCoroutine(anim.AnimateOut());

        // Удаляем старый
        Destroy(oldContainer);
    }

    // 🔑 ЛОГИКА ТОКЕНА
    public bool HandleTokenDrop(GameObject dropTarget, DraggableItemController token)
    {
        if (isBusy) return false; // 🔒 Блокируем во время смены дня

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
        UpdateTokenVisibility();

        if (token != null)
        {
            Destroy(token.gameObject);
            currentTokenInstance = null;
        }

        // Запускаем замену и синхронизацию кнопки
        StartCoroutine(ReplaceContainerAsync(containerRoot));
        StartCoroutine(SyncButtonAfterToken());

        return true;
    }

    private IEnumerator SyncButtonAfterToken()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        ForceUpdateButtonState();
    }

    private void SpawnToken()
    {
        if (tokenPrefab == null || tokenSlot == null) return;
        if (currentTokenInstance != null) Destroy(currentTokenInstance);

        currentTokenInstance = Instantiate(tokenPrefab, tokenSlot);
        var rt = currentTokenInstance.GetComponent<RectTransform>();
        if (rt != null) rt.anchoredPosition = Vector2.zero;

        UpdateTokenVisibility();
    }

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
        if (tokenCostText != null)
            tokenCostText.text = $"💰 {currentTokenCost}";
    }

    private GameObject FindContainerRoot(GameObject obj)
    {
        Transform current = obj.transform;
        while (current != null)
        {
            if (current.GetComponent<OrganStatsSummarizer>() != null ||
                current.GetComponent<ContainerAnimationController>() != null)
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