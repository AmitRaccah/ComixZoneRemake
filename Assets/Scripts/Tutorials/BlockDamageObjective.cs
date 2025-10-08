using System.Collections.Generic;
using UnityEngine;

public class BlockDamageObjective : TutorialObjective
{
    [SerializeField] private GameObject playerActor;
    [SerializeField] private GameObject[] allowedAttackers;

    private int playerId;
    private readonly List<int> attackerIds = new();

    protected override void OnReset()
    {
        CacheIds();
    }

    protected override void OnBegin()
    {
        CacheIds();
        CombatBus.Subscribe<DamageEvent>(OnDamage);
    }

    protected override void OnEnd()
    {
        CombatBus.Unsubscribe<DamageEvent>(OnDamage);
    }

    private void CacheIds()
    {
        playerId = playerActor ? playerActor.GetInstanceID() : 0;
        attackerIds.Clear();
        if (allowedAttackers == null) return;

        for (int i = 0; i < allowedAttackers.Length; i++)
        {
            var go = allowedAttackers[i];
            if (!go) continue;
            attackerIds.Add(go.GetInstanceID());
        }
    }

    private bool IsValidAttacker(int attackerId)
    {
        if (attackerIds.Count == 0)
            return true;

        for (int i = 0; i < attackerIds.Count; i++)
            if (attackerIds[i] == attackerId)
                return true;

        return false;
    }

    private void OnDamage(DamageEvent e)
    {
        if (!IsActive)
            return;

        if (playerId != 0 && e.targetId != playerId)
            return;

        if (!e.isBlocked)
            return;

        if (!IsValidAttacker(e.attackerId))
            return;

        CompleteObjective();
    }
}
