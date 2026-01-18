using UnityEngine;
using System.Collections;

public class AfterImageEffect : MonoBehaviour
{
    [Header("Source")]
    [SerializeField] GameObject spriteSkinRoot;

    [Header("Settings")]
    [SerializeField] float spawnInterval = 0.05f;
    [SerializeField] float lifetime = 0.3f;
    [SerializeField] Color afterImageColor = new Color(1, 1, 1, 0.5f);

    [Header("Mode")]
    [SerializeField] bool alwaysEmitting = false;

    float timer;
    bool burstEmitting;

    void Update()
    {
        if (!alwaysEmitting && !burstEmitting)
            return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            SpawnAfterImage();
            timer = spawnInterval;
        }
    }

    // ================= PUBLIC API =================

    public void EmitForDuration(float duration)
    {
        StartCoroutine(BurstRoutine(duration));
    }

    public void SetAlwaysEmitting(bool enabled)
    {
        alwaysEmitting = enabled;
    }

    public void ToggleAlwaysEmitting()
    {
        alwaysEmitting = !alwaysEmitting;
    }

    // ================= INTERNAL =================

    IEnumerator BurstRoutine(float duration)
    {
        burstEmitting = true;
        timer = 0f;
        yield return new WaitForSeconds(duration);
        burstEmitting = false;
    }

    void SpawnAfterImage()
    {
        if (spriteSkinRoot == null)
            return;

        GameObject ghost = Instantiate(
            spriteSkinRoot,
            spriteSkinRoot.transform.position,
            spriteSkinRoot.transform.rotation,
            null
        );

        // Match rotation exactly (including your Y=180 facing)
        ghost.transform.rotation = spriteSkinRoot.transform.rotation;

        StartCoroutine(FinalizeGhost(ghost));
    }

    IEnumerator FinalizeGhost(GameObject ghost)
    {
        // Wait one frame so the clone evaluates deformation once
        yield return null;

        if (ghost == null)
            yield break;

        // IMPORTANT: do NOT change shader. Keep whatever renders your SpriteSkin correctly.
        SpriteRenderer sourceSR = spriteSkinRoot.GetComponent<SpriteRenderer>();
        SpriteRenderer sr = ghost.GetComponent<SpriteRenderer>();

        if (sr != null && sourceSR != null)
        {
            // Make the ghost independent, but keep the same shader/features as the source
            sr.material = new Material(sourceSR.material);

            // Apply opacity/tint in the most reliable SpriteRenderer way
            sr.color = afterImageColor;

            // Sorting
            sr.sortingLayerID = sourceSR.sortingLayerID;
            sr.sortingOrder = sourceSR.sortingOrder - 1;

            // Keep flips (harmless even if you use Y-rotation)
            sr.flipX = sourceSR.flipX;
            sr.flipY = sourceSR.flipY;
        }

        // Disable any Animator ONLY if it exists on this object (usually it won’t)
        // (If you have one on spriteSkinRoot, it can fight your tint.)
        Animator anim = ghost.GetComponent<Animator>();
        if (anim != null)
            anim.enabled = false;

        Destroy(ghost, lifetime);
    }
}
