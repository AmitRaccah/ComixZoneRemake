using UnityEngine;

[DefaultExecutionOrder(10000)]
[DisallowMultipleComponent]
public class Axis2DConstraint : MonoBehaviour
{
    [SerializeField] float laneZ = 0f;
    [SerializeField] float leftYaw = -90f;
    [SerializeField] float rightYaw = 90f;
    [SerializeField] bool snapYawForCharacterController = false;
    [SerializeField] bool snapYawForRigidbody = true;

    Rigidbody rb;
    CharacterController cc;

    void Awake()
    {
        TryGetComponent(out rb);
        TryGetComponent(out cc);
    }

    void FixedUpdate()
    {
        if (rb)
        {
            var v = rb.linearVelocity; v.z = 0f; rb.linearVelocity = v;
            rb.angularVelocity = Vector3.zero;
            var p = rb.position; if (p.z != laneZ) { p.z = laneZ; rb.position = p; }
        }
        else
        {
            var p = transform.position; if (p.z != laneZ) { p.z = laneZ; transform.position = p; }
        }
    }

    void LateUpdate()
    {
        bool doSnap =
            (rb && snapYawForRigidbody) ||
            (cc && snapYawForCharacterController);

        if (doSnap)
        {
            float y = transform.eulerAngles.y;
            float dR = Mathf.Abs(Mathf.DeltaAngle(y, rightYaw));
            float dL = Mathf.Abs(Mathf.DeltaAngle(y, leftYaw));
            transform.rotation = Quaternion.Euler(0f, dR <= dL ? rightYaw : leftYaw, 0f);
        }

        var p = transform.position;
        if (p.z != laneZ) { p.z = laneZ; transform.position = p; }
    }
}
