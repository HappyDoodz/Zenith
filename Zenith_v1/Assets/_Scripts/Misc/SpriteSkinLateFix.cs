using UnityEngine;
using UnityEngine.U2D.Animation;

public class SpriteSkinLateFix : MonoBehaviour
{
    SpriteSkin skin;

    void Awake()
    {
        skin = GetComponent<SpriteSkin>();
    }

    void LateUpdate()
    {
        // This forces a mesh rebind without ForceUpdate()
        skin.enabled = false;
        skin.enabled = true;
    }
}
