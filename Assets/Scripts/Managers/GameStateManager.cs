using UnityEngine;
using static AudioManager;

public class GameStateManager : MonoBehaviour, IGameState
{
    public static GameStateManager Instance { get; private set; }


    // Game state
    private bool gameActive = false;
    private bool gamePaused = false;
    public bool IsActive => gameActive;
    public bool IsPaused => gamePaused;

    private void Awake()
    {
        InitializeSingleton();
        SubscribeDependencies();
    }
    private void Start()
    {
        EventBus.TriggerGameOpen();
    }

    private void Update()
    {
        HandlePauseInput();
    }

    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void SubscribeDependencies()
    {
        EventBus.OnGameStart += StartGame;
    }
    private void UnsubscribeDependencies()
    {
        EventBus.OnGameStart -= StartGame;
    }

    public void StartGame()
    {       
        if (gameActive) return;
        gameActive = true;
        gamePaused = false; 
    }

    public void EndGame(int stars)
    {
        if (!gameActive)
            return;
        gameActive = false;

        SetPause(true);
        EventBus.TriggerGameEnd();
    }
    public void RestartGame()
    {
        SetPause(false);
        EventBus.TriggerGameOpen();

        GameSceneManager.Instance.StartGame();

    }

    public void SetPause(bool paused)
    {
        if (gamePaused == paused) return;
        if (!gameActive && paused) return;

        gamePaused = paused;
        EventBus.TriggerGamePaused(gamePaused);
    }

    private void HandlePauseInput()
    {
        if (Input.GetKeyUp(KeyCode.Escape) && gameActive)
        {
            SetPause(!gamePaused);
        }
    }

    public void ExitMenu()
    {
        GameSceneManager.Instance.OpenMenu();
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
        UnsubscribeDependencies();
    }
}