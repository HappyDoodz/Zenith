using UnityEngine;

public class PickupSound : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] AudioClip pickupSound;
    [SerializeField] float volume = 1f;
    [SerializeField] float pitchMin = 0.95f;
    [SerializeField] float pitchMax = 1.05f;

    public void PlayPickupSound()
    {
        if (pickupSound == null)
            return;

        // Create a temporary audio object
        GameObject audioObj = new GameObject("PickupSound");
        audioObj.transform.position = transform.position;

        AudioSource source = audioObj.AddComponent<AudioSource>();
        source.clip = pickupSound;
        source.volume = volume;
        source.pitch = Random.Range(pitchMin, pitchMax);
        source.spatialBlend = 0f; // 2D sound
        source.Play();

        Destroy(audioObj, pickupSound.length);
    }
}
