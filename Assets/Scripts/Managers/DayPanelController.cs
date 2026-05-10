using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DayPanelController : MonoBehaviour
{
    [SerializeField] private TMP_Text dayCounterText;
    [SerializeField] private Button dayButton;
    void Start()
    {
        SubscribeDependencies();

        UpdateDayCounter();
        if (dayButton != null)
            dayButton.onClick.AddListener(ClickDayButton);
    }

    private void SubscribeDependencies()
    {
        EventBus.OnInventoryChanged += RequestStatsUpdate;
    }
    private void UnsubscribeDependencies()
    {
        EventBus.OnInventoryChanged -= RequestStatsUpdate;
    }

    private void UpdateDayCounter()
    {
        if (dayCounterText != null)
        {
            int day = DayManager.Instance.CurrentDay;
            dayCounterText.text = $"Day {day}";
        }
    }

    private void ClickDayButton()
    {
        dayButton.interactable = false;
        EventBus.TriggerDayEnd();
        UpdateDayCounter();
        Invoke(nameof(RequestStatsUpdate), 1f); // timer for request of human ready
    }
    private void RequestStatsUpdate()
    {
        dayButton.interactable = DayManager.Instance.GetStatsUpdate(); // return true if any reward karmma gain > 0, else false
    }

    private void OnDestroy()
    {
        UnsubscribeDependencies();
    }

}
