using UnityEngine;

public class FacePlayer : MonoBehaviour
{
    Transform player;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void LateUpdate()
    {
        if (player == null)
            return;

        bool faceRight = player.position.x > transform.position.x;

        transform.rotation = Quaternion.Euler(
            0f,
            faceRight ? 0f : 180f,
            0f
        );
    }
}
