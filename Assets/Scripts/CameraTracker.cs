using UnityEngine;
using StarterAssets;

public class TrackerManualControl : MonoBehaviour
{
    [Header("Refs")]
    public StarterAssetsInputs input;
    public MovementLock movementLock;

    [Header("Tuning")]
    public float speed = 2f;

    [Header("Limits")]
    public float minX = 0f;  
    public float maxX = 100f;

    void Update()
    {
        if (!input) return;
        if (movementLock && movementLock.IsLocked) return;

        float horiz = input.move.x;
        if (Mathf.Abs(horiz) < 0.01f) return;

        float nextX = transform.position.x + horiz * speed * Time.deltaTime;

        if (nextX < minX)
            nextX = minX;

        if (nextX > maxX)
            nextX = maxX;

        transform.position = new Vector3(nextX, transform.position.y, transform.position.z);
    }
}
