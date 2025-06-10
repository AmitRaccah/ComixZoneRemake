using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Attack Data")]
public class AttackData : ScriptableObject
{
    public string attackName;          
    public DamageType damageType;          // enum
    public int damage = 5;             
    public float knockback = 2f;       

    public float hitboxRadius = 0.25f; 
    public Vector3 hitboxOffset;       
    public float activeTime = 0.15f;
    public AttackSide side = AttackSide.Left;

    //CAM
    public float shakeAmplitude = 1f;

    public float freezeFrameDuration = 0.1f;

    //VFX
    public GameObject hitEffectPrefab;
    public Vector3 hitEffectOffset;
}

public enum DamageType { Punch }
public enum AttackSide { Left, Right }
