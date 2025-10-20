using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyShooter : MonoBehaviour
{
    [SerializeField] private KnifeItem knifeConfig;
    [SerializeField] private Transform throwSocket;

    int myId;

    void Awake()
    {
        myId = gameObject.GetInstanceID();
    }

    void OnEnable()
    {
        AttackActivator.TransformsById[myId] = transform;
    }

    void OnDisable()
    {
        if (AttackActivator.TransformsById.TryGetValue(myId, out var t) && t == transform)
            AttackActivator.TransformsById.Remove(myId);
    }

    public void Anim_SpawnKnife()
    {
        if (!knifeConfig || !knifeConfig.Data || string.IsNullOrEmpty(knifeConfig.poolId)) return;
        KnifeFactory.Spawn(gameObject, knifeConfig.poolId, throwSocket, knifeConfig.Data, knifeConfig.speed, knifeConfig.distance, knifeConfig.spinPerSecond);
    }
}
