using System.Collections;
using UnityEngine;

public class PlayerRespawnManager : MonoBehaviour
{
    [Header("Refs")]
    public Transform player;
    public Transform respawnPoint;
    public TrackerFollowDeltaX tracker;
    public Transform trackerRespawnPoint;
    public string respawnRoomId;

    void OnEnable()
    {
        CombatBus.Subscribe<PlayerDownEvent>(OnPlayerDown);
    }

    void OnDisable()
    {
        CombatBus.Unsubscribe<PlayerDownEvent>(OnPlayerDown);
    }

    private void OnPlayerDown(PlayerDownEvent _)
    {
        StartCoroutine(DoRespawn());
    }

    private IEnumerator DoRespawn()
    {
        if (player == null) yield break;
        if (respawnPoint == null) yield break;
        if (tracker == null) yield break;
        if (trackerRespawnPoint == null) yield break;

        Health h = player.GetComponent<Health>();
        float wait = h != null ? h.DeathDelay : 0f;
        if (wait > 0f) yield return new WaitForSeconds(wait);

        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.SetPositionAndRotation(respawnPoint.position, respawnPoint.rotation);

        if (cc != null) cc.enabled = true;

        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Animator a = player.GetComponent<Animator>();
        if (a != null)
        {
            a.Rebind();
            a.Update(0f);
        }

        if (h != null) h.RespawnReset();

        if (!string.IsNullOrEmpty(respawnRoomId))
        {
            tracker.ApplyRoom(respawnRoomId);
        }

        tracker.transform.position = trackerRespawnPoint.position;
        tracker.ResetSync();
    }
}
