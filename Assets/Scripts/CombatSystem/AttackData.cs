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
}

public enum DamageType { Punch, HeavyPunch }
