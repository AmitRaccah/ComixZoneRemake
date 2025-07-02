using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Attack Sequence")]
public class AttackSequence : ScriptableObject
{
    [System.Serializable]
    public struct Step
    {
        public InputType input;    
        public string trigger;  
        public AttackData attack;   
    }

    public Step[] steps;
}
