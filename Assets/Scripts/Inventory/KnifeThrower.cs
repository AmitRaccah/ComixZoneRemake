using UnityEngine;

public class KnifeThrower : MonoBehaviour
{
    [SerializeField] private Transform throwSocket;

    void OnEnable()
    {
        CoreBus.Subscribe<KnifeThrownEvent>(OnKnifeThrown);
    }

    void OnDisable()
    {
        CoreBus.Unsubscribe<KnifeThrownEvent>(OnKnifeThrown);
    }

    private void OnKnifeThrown(KnifeThrownEvent e)
    {
        if (string.IsNullOrEmpty(e.knifeId) || e.attackData == null) return;
        KnifeFactory.Spawn(gameObject, e.knifeId, throwSocket, e.attackData, e.speed, e.distance, e.rotationSpeedX, e.rotationSpeedZ);
    }
}
