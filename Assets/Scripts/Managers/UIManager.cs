using UnityEngine;
using UnityEngine.UI;


public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameUI;

    private void Awake()
    {
        //Perhaps it can be fixed correctly
         // for for dependent objects, Awake() and Start() are called when they become active. to subscribe dependencies and be correctly invoked via System.Action (EventBus) they need to be initialized. 
        SubscribeDependencies();
    }
    private void Start()
    {
        pausePanel.SetActive(false);
        gameUI.SetActive(true);
    }
    private void SubscribeDependencies()
    {
        EventBus.OnGameStart += HandleGameStart;
        EventBus.OnGamePaused += HandleGamePause;
    }
    private void UnsubscribeDependencies()
    {
        EventBus.OnGameStart -= HandleGameStart;
        EventBus.OnGamePaused -= HandleGamePause;
    }

    public void HandleGamePause(bool is_pause)
    {
        pausePanel.SetActive(is_pause);
        gameUI.SetActive(!is_pause);
        Time.timeScale = is_pause ? 0f : 1f;
    }
    public void HandleGameStart()
    {
        HandleGamePause(false);
    }

    private void OnDestroy()
    {
        UnsubscribeDependencies();
    }
} 
