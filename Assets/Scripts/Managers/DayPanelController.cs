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
        
        if (dayButton != null)
            dayButton.onClick.AddListener(ClickDayButton);

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
        if (dayCounterText != null)
        {
            dayCounterText.text = $"Day {DataManager.Instance.currentDay}";
        }
    }

    private void ClickDayButton()
    {
        if (GameManager.Instance.IsHumanAnimation())
            return;
        EventBus.TriggerDayEnd();
        UpdateDayCounter();
        dayButton.interactable = false;
        
    }
    private void RequestStatsUpdate()
    {
        dayButton.interactable = GameManager.Instance.GetStatsUpdate(); // return true if any reward karmma gain > 0, else false
    }

    private void OnDestroy()
    {
        UnsubscribeDependencies();
    }

}
