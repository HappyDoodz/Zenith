using UnityEngine;

public class TimedLife : MonoBehaviour
{
    public float MaxLifetime;
    private float startTime;

    private void Start()
    {
        startTime = Time.time;
    }

    private void Update()
    {
        if (Time.time >= startTime + MaxLifetime)
        {
            Destroy(gameObject);
        }
    }
}
