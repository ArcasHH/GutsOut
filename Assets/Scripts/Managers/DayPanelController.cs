using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DayPanelController : MonoBehaviour
{
    [SerializeField] private TMP_Text dayCounterText;
    [SerializeField] private Button dayButton;

    private int curr_day;

    void Start()
    {
        SubscribeDependencies();
        
        if (dayButton != null)
            dayButton.onClick.AddListener(ClickDayButton);

        curr_day = 1;
        UpdateDayCounter();
        dayButton.interactable = false;//the first animations will give a signal when
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
        DayManager.Instance.CurrentDay = curr_day;
        if (dayCounterText != null)
        {
            //int curr_day = DayManager.Instance.CurrentDay;
            dayCounterText.text = $"Day {curr_day}";
        }
    }

    private void ClickDayButton()
    {
        if (DayManager.Instance.IsHumanAnimation())
            return;

        curr_day++;

        UpdateDayCounter();
        dayButton.interactable = false;
        EventBus.TriggerDayEnd();
        
        //Invoke(nameof(RequestStatsUpdate), 1f); // timer for request of human ready
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
