using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class KnifeController : MonoBehaviour
{
    private int knifeBaseCost = 5;
    private int knifeCostIncrease = 3;

    [SerializeField] private Transform knifeSlot;
    [SerializeField] private GameObject knifePrefab;

    [SerializeField] private TMP_Text knifeCostText;

    private GameObject currentKnifeInstance;

    private int currentKnifeCost;
    private bool knifeUsedThisDay = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        knifeBaseCost = DayManager.Instance.humanDeleterBaseCost;
        knifeCostIncrease = DayManager.Instance.humanDeleterCostIncrease;
        currentKnifeCost = knifeBaseCost;
        UpdateKnifeCostUI();

        if (knifePrefab != null && knifeSlot != null && currentKnifeInstance == null)
            SpawnKnife();

        SubscribeDependencies();
    }

    private void SubscribeDependencies()
    {
        EventBus.OnDayEnd += UpdateKnife;
    }
    private void UnsubscribeDependencies()
    {
        EventBus.OnDayEnd -= UpdateKnife;
    }

    private void SpawnKnife()
    {
        if (knifePrefab == null || knifeSlot == null) 
            return;
        if (currentKnifeInstance != null) 
            Destroy(currentKnifeInstance); //why,,,,??????? need to hide intaed destroying

        currentKnifeInstance = Instantiate(knifePrefab, knifeSlot);
        var knifeItem = currentKnifeInstance.GetComponent<KnifeItem>();
        if (knifeItem != null)
        {
            knifeItem.parentController = this;
        }

        var rt = currentKnifeInstance.GetComponent<RectTransform>();
        if (rt != null) rt.anchoredPosition = Vector2.zero;

        UpdateKnifeVisibility();
    }
    private void UpdateKnife()
    {
        if (knifeUsedThisDay)
        {
            currentKnifeCost += knifeCostIncrease;
            UpdateKnifeCostUI();
            SpawnKnife();
        }

        knifeUsedThisDay = false;
        UpdateKnifeVisibility();
    }
    public void UpdateKnifeVisibility()
    {
        bool show = !knifeUsedThisDay;

        if (currentKnifeInstance != null)
            currentKnifeInstance.SetActive(show);

        if (knifeCostText != null)
            knifeCostText.gameObject.SetActive(show);
    }

    private void UpdateKnifeCostUI()
    {
        if (knifeCostText != null)
            knifeCostText.text = $"Cost: {currentKnifeCost} karma";
    }

    public bool CheckKnife()
    {
        if (DayManager.Instance.TotalScore < currentKnifeCost || knifeUsedThisDay)
        {
            return false;
        }
        return true;
    }

    public bool HandleKnifeDrop(GameObject dropTarget, DraggableItem humanDeleter)
    {
        //if (isBusy) return false;
        if (dropTarget == null) return false;
        if (dropTarget == null) return false;

        if (DayManager.Instance.TotalScore < currentKnifeCost || knifeUsedThisDay)
        {
            return false;
        }

        GameObject containerRoot = FindContainerRoot(dropTarget);
        if (containerRoot == null) return false;

        DayManager.Instance.TotalScore -= currentKnifeCost;
        DayManager.Instance.UpdateUI();

       knifeUsedThisDay = true;
        UpdateKnifeVisibility();

        if (humanDeleter != null)
        {
            Destroy(humanDeleter.gameObject);
            currentKnifeInstance = null;
        }

        DayManager.Instance.ReplaceContainer(containerRoot);
        EventBus.TriggerInventoryChanged();

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

    private void OnDestroy()
    {
        UnsubscribeDependencies();
    }
}
