using UnityEngine;

public static class BlockUtil
{
    public static bool IsBlocked(Component defender, int attackerId)
    {
        Transform t = defender.transform;

        BlockController bc = defender.GetComponent<BlockController>();
        if (bc == null) return false;
        if (!bc.IsBlocking) return false;

        Transform atk;
        if (!AttackActivator.TransformsById.TryGetValue(attackerId, out atk))
            return false;

        Vector3 d = atk.position - t.position;
        d.y = 0f;

        Vector3 fwd = t.forward;
        fwd.y = 0f;
        if (fwd.sqrMagnitude > 0f)
            fwd.Normalize();
        else
            fwd = Vector3.right;

        float dotFx = Vector3.Dot(fwd, Vector3.right);

        float facingSign;
        if (Mathf.Abs(dotFx) > 0.2f)
        {
            facingSign = Mathf.Sign(dotFx);
        }
        else
        {
            float sx = t.lossyScale.x;
            if (Mathf.Approximately(sx, 0f)) sx = 1f;
            facingSign = Mathf.Sign(sx);
        }

        bool attackerInFront;
        if (facingSign > 0f)
            attackerInFront = d.x > 0f;
        else
            attackerInFront = d.x < 0f;

        return attackerInFront;
    }
}