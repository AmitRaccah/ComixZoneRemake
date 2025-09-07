using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Attack Sequence")]
public class AttackSequence : ScriptableObject
{
    public Step[] steps;

    [Header("Loop settings")]
    public bool loopableDuringAttack = false; 


}
