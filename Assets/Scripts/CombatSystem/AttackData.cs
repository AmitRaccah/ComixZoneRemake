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
    public float freezeFrameDuration = 0.1f;

    public GameObject hitEffectPrefab;
    public Vector3 hitEffectOffset;

    public List<ParticleEffectData> additionalHitEffects = new List<ParticleEffectData>();

}

public enum DamageType
{
    Punch, HeavyPunch
}

public enum AttackSide
{
    LeftHand = 0,
    RightHand = 1,
    LeftFoot = 2,
    RightFoot = 3,
}
