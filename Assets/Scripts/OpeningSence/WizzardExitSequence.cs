using UnityEngine;
using System.Collections;

public class WizzardExitSequence : MonoBehaviour
{
    [Header("Rotation")]
    public float targetYaw = 90f;
    public float rotateSpeed = 180f;

    [Header("First Movement")]
    public float moveSpeed = 2f;
    public float firstMoveDistanceX = 3f;

    [Header("Teleport")]
    public Transform teleportTarget;

    [Header("Second Movement")]
    public float secondMoveDistanceX = 5f;

    enum Phase { Idle, Rotate, Move1, Teleport, Move2, Done }
    Phase phase;
    bool active;

    float laneZ;
    float traveled;

    Animator anim;
    Rigidbody rb;
    CharacterController cc;
    SimpleHover hoverScript; 

    void Awake()
    {
        TryGetComponent(out anim);
        TryGetComponent(out rb);
        TryGetComponent(out cc);
        TryGetComponent(out hoverScript);
    }

    public void Begin()
    {
        if (active) return;

        if (hoverScript != null) hoverScript.enabled = false;
        if (anim != null) anim.enabled = false;
        if (cc != null) cc.enabled = false;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        laneZ = transform.position.z;
        traveled = 0f;
        phase = Phase.Rotate;
        active = true;
    }

    void Update()
    {
        if (!active) return;

        switch (phase)
        {
            case Phase.Rotate:
                DoRotation();
                break;

            case Phase.Move1:
                if (DoMove(firstMoveDistanceX))
                {
                    phase = Phase.Teleport;
                }
                break;

            case Phase.Teleport:
                DoTeleport();
                break;

            case Phase.Move2:
                if (DoMove(secondMoveDistanceX))
                {
                    phase = Phase.Done;
                    gameObject.SetActive(false); 
                }
                break;
        }
    }

    void DoRotation()
    {
        float currentYaw = transform.eulerAngles.y;
        float newYaw = Mathf.MoveTowardsAngle(currentYaw, targetYaw, rotateSpeed * Time.deltaTime);
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, newYaw, transform.eulerAngles.z);

        LockZ();

        if (Mathf.Approximately(newYaw, targetYaw))
        {
            phase = Phase.Move1;
            traveled = 0f;
        }
    }

    void DoTeleport()
    {
        if (teleportTarget != null)
        {
            Vector3 p = teleportTarget.position;
            p.z = laneZ;
            transform.position = p;
        }
        traveled = 0f;
        phase = Phase.Move2;
    }

    void LockZ()
    {
        Vector3 p = transform.position;
        p.z = laneZ;
        transform.position = p;
    }

    bool DoMove(float dist)
    {
        LockZ();

        if (Mathf.Approximately(dist, 0f))
            return true;

        float dir = Mathf.Sign(dist);
        float need = Mathf.Abs(dist);
        float remainingDist = need - traveled;

        float step = Mathf.Min(moveSpeed * Time.deltaTime, remainingDist);

        traveled += step;

        Vector3 p = transform.position;
        p.x += dir * step;
        p.z = laneZ;
        transform.position = p;

        return traveled >= need;
    }
}