using UnityEngine;
using System.Collections;

public class SceneBootstrap : MonoBehaviour
{
    IEnumerator Start()
    {
        yield return ScreenFader.Instance.FadeIn();
    }
}
