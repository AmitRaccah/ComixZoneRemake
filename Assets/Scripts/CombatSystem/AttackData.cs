using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Attack Data")]
public class AttackData : ScriptableObject
{
    public string attackName;
    public DamageType damageType;
    public int damage = 5;
    public float knockback = 2f;
    public float activeTime = 0.15f;
    public float hitboxRadius = 0.25f;
    public Vector3 hitboxOffset;
    public AttackSide side = AttackSide.LeftHand;
    public float shakeAmplitude = 1f;
    public float blockShakeAmplitude = -1f;
    public float freezeFrameDuration = 0.1f;
    public float blockFreezeFrameDuration = -1f;
    //public GameObject hitEffectPrefab;
    //public Vector3 hitEffectOffset;
    public List<ParticleEffectData> additionalHitEffects = new List<ParticleEffectData>();
    public List<ParticleEffectData> blockHitEffects = new List<ParticleEffectData>();

    public float GetShakeAmplitude(bool blocked)
    {
        if (!blocked) return shakeAmplitude;
        return blockShakeAmplitude >= 0f ? blockShakeAmplitude : shakeAmplitude;
    }

    public float GetFreezeFrameDuration(bool blocked)
    {
        if (!blocked) return freezeFrameDuration;
        return blockFreezeFrameDuration >= 0f ? blockFreezeFrameDuration : freezeFrameDuration;
    }

    public IReadOnlyList<ParticleEffectData> GetHitEffects(bool blocked)
    {
        if (!blocked) return additionalHitEffects;
        return (blockHitEffects != null && blockHitEffects.Count > 0) ? blockHitEffects : additionalHitEffects;
    }
}

public enum DamageType
{
    Punch,
    HeavyPunch,
    Knife
}

public enum AttackSide
{
    LeftHand = 0,
    RightHand = 1,
    LeftFoot = 2,
    RightFoot = 3,
}