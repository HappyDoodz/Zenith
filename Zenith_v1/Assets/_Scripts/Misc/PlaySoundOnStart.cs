using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayRandomSoundOnStart : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip[] clips;

    [Range(0f, 1f)]
    public float volume = 1f;

    [Header("Pitch Variation")]
    [Range(0.9f, 1.1f)]
    public float pitchMin = 0.95f;

    [Range(0.9f, 1.1f)]
    public float pitchMax = 1.05f;

    void Start()
    {
        if (clips == null || clips.Length == 0)
            return;

        AudioSource source = GetComponent<AudioSource>();

        AudioClip clip =
            clips[Random.Range(0, clips.Length)];

        source.pitch  =
            Random.Range(pitchMin, pitchMax);

        source.volume = volume;

        source.PlayOneShot(clip);
    }
}
