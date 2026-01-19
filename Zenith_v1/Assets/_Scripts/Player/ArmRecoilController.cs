using UnityEngine;
using System.Collections;

public class ArmRecoilController : MonoBehaviour
{
    Vector3 baseLocalPosition;
    Vector3 recoilOffset;

    Coroutine recoilRoutine;

    void Awake()
    {
        baseLocalPosition = transform.localPosition;
    }

    void LateUpdate()
    {
        // Apply recoil AFTER IK has updated
        transform.localPosition = baseLocalPosition + recoilOffset;
    }

    public void ApplyRecoil(
        Vector2 offset,
        float kickTime,
        float returnTime,
        bool facingRight
    )
    {
        if (recoilRoutine != null)
            StopCoroutine(recoilRoutine);

        Vector2 finalOffset = offset;
        if (!facingRight)
            finalOffset.x *= -1f;

        recoilRoutine = StartCoroutine(
            RecoilRoutine(finalOffset, kickTime, returnTime)
        );
    }

    IEnumerator RecoilRoutine(
        Vector2 offset,
        float kickTime,
        float returnTime
    )
    {
        recoilOffset = Vector3.zero;

        float t = 0f;
        while (t < kickTime)
        {
            t += Time.deltaTime;
            recoilOffset =
                Vector3.Lerp(Vector3.zero, offset, t / kickTime);
            yield return null;
        }

        t = 0f;
        while (t < returnTime)
        {
            t += Time.deltaTime;
            recoilOffset =
                Vector3.Lerp(offset, Vector3.zero, t / returnTime);
            yield return null;
        }

        recoilOffset = Vector3.zero;
    }
}
