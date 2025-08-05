using UnityEngine;
using StarterAssets;        // StarterAssetsInputs

/// מזיז את ה-Tracker על ציר **X** לפי A/D כל עוד השחקן לא נעול
public class TrackerManualControl : MonoBehaviour
{
    /* ───────────── Refs ───────────── */
    [Header("Refs")]
    [Tooltip("קומפוננטת הקלט מהשחקן")]
    public StarterAssetsInputs input;

    [Tooltip("קומפוננטת MovementLock של השחקן (לא חובה)")]
    public MovementLock movementLock;

    /* ──────────── Tuning ──────────── */
    [Header("Tuning")]
    [Tooltip("מהירות (Units/sec) כשהלחיצה מלאה")]
    public float speed = 2f;

    /* ─────────── Update ─────────── */
    void Update()
    {
        if (!input) return;
        if (movementLock && movementLock.IsLocked) return;   // השחקן נעול

        float horiz = input.move.x;                          // A = -1, D = 1
        if (Mathf.Abs(horiz) < 0.01f) return;                // אין קלט

        Vector3 delta = Vector3.right * horiz * speed * Time.deltaTime; // ← ציר X
        transform.position += delta;
    }
}
