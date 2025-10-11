using UnityEngine;
using Unity.Behavior;

public class EnemyRecognizePlayer : MonoBehaviour
{
    public float viewDistance = 5f;
    public float eyeHeight = 1.6f;
    public float attackDistance = 1.2f;
    public LayerMask targetLayers = ~0;

    private BehaviorGraphAgent _agent;
    private Transform _player;

    void Awake()
    {
        _agent = GetComponent<BehaviorGraphAgent>();
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p) { _player = p.transform; _agent.SetVariableValue("PlayerTransform", p); }
    }

    void Update()
    {
        if (!_player) return;

        Vector3 eye = transform.position + Vector3.up * eyeHeight;
        float signX = Mathf.Sign(_player.position.x - transform.position.x);
        if (signX == 0f) signX = 1f;
        Vector3 dir = new Vector3(signX, 0f, 0f);

        const float skin = 0.1f;
        Vector3 origin = eye - dir * skin;

        bool seeingPlayer = false;
        if (Physics.Raycast(origin, dir, out RaycastHit hit, viewDistance + skin, targetLayers))
            seeingPlayer = (hit.transform && hit.transform.root == _player);

        float dx = seeingPlayer ? Mathf.Abs(_player.position.x - transform.position.x) : 999f;
        bool inRange = seeingPlayer && dx <= attackDistance;

        _agent.SetVariableValue("CanSeePlayer", seeingPlayer);
        _agent.SetVariableValue("IsInAttackRange", inRange);
        _agent.SetVariableValue("Distance", dx);

#if UNITY_EDITOR
        Debug.DrawRay(origin, dir * (viewDistance + skin), seeingPlayer ? Color.green : Color.red);
#endif
    }
}
