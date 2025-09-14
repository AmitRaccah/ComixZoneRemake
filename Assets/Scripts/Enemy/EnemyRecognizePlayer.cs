using UnityEngine;
using Unity.Behavior;

[RequireComponent(typeof(BehaviorGraphAgent))]
[RequireComponent(typeof(EnemyCore))]
public class EnemyRecognizePlayer : MonoBehaviour
{
    public float viewDistance = 5f;
    public float eyeHeight = 1.6f;
    public float attackDistance = 1.2f;
    public Vector3 boxHalfExtents = new Vector3(0.5f, 0.9f, 0.1f);

    private BehaviorGraphAgent _agent;
    private Transform _player; 

    void Awake()
    {
        _agent = GetComponent<BehaviorGraphAgent>();
        ResolvePlayer();
        if (_player != null)
            _agent.SetVariableValue("PlayerTransform", _player.gameObject);
    }

    void Update()
    {
        if (_player == null || !_player.gameObject.activeInHierarchy)
        {
            ResolvePlayer();
            if (_player == null) return;
            _agent.SetVariableValue("PlayerTransform", _player.gameObject);
        }
        else
        {
            GameObject tagged = GameObject.FindGameObjectWithTag("Player");
            if (tagged && tagged.transform != _player)
            {
                _player = tagged.transform;
                _agent.SetVariableValue("PlayerTransform", _player.gameObject);
            }
        }

        Vector3 eye = transform.position + Vector3.up * eyeHeight;
        Vector3 dir = transform.forward;

        RaycastHit hit;
        bool hitSomething = Physics.BoxCast(
            eye,
            boxHalfExtents,
            dir,
            out hit,
            transform.rotation,
            viewDistance,
            ~0,
            QueryTriggerInteraction.Ignore
        );

        bool seeingPlayer = false;
        if (hitSomething)
        {
            Transform t = hit.transform;
            Transform r = t.root;
            seeingPlayer = (r == _player.root) || t == _player || t.IsChildOf(_player) || t.CompareTag("Player") || r.CompareTag("Player");
        }

        float dist = Vector3.Distance(transform.position, _player.position);
        bool inRange = seeingPlayer && dist <= attackDistance;

        _agent.SetVariableValue("CanSeePlayer", seeingPlayer);
        _agent.SetVariableValue("IsInAttackRange", inRange);

        Debug.DrawLine(eye, eye + dir * viewDistance, seeingPlayer ? Color.green : Color.red);
    }

    private void ResolvePlayer()
    {
        GameObject go = GameObject.FindGameObjectWithTag("Player");
        _player = go ? go.transform : null;
    }
}
