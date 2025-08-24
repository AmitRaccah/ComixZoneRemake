using UnityEngine;

public class TrackerFollowDeltaX : MonoBehaviour
{
    public Transform player;

    public float speed = 1f;

    float lastX;
    bool haveLast;

    void OnEnable()
    {
        haveLast = player != null;
        if (haveLast) lastX = player.position.x;
    }

    public void ResetSync()
    {
        if (!player) return;
        lastX = player.position.x;
        haveLast = true;
    }

    void LateUpdate()
    {
        if (!player) return;

        if (!haveLast) { lastX = player.position.x; haveLast = true; return; }

        float dx = player.position.x - lastX;
        lastX = player.position.x;

        if (dx == 0f) return;

        Vector3 p = transform.position;
        p.x += dx * speed;   
        transform.position = p;
    }
}
