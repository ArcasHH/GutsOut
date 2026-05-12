using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject containerPrefab;
    [SerializeField] private Transform containersParent;

    [Header("UI")]
    [SerializeField] private TMP_Text scoreText;

    public int CurrentDay { get; private set; } = 1;
    public int TotalScore { get; private set; } = 0;

    [SerializeField] private TMP_Text karmaText;

    [SerializeField] private GameObject[] HumansContainers;
    public static GameManager Instance { get; private set; }
    public void SetCurrentDay(int day) => CurrentDay = day;


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

    public bool IsHumanAnimation()
    {
        int counter = 0;
        foreach (var humanContainer in HumansContainers)
        {
            counter += humanContainer.transform.childCount;
        }
        return counter > HumansContainers.Length;
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
        int expectedReward = anyFulfilled >= 3 ? Balance.RewardForThree :
                            anyFulfilled == 2 ? Balance.RewardForTwo :
                            anyFulfilled == 1 ? Balance.RewardForOne : 0;

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

        int dailyReward = replacedCount >= 3 ? Balance.RewardForThree :
                          replacedCount == 2 ? Balance.RewardForTwo :
                          replacedCount == 1 ? Balance.RewardForOne : 0;

        TotalScore += dailyReward;
        UpdateUI();

    }

    public bool KillTargetHuman(GameObject dropTarget, int knifeCost)
    {
        GameObject containerRoot = FindContainerRoot(dropTarget);
        if (TotalScore < knifeCost || containerRoot == null)
        {
            return false;
        }
        TotalScore -= knifeCost;
        UpdateUI();
        ReplaceContainer(containerRoot);

        return true;
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
        EventBus.TriggerInventoryChanged();
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