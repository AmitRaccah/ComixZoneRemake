//using UnityEngine;
//using System.Collections;

//public class EnemyFaceOnHit : MonoBehaviour
//{
//    [SerializeField] private float turnDuration = 0.1f;
//    [SerializeField] private float behindDotThreshold = 0.0f;

//    private int myId;
//    private bool turning;

//    void Awake() => myId = gameObject.GetInstanceID();
//    void OnEnable() => CombatBus.Subscribe<DamageEvent>(OnDamage);
//    void OnDisable() => CombatBus.Unsubscribe<DamageEvent>(OnDamage);

//    void OnDamage(DamageEvent e)
//    {
//        if (e.targetId != myId) return;
//        if (!AttackActivator.TransformsById.TryGetValue(e.attackerId, out var atk)) return;

//        Vector3 toAtk = (atk.position - transform.position).normalized;
//        toAtk.y = 0f;

//        if (Vector3.Dot(transform.forward, toAtk) <= behindDotThreshold && !turning)
//            StartCoroutine(TurnTowards(atk.position));
//    }

//    IEnumerator TurnTowards(Vector3 worldPos)
//    {
//        turning = true;
//        Quaternion start = transform.rotation;
//        Vector3 dir = (worldPos - transform.position); dir.y = 0f;
//        if (dir.sqrMagnitude < 1e-4f) { turning = false; yield break; }
//        Quaternion target = Quaternion.LookRotation(dir.normalized, Vector3.up);

//        float t = 0f;
//        while (t < turnDuration)
//        {
//            t += Time.deltaTime;
//            transform.rotation = Quaternion.Slerp(start, target, t / turnDuration);
//            yield return null;
//        }
//        transform.rotation = target;
//        turning = false;
//    }
//}
