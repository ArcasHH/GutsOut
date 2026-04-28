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
    [SerializeField] private int humanDeleterBaseCost = 20;
    [SerializeField] private int humanDeleterCostIncrease = 5;
    [SerializeField] private Transform humanDeleterSlot;
    [SerializeField] private GameObject humanDeleterPrefab;
    [SerializeField] private TMP_Text humanDeleterCostText;

    [Header("Спавн дневных контейнеров")]
    [Tooltip("Объект 'GameObjects', внутри которого лежат слоты для ежедневных контейнеров")]
    [SerializeField] private Transform dailySpawnRoot;

    private int currentHumanDeleterCost;
    private GameObject currentHumanDeleterInstance;
    private bool humanDeleterUsedThisDay = false;
    private bool isBusy = false;

    public static DayManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        currentHumanDeleterCost = humanDeleterBaseCost;

        if (endDayButton != null)
            endDayButton.onClick.AddListener(EndDay);

        UpdateUI();
        UpdateHumanDeleterCostUI();

        if (humanDeleterPrefab != null && humanDeleterSlot != null && currentHumanDeleterInstance == null)
            SpawnHumanDeleter();
    }

    private void OnEnable() => EventBus.OnInventoryChanged += RequestButtonUpdate;
    private void OnDisable() => EventBus.OnInventoryChanged -= RequestButtonUpdate;

    private void Start() => RequestButtonUpdate();

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
            if (s.IsCollection) continue;

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
        if (isBusy) return;

        isBusy = true;
        if (endDayButton != null) endDayButton.interactable = false;

        StartCoroutine(ProcessDayCycle());
    }

    private IEnumerator ProcessDayCycle()
    {
        var summarizers = containersParent.GetComponentsInChildren<OrganStatsSummarizer>(true);
        var toReplace = new List<GameObject>();

        foreach (var s in summarizers)
        {
            if (s == null || !s.gameObject.activeInHierarchy) continue;
            if (s.IsCollection) continue;
            s.CalculateStats();
            if (s.IsFulfilled) toReplace.Add(s.gameObject);
        }

        int replacedCount = toReplace.Count;

        var despawnTasks = new List<Coroutine>();
        foreach (var old in toReplace)
            despawnTasks.Add(StartCoroutine(ReplaceContainerAsync(old)));

        foreach (var task in despawnTasks)
            yield return task;

        int dailyReward = replacedCount >= 3 ? rewardForThree :
                          replacedCount == 2 ? rewardForTwo :
                          replacedCount == 1 ? rewardForOne : 0;

        TotalScore += dailyReward;
        CurrentDay++;
        UpdateUI();

        if (humanDeleterUsedThisDay)
        {
            currentHumanDeleterCost += humanDeleterCostIncrease;
            UpdateHumanDeleterCostUI();
            SpawnHumanDeleter();
        }

        humanDeleterUsedThisDay = false;
        UpdateHumanDeleterVisibility();
        EventBus.OnInventoryChanged?.Invoke();

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        ForceUpdateButtonState();

        isBusy = false;
        Debug.Log($"[DayManager] Day {CurrentDay - 1} ended. Replaced: {replacedCount}. Reward: +{dailyReward}. Karma: {TotalScore}");
    }

    private IEnumerator ReplaceContainerAsync(GameObject oldContainer)
    {
        if (oldContainer == null) yield break;

        Transform oldParent = oldContainer.transform.parent;

        Vector3 pos = oldContainer.transform.position;
        Quaternion rot = oldContainer.transform.rotation;
        int index = oldContainer.transform.GetSiblingIndex();

        GameObject newContainer = Instantiate(containerPrefab, pos, rot, oldParent);
        newContainer.transform.SetSiblingIndex(index);

        if (oldContainer.TryGetComponent<ContainerAnimationController>(out var anim))
            yield return StartCoroutine(anim.AnimateOut());

        Destroy(oldContainer);
    }

    public bool HandleHumanDeleterDrop(GameObject dropTarget, DraggableItemController humanDeleter)
    {
        if (isBusy) return false; 
        if (dropTarget == null || TotalScore < currentHumanDeleterCost || humanDeleterUsedThisDay)
        {
            return false;
        }

        GameObject containerRoot = FindContainerRoot(dropTarget);
        if (containerRoot == null) return false;

        TotalScore -= currentHumanDeleterCost;
        UpdateUI();

        humanDeleterUsedThisDay = true;
        UpdateHumanDeleterVisibility();

        if (humanDeleter != null)
        {
            Destroy(humanDeleter.gameObject);
            currentHumanDeleterInstance = null;
        }

        StartCoroutine(ReplaceContainerAsync(containerRoot));
        StartCoroutine(SyncButtonAfterHumanDeleter());

        return true;
    }

    private IEnumerator SyncButtonAfterHumanDeleter()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        ForceUpdateButtonState();
    }

    private void SpawnHumanDeleter()
    {
        if (humanDeleterPrefab == null || humanDeleterSlot == null) return;
        if (currentHumanDeleterInstance != null) Destroy(currentHumanDeleterInstance);

        currentHumanDeleterInstance = Instantiate(humanDeleterPrefab, humanDeleterSlot);
        var rt = currentHumanDeleterInstance.GetComponent<RectTransform>();
        if (rt != null) rt.anchoredPosition = Vector2.zero;

        UpdateHumanDeleterVisibility();
    }

    private void UpdateHumanDeleterVisibility()
    {
        bool show = !humanDeleterUsedThisDay;

        if (currentHumanDeleterInstance != null)
            currentHumanDeleterInstance.SetActive(show);

        if (humanDeleterCostText != null)
            humanDeleterCostText.gameObject.SetActive(show);
    }

    private void UpdateHumanDeleterCostUI()
    {
        if (humanDeleterCostText != null)
            humanDeleterCostText.text = $"Cost: {currentHumanDeleterCost} karma";
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
        if (dayCounterText != null) dayCounterText.text = $"Day: {CurrentDay}";
        if (scoreText != null) scoreText.text = $"Karma: {TotalScore}";
    }
}