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
    public int TotalScore { get; set; } = 0;

    [SerializeField] private TMP_Text karmaText;

    [Header("KarmaPerDay")]
    [SerializeField] private int rewardForOne = 10;
    [SerializeField] private int rewardForTwo = 30;
    [SerializeField] private int rewardForThree = 50;

    [Header("KnifeCost")]
    public int humanDeleterBaseCost = 5; // just to set cost in this script
    public int humanDeleterCostIncrease = 3;

    [SerializeField] private KnifeController knifeController;

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
        RequestRewardUpdate();
    }
    public bool RequestStatsReady()
    {
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
        AudioManager.Instance.PlaySound(AudioManager.SoundType.EndDay);
        ProcessDayCycle();
    }

    private void ProcessDayCycle()
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

        foreach (var old in toReplace)
            ReplaceContainer(old);

        int dailyReward = replacedCount >= 3 ? rewardForThree :
                          replacedCount == 2 ? rewardForTwo :
                          replacedCount == 1 ? rewardForOne : 0;

        TotalScore += dailyReward;
        UpdateUI();

    }

    public void ReplaceContainer(GameObject oldContainer)
    {
        if (oldContainer == null) return;

        Transform oldParent = oldContainer.transform.parent;
        Vector3 pos = oldContainer.transform.position;
        Quaternion rot = oldContainer.transform.rotation;
        int index = oldContainer.transform.GetSiblingIndex();

        GameObject newContainer = Instantiate(containerPrefab, pos, rot, oldParent);
        newContainer.transform.SetSiblingIndex(index);

        if (oldContainer.TryGetComponent<ContainerAnimationController>(out var anim))
        {
            anim.OnAnimationComplete = null;
            anim.OnAnimationComplete += () => Destroy(oldContainer);
            anim.PlayAnimateOut();
        }
        else
        {
            Destroy(oldContainer);
        }
    }

    public void UpdateUI()
    {
        if (scoreText != null) scoreText.text = $"Karma: {TotalScore}";
    }

    private void OnDestroy()
    {
        UnsubscribeDependencies();
    }
}