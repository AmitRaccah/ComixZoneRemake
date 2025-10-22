using UnityEngine;

[DisallowMultipleComponent]
public class SpawnDamageBlocker : MonoBehaviour
{
    public bool Active { get; private set; }
    public void SetActive(bool on) { Active = on; }
}
