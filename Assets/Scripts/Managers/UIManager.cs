using UnityEngine;
using UnityEngine.UI;


public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameUI;

    private void Start()
    {
        SubscribeDependencies();
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
