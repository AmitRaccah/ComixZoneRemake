using UnityEngine;

using StarterAssets;

public class TrackerManualControl : MonoBehaviour

{

    [Header("Refs")]

    public StarterAssetsInputs input;

    public MovementLock movementLock;

    [Header("Tuning")]

    public float speed = 2f;

    public float runSpeed = 4f;

    [Header("Limits")]

    public float initialMinX = 0f;

    public float initialMaxX = 100f;

    public PanelLimit[] panelLimits;

    // Current limits (updated dynamically)

    private float currentMinX;

    private float currentMaxX;

    void Start()

    {

        currentMinX = initialMinX;

        currentMaxX = initialMaxX;

    }

    void Update()

    {

        if (!input) return;

        if (movementLock && movementLock.IsLocked) return;

        float horiz = input.move.x;

        if (Mathf.Abs(horiz) < 0.01f) return;

        float currentSpeed = input.sprint ? runSpeed : speed;

        float nextX = transform.position.x + horiz * currentSpeed * Time.deltaTime;

        if (nextX < currentMinX)

            nextX = currentMinX;

        if (nextX > currentMaxX)

            nextX = currentMaxX;

        transform.position = new Vector3(nextX, transform.position.y, transform.position.z);

    }

    public void SetLimitsForCollider(Collider triggeredCollider)

    {

        foreach (var limit in panelLimits)

        {

            if (limit.triggerCollider == triggeredCollider)

            {

                currentMinX = limit.minX;

                currentMaxX = limit.maxX;

                return;

            }

        }

        Debug.LogWarning("No matching panel limit found for collider: " + triggeredCollider.name);

    }

}

[System.Serializable]

public struct PanelLimit

{

    public Collider triggerCollider;

    public float minX;

    public float maxX;

}

