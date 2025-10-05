using UnityEngine;
using System.Collections.Generic;

public class VfxPoolManager : MonoBehaviour
{
    public static VfxPoolManager Instance { get; private set; }

    [Tooltip("Drag all pre-placed, disabled VFX objects from the Hierarchy into this list.")]
    [SerializeField] private List<VfxPoolMember> effectsInScene;

    private readonly Dictionary<string, int> nextIndexPerId = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public GameObject Spawn(string vfxId, Vector3 position, Quaternion rotation)
    {
        if (string.IsNullOrEmpty(vfxId)) return null;

        if (!nextIndexPerId.TryGetValue(vfxId, out int startIndex))
        {
            startIndex = 0;
        }

        for (int i = 0; i < effectsInScene.Count; i++)
        {
            int currentIndex = (startIndex + i) % effectsInScene.Count;
            var effect = effectsInScene[currentIndex];

            if (effect.vfxId == vfxId && !effect.IsActive)
            {
                effect.PrepareForSpawn(position, rotation);
                nextIndexPerId[vfxId] = (currentIndex + 1) % effectsInScene.Count;
                return effect.gameObject;
            }
        }

        Debug.LogWarning($"VfxPoolManager: No available effect with ID '{vfxId}' found. Consider adding more copies to the scene.", this);
        return null;
    }

    internal void Return(VfxPoolMember member)
    {
        if (member != null)
        {
            member.ReturnToPool();
        }
    }
}
