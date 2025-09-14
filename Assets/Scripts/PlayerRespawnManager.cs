using System.Collections;
using UnityEngine;
using StarterAssets;

public class PlayerRespawnManager : MonoBehaviour
{
    [Header("Refs")]
    public Transform player;
    public Transform respawnPoint;
    public TrackerFollowDeltaX tracker;
    public Transform trackerRespawnPoint;
    public string respawnRoomId;

    StarterAssetsInputs inputs;
    ThirdPersonController controller;
    MovementLock movementLock;

    void Awake()
    {
        CacheRefs();
    }

    void OnEnable()
    {
        CombatBus.Subscribe<PlayerDownEvent>(OnPlayerDown);
    }

    void OnDisable()
    {
        CombatBus.Unsubscribe<PlayerDownEvent>(OnPlayerDown);
    }

    void CacheRefs()
    {
        if (!player) return;
        inputs = player.GetComponent<StarterAssetsInputs>();
        controller = player.GetComponent<ThirdPersonController>();
        movementLock = player.GetComponent<MovementLock>();
    }

    void OnPlayerDown(PlayerDownEvent _)
    {
        StartCoroutine(DoRespawn());
    }

    IEnumerator DoRespawn()
    {
        if (!player || !respawnPoint || !tracker || !trackerRespawnPoint) yield break;

        if (inputs) inputs.enabled = false;
        if (movementLock) movementLock.SetExternalLock(true);

        var h = player.GetComponent<Health>();
        float wait = (h != null) ? h.DeathDelay : 0f;
        if (wait > 0f) yield return new WaitForSeconds(wait);

        var cc = player.GetComponent<CharacterController>();
        if (cc) cc.enabled = false;
        player.SetPositionAndRotation(respawnPoint.position, respawnPoint.rotation);
        if (cc) cc.enabled = true;

        var rb = player.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (h) h.RespawnReset();

        if (!string.IsNullOrEmpty(respawnRoomId))
            tracker.ApplyRoom(respawnRoomId);

        tracker.transform.position = trackerRespawnPoint.position;
        tracker.ResetSync();

        if (inputs) inputs.enabled = true;
        if (movementLock) movementLock.SetExternalLock(false);
    }
}