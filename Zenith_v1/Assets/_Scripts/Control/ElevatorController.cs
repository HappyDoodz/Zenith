using UnityEngine;
using System.Collections;

public class ElevatorController : MonoBehaviour
{
    [SerializeField] Animator animator;

    [Header("Audio")]
    [Tooltip("Sound played when the elevator is triggered")]
    public AudioClip elevatorEnterSound;

    bool active;
    bool triggered;

    public bool IsActive => active;

    private void Start()
    {
        PlayElevatorSound();
    }

    public void SetActive(bool value)
    {
        active = value;
        animator?.SetBool("Active", value);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!active || triggered)
            return;

        if (!other.CompareTag("Player"))
            return;

        triggered = true;

        PlayerController2D controller =
            other.GetComponent<PlayerController2D>();

        controller?.EnterElevator();

        PlayElevatorSound();

        StartCoroutine(TransitionFloor());
    }

    IEnumerator TransitionFloor()
    {
        animator?.SetTrigger("Enter");

        yield return ScreenFader.Instance.FadeOut();

        MainController.Instance.AdvanceFloor();

        if (MainController.Instance.currentFloor >= 20)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                MainController.Instance.gameOverSceneName
            );
        }

        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            );
        }
    }

    // ================= AUDIO =================

    void PlayElevatorSound()
    {
        if (elevatorEnterSound == null)
            return;

        GameObject audioObj = new GameObject("ElevatorEnterSound");
        audioObj.transform.position = transform.position;

        AudioSource src = audioObj.AddComponent<AudioSource>();
        src.clip = elevatorEnterSound;
        src.spatialBlend = 0f; // 2D sound
        src.playOnAwake = false;
        src.Play();

        Destroy(audioObj, elevatorEnterSound.length);
    }
}
