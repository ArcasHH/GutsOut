using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class DayManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject containerPrefab;
    [SerializeField] private Transform containersParent;

    [Header("UI")]
    [SerializeField] private TMP_Text scoreText;

    public int CurrentDay = 1; // need redo. now it is just for set
    public int TotalScore { get; private set; } = 0;

    [SerializeField] private TMP_Text karmaText;

    [Header("KarmaPerDay")]
    [SerializeField] private int rewardForOne = 10;
    [SerializeField] private int rewardForTwo = 30;
    [SerializeField] private int rewardForThree = 50;

    [Header("KnifeCost")]
    public int humanDeleterBaseCost = 5; // just to set cost in this script
    public int humanDeleterCostIncrease = 3;

    [SerializeField] private KnifeController knifeController;

    private bool isBusy = false;

    public static DayManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        UpdateUI();
        SubscribeDependencies();
    }

    private void SubscribeDependencies()
    {
        EventBus.OnDayEnd += EndDay;
    }
    private void UnsubscribeDependencies()
    {
        EventBus.OnDayEnd -= EndDay;
    }

    private void OnEnable() => EventBus.OnInventoryChanged += RequestStatsUpdate;
    private void OnDisable() => EventBus.OnInventoryChanged -= RequestStatsUpdate;

    private void Start() => RequestStatsUpdate();

    public void RequestStatsUpdate()
    {
        if (isBusy) return;
        RequestRewardUpdate();
    }
    public bool RequestStatsReady()
    {
        if (isBusy) return false;
        return RequestRewardUpdate();
    }
    public bool GetStatsUpdate()
    {
        return RequestRewardUpdate();
    }

    private bool RequestRewardUpdate()
    {
        int anyFulfilled = 0;
        var summarizers = containersParent.GetComponentsInChildren<OrganStatsSummarizer>(true);

        foreach (var s in summarizers)
        {
            if (s == null || !s.gameObject.activeInHierarchy) continue;
            if (s.IsCollection) continue;

            s.CalculateStats();
            if (s.IsFulfilled)
            {
                anyFulfilled++;
            }
        }
        int expectedReward = anyFulfilled >= 3 ? rewardForThree:
                            anyFulfilled == 2 ? rewardForTwo:
                            anyFulfilled == 1 ? rewardForOne : 0;

        if (karmaText != null)
        {
            karmaText.text = $"karma gain: {expectedReward}";
        }

        return anyFulfilled > 0;
    }

    public void EndDay()
    {
        if (isBusy) return;
        AudioManager.Instance.PlaySound(AudioManager.SoundType.EndDay);
        isBusy = true;
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
        UpdateUI();

        EventBus.OnInventoryChanged?.Invoke();

        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        EventBus.TriggerInventoryChanged();

        isBusy = false;
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

    public bool HandleKnifeDrop(GameObject dropTarget, DraggableItem humanDeleter) //BAD! not here
    {
        if (isBusy) return false;
        if (dropTarget == null) return false;
        if (dropTarget == null) return false;

        int currentHumanDeleterCost = knifeController.GetCurrKnifeCost();

        if (TotalScore < currentHumanDeleterCost || knifeController.knifeUsedThisDay)
        {
            return false;
        }

        GameObject containerRoot = FindContainerRoot(dropTarget);
        if (containerRoot == null) return false;

        TotalScore -= currentHumanDeleterCost;
        UpdateUI();

        knifeController.knifeUsedThisDay = true;
        knifeController.UpdateKnifeVisibility();

        if (humanDeleter != null)
        {
            Destroy(humanDeleter.gameObject);
            knifeController.SetNull();
        }

        StartCoroutine(ReplaceContainerAsync(containerRoot));
        StartCoroutine(SyncButtonAfterKnife());

        return true;
    }

    private IEnumerator SyncButtonAfterKnife()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        EventBus.TriggerInventoryChanged();
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
        if (scoreText != null) scoreText.text = $"Karma: {TotalScore}";
    }

    private void OnDestroy()
    {
        UnsubscribeDependencies();
    }
}