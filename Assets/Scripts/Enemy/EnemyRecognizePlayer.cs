using UnityEngine;
using Unity.Behavior;
using System.Collections;

[RequireComponent(typeof(BehaviorGraphAgent))]
[RequireComponent(typeof(EnemyCore))]
public class EnemyRecognizePlayer : MonoBehaviour
{
    public float viewDistance = 5f;

    private BehaviorGraphAgent _agent;
    private Transform _player;

    public float eyeHeight = 1.6f;
    public float attackDistance = 1.2f;
    public Vector3 boxHalfExtents = new Vector3(0.5f, 0.9f, 0.1f);

    private void Awake()
    {
        _agent = GetComponent<BehaviorGraphAgent>();
        FindPlayer();
        if (_player != null)
            _agent.SetVariableValue("PlayerTransform", _player.gameObject);
    }

    private void Update()
    {
        if (_player == null) return;

        Vector3 eye = transform.position + Vector3.up * eyeHeight;

        float signX = Mathf.Sign(_player.position.x - transform.position.x);
        if (signX == 0f) signX = 1f;
        Vector3 dir = new Vector3(signX, 0f, 0f);

        RaycastHit hit;
        bool seeingPlayer =
            Physics.BoxCast(eye, boxHalfExtents, dir, out hit, Quaternion.identity, viewDistance)
            && hit.transform == _player;

        float dx = Mathf.Abs(_player.position.x - transform.position.x);
        bool inRange = seeingPlayer && dx <= attackDistance;

        _agent.SetVariableValue<bool>("CanSeePlayer", seeingPlayer);
        _agent.SetVariableValue<bool>("IsInAttackRange", inRange);
        _agent.SetVariableValue<float>("Distance", dx);

#if UNITY_EDITOR
        Debug.DrawLine(eye, eye + dir * viewDistance, seeingPlayer ? Color.green : Color.red);
#endif
    }

    private void FindPlayer()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) _player = p.transform;
    }
}
