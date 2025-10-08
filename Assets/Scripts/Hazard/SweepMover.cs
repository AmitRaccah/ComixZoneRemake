using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyPoolMember))]
[RequireComponent(typeof(Collider))]
public class SweepMover : MonoBehaviour
{
    [SerializeField] private float speed = 8f;
    [SerializeField] private float travelDistance = 20f;
    [SerializeField] private float reachEpsilon = 0.05f;

    private EnemyPoolMember member;
    private Coroutine co;

    void Awake()
    {
        member = GetComponent<EnemyPoolMember>();
    }

    void OnSpawnedFromPool()
    {
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(Run());
    }

    void OnReturnedToPool()
    {
        if (co != null) StopCoroutine(co);
        co = null;
    }

    private IEnumerator Run()
    {
        Vector3 start = transform.position;
        Vector3 dir = transform.forward;
        Vector3 target = start + dir * Mathf.Max(0.01f, travelDistance);
        float s = Mathf.Max(0.01f, speed);

        while (true)
        {
            Vector3 to = target - transform.position;
            float dist = to.magnitude;
            if (dist <= reachEpsilon) break;
            float step = s * Time.deltaTime;
            if (step > dist) step = dist;
            transform.position += to.normalized * step;
            yield return null;
        }

        member.Release();
    }
}
