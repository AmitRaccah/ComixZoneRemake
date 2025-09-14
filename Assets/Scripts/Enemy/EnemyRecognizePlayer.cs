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
        FindPlayer();
        if (_player) _agent.SetVariableValue("PlayerTransform", _player.gameObject);
    }

    void Update()
    {
        if (_player == null || !_player.gameObject.activeInHierarchy)
        {
            FindPlayer();
            if (_player) _agent.SetVariableValue("PlayerTransform", _player.gameObject);
            if (_player == null) return;
        }

        Vector3 dir = transform.forward;
        Vector3 eye = transform.position + Vector3.up * eyeHeight;

        RaycastHit hit;
        bool hitSomething = Physics.BoxCast(
            eye,
            boxHalfExtents,
            dir,
            out hit,
            transform.rotation,
            viewDistance
        );

        bool hitIsPlayer = false;
        if (hitSomething)
        {
            // לעתים קוליידר של ילד – נבדוק root או תג Player
            Transform root = hit.rigidbody ? hit.rigidbody.transform : hit.transform.root;
            hitIsPlayer = root == _player || root.CompareTag("Player") || hit.transform.CompareTag("Player");
        }

        bool seeingPlayer = hitSomething && hitIsPlayer;

        float dist = Vector3.Distance(transform.position, _player.position);
        bool inRange = seeingPlayer && dist <= attackDistance;

        _agent.SetVariableValue<bool>("CanSeePlayer", seeingPlayer);
        _agent.SetVariableValue<bool>("IsInAttackRange", inRange);

        Debug.DrawLine(eye, eye + dir * viewDistance, seeingPlayer ? Color.green : Color.red);
    }

    void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        Vector3 eye = transform.position + Vector3.up * eyeHeight;
        var prev = UnityEditor.Handles.matrix;

        UnityEditor.Handles.matrix = Matrix4x4.TRS(eye, transform.rotation, Vector3.one);
        UnityEditor.Handles.color = Color.cyan;
        UnityEditor.Handles.DrawWireCube(Vector3.zero, boxHalfExtents * 2f);

        UnityEditor.Handles.matrix = prev;
        UnityEditor.Handles.DrawLine(eye, eye + transform.forward * viewDistance);
#endif
    }

    private void FindPlayer()
    {
        GameObject go = GameObject.FindGameObjectWithTag("Player");
        _player = go ? go.transform : null;
    }
}
