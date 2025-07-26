using UnityEngine;
using System.Collections;


[RequireComponent(typeof(SphereCollider))]

public class EnemyRecognizePlayer : MonoBehaviour
{
   private EnemyCore _core;
    private bool _canSeePlayer = false;

    public bool CanSeePlayer
    {
        get { return _canSeePlayer; }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void Awake()
    {
        _core = GetComponent<EnemyCore>();

        SphereCollider col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        if (col.radius == 0)
        {
            col.radius = 2.5f;
        }
    }

    private void OnEnterTrigger(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!_canSeePlayer)
            {
                _canSeePlayer = true;
        //        CombatBus.Publish(new EnemySpottedPlayerEvent(_core.gameObject.GetInstanceID()));
            }
        }
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        RaycastHit hit;

        float distanceToObstacle = 0;

  //      Vector3 e1 = transform.position + _core;

  //     if (Physics.SphereCast())
        {
   //         distanceToObstacle = hit.distance;
        }
        ;
    }
}
