using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemyShooter : MonoBehaviour
{
    [SerializeField] private KnifeItem knifeConfig;  
    [SerializeField] private GameObject knifePrefab;
    [SerializeField] private Transform throwSocket;

    public void Anim_SpawnKnife()
    {
        if (!knifeConfig || !knifeConfig.Data || !knifePrefab) return;

        KnifeFactory.Spawn(gameObject, knifePrefab, throwSocket,
                           knifeConfig.Data, knifeConfig.speed, knifeConfig.distance, knifeConfig.rotationSpeed);
    }
}
