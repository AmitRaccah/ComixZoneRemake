using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    [System.Serializable]
    public class EnemySpawnSlot
    {
        public string name;
        public GameObject prefab;
        public Transform spawnPoint;
        [Min(1)] public int prewarmCount = 1;
        [Min(0f)] public float respawnDelay = 1f;
        [Tooltip("Index in RoomsDirectorManual to notify when this enemy is defeated.")]
        public int roomIndex = -1;
        [Tooltip("When true the slot stops spawning once its room unlocks.")]
        public bool stopRespawningOnUnlock = true;
    }

    [SerializeField] private EnemySpawnSlot[] slots;

    private class SlotRuntime
    {
        public readonly Queue<EnemyPoolMember> pooled = new Queue<EnemyPoolMember>();
        public EnemyPoolMember activeMember;
        public Coroutine pendingRespawn;
        public readonly EnemySpawnSlot config;

        public SlotRuntime(EnemySpawnSlot c)
        {
            config = c;
        }
    }

    private SlotRuntime[] runtimeSlots;

    void Awake()
    {
        if (slots == null || slots.Length == 0) return;

        runtimeSlots = new SlotRuntime[slots.Length];
        for (int i = 0; i < slots.Length; i++)
        {
            EnemySpawnSlot slot = slots[i];
            if (slot == null || slot.prefab == null) continue;
            SlotRuntime runtime = new SlotRuntime(slot);
            runtimeSlots[i] = runtime;
            PrewarmSlot(i, runtime);
        }
    }

    void Start()
    {
        if (runtimeSlots == null) return;

        for (int i = 0; i < runtimeSlots.Length; i++)
        {
            if (runtimeSlots[i] != null)
                SpawnFromSlot(i);
        }
    }

    private void PrewarmSlot(int index, SlotRuntime runtime)
    {
        int count = Mathf.Max(1, runtime.config.prewarmCount);
        for (int i = 0; i < count; i++)
        {
            EnemyPoolMember member = CreateMember(index, runtime.config.prefab);
            ReturnToPool(runtime, member);
        }
    }

    private EnemyPoolMember CreateMember(int slotIndex, GameObject prefab)
    {
        GameObject go = Instantiate(prefab, transform);
        go.SetActive(false);
        EnemyPoolMember member = go.GetComponent<EnemyPoolMember>();
        if (member == null) member = go.AddComponent<EnemyPoolMember>();
        member.Bind(this, slotIndex);
        return member;
    }

    private EnemyPoolMember GetMember(int slotIndex, SlotRuntime runtime)
    {
        if (runtime.pooled.Count > 0)
            return runtime.pooled.Dequeue();

        return CreateMember(slotIndex, runtime.config.prefab);
    }

    private void ReturnToPool(SlotRuntime runtime, EnemyPoolMember member)
    {
        if (member == null) return;
        GameObject go = member.gameObject;
        if (go != null)
        {
            go.SetActive(false);
            go.transform.SetParent(transform, false);
        }
        runtime.pooled.Enqueue(member);
    }

    private void SpawnFromSlot(int index)
    {
        if (runtimeSlots == null) return;
        if (index < 0 || index >= runtimeSlots.Length) return;
        SlotRuntime runtime = runtimeSlots[index];
        if (runtime == null) return;
        EnemyPoolMember member = GetMember(index, runtime);
        runtime.activeMember = member;

        Transform spawnPoint = runtime.config.spawnPoint != null ? runtime.config.spawnPoint : transform;
        Transform target = member.transform;
        target.position = spawnPoint.position;
        target.rotation = spawnPoint.rotation;

        member.PrepareForSpawn();
        target.gameObject.SetActive(true);
    }

    public void HandleMemberRelease(EnemyPoolMember member, int slotIndex)
    {
        if (runtimeSlots == null || slotIndex < 0 || slotIndex >= runtimeSlots.Length)
        {
            member.gameObject.SetActive(false);
            return;
        }

        SlotRuntime runtime = runtimeSlots[slotIndex];
        if (runtime == null)
        {
            member.gameObject.SetActive(false);
            return;
        }
        if (runtime.activeMember == member)
            runtime.activeMember = null;

        ReturnToPool(runtime, member);

        EnemySpawnSlot config = runtime.config;
        if (config.roomIndex >= 0 && RoomsDirectorManual.Instance != null)
        {
            RoomsDirectorManual.Instance.NotifyEnemyKilled(config.roomIndex);
            if (config.stopRespawningOnUnlock && RoomsDirectorManual.Instance.IsRoomUnlocked(config.roomIndex))
            {
                if (runtime.pendingRespawn != null)
                {
                    StopCoroutine(runtime.pendingRespawn);
                    runtime.pendingRespawn = null;
                }
                return;
            }
        }

        if (runtime.pendingRespawn != null)
        {
            StopCoroutine(runtime.pendingRespawn);
            runtime.pendingRespawn = null;
        }
        runtime.pendingRespawn = StartCoroutine(RespawnRoutine(slotIndex, runtime));
    }

    private IEnumerator RespawnRoutine(int slotIndex, SlotRuntime runtime)
    {
        float delay = Mathf.Max(0f, runtime.config.respawnDelay);
        if (delay > 0f) yield return new WaitForSeconds(delay);

        if (runtime.config.roomIndex >= 0 && runtime.config.stopRespawningOnUnlock && RoomsDirectorManual.Instance != null && RoomsDirectorManual.Instance.IsRoomUnlocked(runtime.config.roomIndex))
        {
            runtime.pendingRespawn = null;
            yield break;
        }

        SpawnFromSlot(slotIndex);
        runtime.pendingRespawn = null;
    }
}