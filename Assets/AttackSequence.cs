using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Attack Sequence")]
public class AttackSequence : ScriptableObject
{
    public Step[] steps; 

    [System.Serializable]
    public struct Step
    {
        public InputType input;    
        public PlayerStance stance;   
        public string trigger;  
        public AttackData attack;   
    }
}
