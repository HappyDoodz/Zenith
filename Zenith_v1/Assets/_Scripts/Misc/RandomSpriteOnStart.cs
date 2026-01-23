using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class RandomSpriteOnStart : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite[] sprites;

    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        if (sprites == null || sprites.Length == 0)
            return;

        sr.sprite =
            sprites[Random.Range(0, sprites.Length)];
    }
}
