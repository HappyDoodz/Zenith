using UnityEngine;
using System.Collections;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    public enum MusicState
    {
        None,
        Menu,
        Gameplay,
        GameOver
    }

    [Header("Audio Sources")]
    [SerializeField] AudioSource sourceA;
    [SerializeField] AudioSource sourceB;

    [Header("Menu Music")]
    public AudioClip menuMusic;

    [Header("Game Over Music")]
    public AudioClip gameOverMusic;

    [Header("Gameplay Music Pool")]
    public AudioClip[] gameplayTracks;

    [Header("Volume")]
    [Range(0f, 1f)] public float masterVolume   = 1f;
    [Range(0f, 1f)] public float menuVolume     = 0.7f;
    [Range(0f, 1f)] public float gameplayVolume = 0.55f;
    [Range(0f, 1f)] public float gameOverVolume = 0.7f;

    [Header("Fade Settings")]
    public float fadeDuration = 1.5f;
    public float gameplayTrackDelay = 0.5f;

    MusicState currentState;
    AudioSource activeSource;
    AudioSource inactiveSource;
    Coroutine gameplayRoutine;

    // ================= UNITY =================

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (sourceA == null)
            sourceA = gameObject.AddComponent<AudioSource>();

        if (sourceB == null)
            sourceB = gameObject.AddComponent<AudioSource>();

        sourceA.loop = true;
        sourceB.loop = true;

        sourceA.volume = 0f;
        sourceB.volume = 0f;

        activeSource = sourceA;
        inactiveSource = sourceB;

        currentState = MusicState.None;
    }

    // ================= PUBLIC API =================

    public void PlayMenuMusic()
    {
        SwitchState(
            MusicState.Menu,
            menuMusic,
            true,
            menuVolume
        );
    }

    public void PlayGameOverMusic()
    {
        SwitchState(
            MusicState.GameOver,
            gameOverMusic,
            true,
            gameOverVolume
        );
    }

    public void PlayGameplayMusic()
    {
        if (currentState == MusicState.Gameplay)
            return;

        // FADE OUT CURRENT TRACK FIRST
        StartCoroutine(FadeOutActiveSource());

        currentState = MusicState.Gameplay;

        if (gameplayRoutine != null)
            StopCoroutine(gameplayRoutine);

        gameplayRoutine = StartCoroutine(GameplayMusicLoop());
    }
    // ================= INTERNAL =================

    void SwitchState(
        MusicState newState,
        AudioClip clip,
        bool loop,
        float targetVolume
    )
    {
        if (currentState == newState)
            return;

        currentState = newState;

        if (gameplayRoutine != null)
        {
            StopCoroutine(gameplayRoutine);
            gameplayRoutine = null;
        }

        if (clip != null)
            StartCoroutine(CrossfadeTo(clip, loop, targetVolume));
    }

    IEnumerator GameplayMusicLoop()
    {
        yield return new WaitForSeconds(gameplayTrackDelay);

        while (currentState == MusicState.Gameplay)
        {
            if (!activeSource.isPlaying)
            {
                AudioClip next =
                    gameplayTracks[Random.Range(0, gameplayTracks.Length)];

                yield return CrossfadeTo(
                    next,
                    false,
                    gameplayVolume
                );
            }

            yield return null;
        }
    }

    IEnumerator CrossfadeTo(
        AudioClip clip,
        bool loop,
        float targetVolume
    )
    {
        if (clip == null)
            yield break;

        inactiveSource.clip = clip;
        inactiveSource.loop = loop;
        inactiveSource.volume = 0f;
        inactiveSource.Play();

        float t = 0f;
        float finalVolume = targetVolume * masterVolume;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float blend = t / fadeDuration;

            inactiveSource.volume = Mathf.Lerp(0f, finalVolume, blend);
            activeSource.volume   = Mathf.Lerp(activeSource.volume, 0f, blend);

            yield return null;
        }

        activeSource.Stop();
        inactiveSource.volume = finalVolume;

        SwapSources();
    }

    void SwapSources()
    {
        var temp = activeSource;
        activeSource = inactiveSource;
        inactiveSource = temp;
    }

    // ================= OPTIONAL =================

    /// <summary>
    /// Allows runtime volume adjustment (options menu ready)
    /// </summary>
    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);

        activeSource.volume *= masterVolume;
    }

    IEnumerator FadeOutActiveSource()
    {
        float startVolume = activeSource.volume;
        float t = 0f;
    
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            activeSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }
    
        activeSource.Stop();
        activeSource.volume = 0f;
    }
}
