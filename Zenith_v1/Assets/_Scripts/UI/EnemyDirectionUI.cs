using UnityEngine;

public class EnemyDirectionUI : MonoBehaviour
{
    public GameObject leftArrow;
    public GameObject rightArrow;
    public Camera cam;

    void Update()
    {
        bool enemyLeft = false;
        bool enemyRight = false;

        foreach (var enemy in FindObjectsOfType<EnemyHealth>())
        {
            Vector3 viewPos = cam.WorldToViewportPoint(enemy.transform.position);

            if (viewPos.x < 0)
                enemyLeft = true;
            else if (viewPos.x > 1)
                enemyRight = true;
        }

        leftArrow.SetActive(enemyLeft);
        rightArrow.SetActive(enemyRight);
    }
}
