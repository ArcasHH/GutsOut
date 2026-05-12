using UnityEngine;

public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager Instance { get;private set; }

    // private string previousSceneName = ""; // use if need reload scene or more complex

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        EventBus.TriggerMenuOpen(true);
        //GetSceneName();
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        //GetSceneName();
    }

    //public void LoadPreviousScene()
    //{
    //    Debug.Log($"Try to load  - {previousSceneName}.");

    //    if (!string.IsNullOrEmpty(previousSceneName))
    //    {
    //        GameSceneManager.LoadScene(previousSceneName);
    //    }
    //    else
    //    {
    //        Debug.LogWarning("No previous scene recorded.");
    //    }
    //}

    public static void LoadScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
    public static void ReloadCurrentScene()
    {
        var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene.name);
    }
    public static string GetCurrentSceneName()
    {
        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
    }

    public void StartGame()
    {
        //GetSceneName();
        Time.timeScale = 1;
        LoadScene("Game");
        EventBus.TriggerGameOpen();
        EventBus.TriggerMenuOpen(false);
    }

    public void OpenMenu()
    {
        //GetSceneName();
        LoadScene("Menu");
        Time.timeScale = 1;
        EventBus.TriggerMenuOpen(true);
    }

    public void ExitGame()
    {
        Application.Quit();

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    //private void GetSceneName()
    //{
    //     previousSceneName = GameSceneManager.GetCurrentSceneName();
    //}
}
