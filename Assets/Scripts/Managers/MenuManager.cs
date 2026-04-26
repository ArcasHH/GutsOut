
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject gameUI;

    public void MainMenu()
    {
        EventBus.TriggerMenuOpen(true);
        GameSceneManagerWrapper.Instance.OpenMenu();
    }

    //public void OpenMenu()
    //{
    //    menuPanel.SetActive(true);
    //    Time.timeScale = 0;
    //}

    //public void ContinueGame()
    //{
    //    menuPanel.SetActive(false);
    //    Time.timeScale = 1;
    //}

    public void Restart()
    {
        GameSceneManager.ReloadCurrentScene();
    }

    public bool IsActive() // return true if any window is open
    {
        if (menuPanel != null && menuPanel.activeSelf) return true;
        else return false;
    }
    public void Exit()
    {
        GameSceneManagerWrapper.Instance.ExitGame();
    }
}
