using UnityEngine;
using StarterAssets;        

public class TrackerManualControl : MonoBehaviour
{
    /* ───────────── Refs ───────────── */
    [Header("Refs")]
    public StarterAssetsInputs input;

    public MovementLock movementLock;

    /* ──────────── Tuning ──────────── */
    [Header("Tuning")]
    public float speed = 2f;

    /* ─────────── Update ─────────── */
    void Update()
    {
        if (!input) return;
        if (movementLock && movementLock.IsLocked) return;   

        float horiz = input.move.x;                         
        if (Mathf.Abs(horiz) < 0.01f) return;               

        Vector3 delta = Vector3.right * horiz * speed * Time.deltaTime;
        transform.position += delta;
    }
}
