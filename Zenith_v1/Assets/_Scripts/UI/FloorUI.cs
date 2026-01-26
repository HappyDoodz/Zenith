using UnityEngine;
using TMPro;

public class FloorUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] TextMeshProUGUI floorText;
    [SerializeField] TextMeshProUGUI killsText;
    [SerializeField] GameObject elevatorReadyBox;

    ElevatorController elevator;

    bool playedSound;

    public AudioClip winSound;

    void Start()
    {
        elevator = FindFirstObjectByType<ElevatorController>();

        UpdateFloor();
        UpdateElevatorState();
    }

    void Update()
    {
        UpdateFloor();
        UpdateKills();
        UpdateElevatorState();
    }

    void UpdateFloor()
    {
        if (MainController.Instance == null || floorText == null)
            return;

        if (MainController.Instance.towerMode) { floorText.text = $"FLOOR {MainController.Instance.currentFloor}"; }
        if (MainController.Instance.survivaMode) { floorText.text = $"WAVE {MainController.Instance.currentWaves}"; }
    }

    void UpdateKills()
    {
        if (MainController.Instance == null || killsText == null)
            return;

        killsText.text = $"KILLS {MainController.Instance.currentKills}";
    }

    void UpdateElevatorState()
    {
        if (elevator == null || elevatorReadyBox == null)
            return;

        elevatorReadyBox.SetActive(elevator.IsActive);

        if (!playedSound && elevator.IsActive)
        {
            PlayWinSound();
        }
    }

    void PlayWinSound()
    {
        if (winSound == null)
            return;

        GameObject audioObj = new GameObject("WinSound");
        audioObj.transform.position = transform.position;

        AudioSource src = audioObj.AddComponent<AudioSource>();
        src.clip = winSound;
        src.spatialBlend = 0f; // 2D sound
        src.playOnAwake = false;
        src.Play();
        playedSound = true;

        Destroy(audioObj, winSound.length);
    }
}
