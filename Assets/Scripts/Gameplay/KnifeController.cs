using UnityEngine;
using TMPro;

public class KnifeController : MonoBehaviour
{
    private int knifeBaseCost = 5;
    private int knifeCostIncrease = 3;

    [SerializeField] private Transform knifeSlot;
    [SerializeField] private GameObject knifePrefab;

    [SerializeField] private TMP_Text knifeCostText;

    private GameObject currentKnifeInstance;

    private int currentKnifeCost;
    public bool knifeUsedThisDay = false;

    public int GetCurrKnifeCost() => currentKnifeCost;


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

    public void SetNull()
    {
        currentKnifeInstance = null;
    }

    private void OnDestroy()
    {
        UnsubscribeDependencies();
    }
}
