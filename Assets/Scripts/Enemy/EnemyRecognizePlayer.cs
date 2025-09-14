using UnityEngine;
using System.Collections;
using Unity.Behavior;
using Unity.AppUI.Core;


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

    private Blackboard _bb;

    //private BlackboardVariable<bool> _canSeeVar;
    //private BlackboardVariable<bool> _inRangeVar;

    // public float radius = 0.5f;

    void Awake()
    {
        _agent = GetComponent<BehaviorGraphAgent>();

        //foreach (BlackboardVariable v in _bb.Variables)
        //{
        //    if (v.Name == "CanSeePlayer")
        //        _canSeeVar = v as BlackboardVariable<bool>;

        //    if (v.Name == "IsInAttackRange")
        //        _inRangeVar = v as BlackboardVariable<bool>;
        //}

        FindPlayer();

        _agent.SetVariableValue("PlayerTransform", _player.gameObject);

    }

    void Update()
    {
        if (_player == null) return;

        Vector3 dir = transform.forward;
        Vector3 eye = transform.position + Vector3.up * eyeHeight;

        RaycastHit hit;
        bool seeingPlayer =
            Physics.BoxCast(eye,                 // start point
                            boxHalfExtents,      // scale
                            dir,                 // direction
                            out hit,
                            transform.rotation,  // box rotate
                            viewDistance)        // distance
            && hit.transform == _player;

        if (seeingPlayer)
            Debug.Log("BoxCast hit " + hit.transform.name);


        float dist = Vector3.Distance(transform.position, _player.position);
        bool inRange = seeingPlayer && dist <= attackDistance;

        _agent.SetVariableValue<bool>("CanSeePlayer", seeingPlayer);
        _agent.SetVariableValue<bool>("IsInAttackRange", inRange);

        Debug.DrawLine(eye, eye + dir * viewDistance,
                       seeingPlayer ? Color.green : Color.red);
    }

    void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        Vector3 eye = transform.position + Vector3.up * eyeHeight;
        Vector3 dir = transform.forward;
        Vector3 end = eye + dir * viewDistance;

        var prevMatrix = UnityEditor.Handles.matrix;

        UnityEditor.Handles.matrix = Matrix4x4.TRS(eye, transform.rotation, Vector3.one);
        UnityEditor.Handles.color = Color.cyan;
        UnityEditor.Handles.DrawWireCube(Vector3.zero, boxHalfExtents * 2f);

        UnityEditor.Handles.matrix = Matrix4x4.TRS(end, transform.rotation, Vector3.one);
        UnityEditor.Handles.DrawWireCube(Vector3.zero, boxHalfExtents * 2f);

        UnityEditor.Handles.matrix = prevMatrix;
        UnityEditor.Handles.DrawLine(eye, end);

        UnityEditor.Handles.matrix = prevMatrix;
#endif
    }


    private void FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            _player = player.transform;
        }


    }
}