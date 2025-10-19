using UnityEngine;

[DefaultExecutionOrder(10000)]
[DisallowMultipleComponent]
public class Axis2DConstraint : MonoBehaviour
{
    [SerializeField] float laneZ = 0f;
    [SerializeField] bool zeroZVelocity = true;
    [SerializeField] bool zeroAngularVelocity = true;

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
            if (zeroZVelocity)
            {
                var v = rb.linearVelocity; v.z = 0f; rb.linearVelocity = v;
            }
            if (zeroAngularVelocity) rb.angularVelocity = Vector3.zero;
            var p = rb.position; if (p.z != laneZ) { p.z = laneZ; rb.position = p; }
        }
        else
        {
            var p = transform.position; if (p.z != laneZ) { p.z = laneZ; transform.position = p; }
        }
    }

    void LateUpdate()
    {
        var p = transform.position;
        if (p.z != laneZ) { p.z = laneZ; transform.position = p; }
    }
}
