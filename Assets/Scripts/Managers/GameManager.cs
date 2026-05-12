using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject containerPrefab;
    [SerializeField] private Transform containersParent;

    [Header("UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text karmaText;
    

    [SerializeField] private GameObject[] HumansContainers;
    public static GameManager Instance { get; private set; }


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
    public bool GetStatsUpdate()
    {
        return RequestRewardUpdate();
    }

    public void EndDay()
    {
        AudioManager.Instance.PlaySound(AudioManager.SoundType.EndDay);
        ProcessDayCycle();
    }
    private void ProcessDayCycle()
    {
        int replacedCount = ProcessAndCountFulfilledContainers(replaceContainers: true);
        DataManager.Instance.AddScore(Balance.GetRewardByKarmaCount(replacedCount));
        UpdateUI();
    }
    private bool RequestRewardUpdate()
    {
        int toReplace = ProcessAndCountFulfilledContainers(replaceContainers: false);

        if (karmaText != null)
        {
            karmaText.text = $"karma gain: {Balance.GetRewardByKarmaCount(toReplace)}";
        }
        return toReplace > 0;
    }
    private int ProcessAndCountFulfilledContainers(bool replaceContainers)
    {
        var summarizers = containersParent.GetComponentsInChildren<OrganStatsSummarizer>(true);
        var fulfilledContainers = new List<GameObject>();

        foreach (var s in summarizers)
        {
            if (s == null || !s.gameObject.activeInHierarchy) continue;
            if (s.IsCollection) continue;

            s.CalculateStats();

            if (s.IsFulfilled)
            {
                fulfilledContainers.Add(s.gameObject);
            }
        }

        if (replaceContainers)
        {
            foreach (var container in fulfilledContainers)
            {
                ReplaceContainer(container);
            }
        }

        return fulfilledContainers.Count;
    }

    public bool KillTargetHuman(GameObject dropTarget, int knifeCost)
    {
        GameObject containerRoot = FindContainerRoot(dropTarget);
        if (DataManager.Instance.totalKarma < knifeCost || containerRoot == null)
        {
            return false;
        }
        DataManager.Instance.AddScore(- knifeCost);
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
        if (scoreText != null) scoreText.text = $"Karma: {DataManager.Instance.totalKarma}";
    }

    private void OnDestroy()
    {
        UnsubscribeDependencies();
    }
}