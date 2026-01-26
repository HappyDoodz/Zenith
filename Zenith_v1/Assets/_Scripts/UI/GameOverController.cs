using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameOverScreenController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] TextMeshProUGUI resultText;
    [SerializeField] TextMeshProUGUI floorsText;
    [SerializeField] TextMeshProUGUI wavesText;
    [SerializeField] TextMeshProUGUI totalKillsText;
    [SerializeField] TextMeshProUGUI basicKillsText;
    [SerializeField] TextMeshProUGUI eliteKillsText;
    [SerializeField] TextMeshProUGUI bossKillsText;
    [SerializeField] TextMeshProUGUI continueHintText;

    [Header("Settings")]
    public float inputLockTime = 2.5f;
    public int victoryFloor = 20;

    bool canContinue;
    bool transitioning;

    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        MusicManager.Instance.PlayMenuMusic();
        PopulateStats();
        StartCoroutine(InputLockRoutine());
    }

    void PopulateStats()
    {
        MusicManager.Instance.PlayMenuMusic();
        
        if (MainController.Instance == null)
            return;

        int floor = MainController.Instance.currentFloor;

        // Victory / Defeat
        if (floor >= victoryFloor)
            resultText.text = "VICTORY";
        else
            resultText.text = "DEFEAT";

        if (MainController.Instance.towerMode) { floorsText.text = $"Floors {MainController.Instance.currentFloor.ToString()}"; }
        else { floorsText.text = ""; }
        if (MainController.Instance.survivaMode) { wavesText.text = $"Waves {MainController.Instance.currentWaves.ToString()}"; }
        else { wavesText.text = ""; }
        totalKillsText.text  = $"Kills {MainController.Instance.currentKills.ToString()}";
        basicKillsText.text  = $"Basic {MainController.Instance.basicKills.ToString()}";
        eliteKillsText.text  = $"Elites {MainController.Instance.eliteKills.ToString()}";
        bossKillsText.text   = $"Bosses {MainController.Instance.bossKills.ToString()}";

        if (continueHintText != null)
            continueHintText.gameObject.SetActive(false);
    }

    IEnumerator InputLockRoutine()
    {
        canContinue = false;

        yield return new WaitForSeconds(inputLockTime);

        canContinue = true;

        if (continueHintText != null)
            continueHintText.gameObject.SetActive(true);
    }

    void Update()
    {
        if (!canContinue || transitioning)
            return;

        // Any key / mouse / controller button
        if (Input.anyKeyDown)
        {
            StartCoroutine(ReturnToMenu());
        }
    }

    IEnumerator ReturnToMenu()
    {
        transitioning = true;
        MainController.Instance.ResetRun();

        if (ScreenFader.Instance != null)
            yield return ScreenFader.Instance.FadeOut();

        string menuScene =
            !string.IsNullOrEmpty(MainController.Instance.mainMenuSceneName)
                ? MainController.Instance.mainMenuSceneName
                : "MainMenu";

        SceneManager.LoadScene(menuScene);
    }
}
