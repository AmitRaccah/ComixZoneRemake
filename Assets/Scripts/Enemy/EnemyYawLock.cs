using UnityEngine;
using System.Collections;

[DefaultExecutionOrder(10000)]
[DisallowMultipleComponent]
public class EnemyYawLock : MonoBehaviour
{
    [SerializeField] private float leftYaw = -90f;
    [SerializeField] private float rightYaw = 90f;

    private float targetYaw;
    private Rigidbody rb;
    private readonly WaitForEndOfFrame eof = new WaitForEndOfFrame();

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        float y = transform.eulerAngles.y;
        float d90 = Mathf.Abs(Mathf.DeltaAngle(y, rightYaw));
        float dm90 = Mathf.Abs(Mathf.DeltaAngle(y, leftYaw));
        targetYaw = (d90 <= dm90) ? rightYaw : leftYaw;
    }

    private void OnEnable()
    {
        StartCoroutine(EndOfFrameClamp());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator EndOfFrameClamp()
    {
        while (true)
        {
            yield return eof; 
            ApplyYaw();
        }
    }

    private void OnAnimatorMove()
    {
        ApplyYaw();
    }

    private void LateUpdate()
    {
        ApplyYaw();
    }

    private void FixedUpdate()
    {
        if (rb != null)
        {
            Quaternion q = Quaternion.Euler(0f, targetYaw, 0f);
            rb.MoveRotation(q);
            rb.angularVelocity = Vector3.zero;
        }
    }

    private void ApplyYaw()
    {
        Quaternion q = Quaternion.Euler(0f, targetYaw, 0f);
        transform.rotation = q;
        if (rb != null) rb.angularVelocity = Vector3.zero;
    }
}
