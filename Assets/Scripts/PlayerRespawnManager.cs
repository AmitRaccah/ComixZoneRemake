using UnityEngine;
using System.Collections;

public class PlayerRespawnManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform playerSpawnPoint;
    [SerializeField] private TrackerFollowDeltaX tracker;
    [SerializeField] private Transform trackerSpawnPoint;

    private bool busy;

    private void OnEnable()
    {
        CombatBus.Subscribe<PlayerDownEvent>(OnPlayerDown);
    }

    private void OnDisable()
    {
        CombatBus.Unsubscribe<PlayerDownEvent>(OnPlayerDown);
    }

    private void OnPlayerDown(PlayerDownEvent e)
    {
        if (busy) return;
        busy = true;

        tracker.player = null;
        StartCoroutine(RespawnWhenDestroyed(e.playerId));
    }

    private IEnumerator RespawnWhenDestroyed(int deadPlayerId)
    {
        while (FindPlayerHealthById(deadPlayerId) != null)
        {
            yield return null;
        }

        Vector3 tp = tracker.transform.position;
        tp.x = trackerSpawnPoint.position.x;
        tracker.transform.position = tp;

        GameObject newPlayer = Instantiate(playerPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);

        tracker.player = newPlayer.transform;
        tracker.ResetSync();

        busy = false;
    }

    private Health FindPlayerHealthById(int id)
    {
        Health[] all = FindObjectsOfType<Health>();
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] != null && all[i].EntityId == id)
            {
                return all[i];
            }
        }
        return null;
    }
}