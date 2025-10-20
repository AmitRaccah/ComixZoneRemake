using UnityEngine;

public static class BlockUtil
{
    public static bool IsBlocked(Component defender, int attackerId)
    {
        var bc = defender.GetComponent<BlockController>();
        if (bc == null || !bc.IsBlocking) return false;
        if (!AttackActivator.TransformsById.TryGetValue(attackerId, out var atk)) return false;
        return Axis2D.IsInFront(defender.transform, atk.position);
    }
}
