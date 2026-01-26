using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    bool isLoading;

    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        MusicManager.Instance.PlayMenuMusic();
    }

    public void LoadTowerMode()
    {
        MainController.Instance.towerMode = true;
        LoadScene("TowerMode");
    }

    public void LoadSurvivalMode()
    {
        MainController.Instance.survivaMode = true;
        LoadScene("SurvivalMode");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    void LoadScene(string sceneName)
    {
        if (isLoading)
            return;

        StartCoroutine(LoadRoutine(sceneName));
    }

    IEnumerator LoadRoutine(string sceneName)
    {
        isLoading = true;

        // Fade to black
        if (ScreenFader.Instance != null)
            yield return ScreenFader.Instance.FadeOut();

        SceneManager.LoadScene(sceneName);
    }
}