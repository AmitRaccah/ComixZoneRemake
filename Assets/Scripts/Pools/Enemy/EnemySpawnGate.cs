using UnityEngine;
using System.Collections;
using Unity.Behavior;

[DefaultExecutionOrder(-150)]
[DisallowMultipleComponent]
public class EnemySpawnGate : MonoBehaviour
{
    public bool IsSpawning { get; private set; }

    BehaviorGraphAgent ai;
    EnemyShooter shooter;
    SpawnDamageBlocker blocker;
    InkWipeTest ink;
    Coroutine routine;

    void Awake()
    {
        ai = GetComponent<BehaviorGraphAgent>();
        shooter = GetComponent<EnemyShooter>();
        blocker = GetComponent<SpawnDamageBlocker>();
        if (blocker == null) blocker = gameObject.AddComponent<SpawnDamageBlocker>();
        ink = GetComponent<InkWipeTest>();
    }

    void OnDisable()
    {
        if (routine != null) StopCoroutine(routine);
        EndSpawn();
    }

    void OnSpawnedFromPool()
    {
        BeginSpawn();
    }

    void BeginSpawn()
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        IsSpawning = true;
        if (ink != null) ink.StartWipe();
        if (ai != null) ai.enabled = false;
        if (shooter != null) shooter.enabled = false;
        if (blocker != null) blocker.SetActive(true);
        float duration = ink != null ? ink.Duration : 2f;
        yield return new WaitForSeconds(duration);
        EndSpawn();
    }

    void EndSpawn()
    {
        if (!IsSpawning) return;
        IsSpawning = false;
        if (ai != null) ai.enabled = true;
        if (shooter != null) shooter.enabled = true;
        if (blocker != null) blocker.SetActive(false);
    }
}
