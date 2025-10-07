using System.Collections.Generic;
using UnityEngine;

public class HazardPoolManager : MonoBehaviour
{
    public static HazardPoolManager Instance { get; private set; }

    [Tooltip("Pre-placed, disabled hazard objects (each has HazardPoolMember with a hazardId).")]
    [SerializeField] private List<HazardPoolMember> objectsInScene;

    private readonly Dictionary<string, int> nextIndexPerId = new();

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public GameObject Spawn(string hazardId, Vector3 position, Quaternion rotation)
    {
        if (string.IsNullOrEmpty(hazardId)) return null;

        if (!nextIndexPerId.TryGetValue(hazardId, out int startIndex))
            startIndex = 0;

        for (int i = 0; i < objectsInScene.Count; i++)
        {
            int idx = (startIndex + i) % objectsInScene.Count;
            var m = objectsInScene[idx];
            if (!m || m.hazardId != hazardId || m.IsActive) continue;

            m.PrepareForSpawn(position, rotation);

            if (!m.gameObject.activeSelf || !m.gameObject.activeInHierarchy)
            {
                Debug.LogWarning($"[HazardPool] Member '{m.name}' still inactive after PrepareForSpawn. Forcing activeSelf=true.", m);
                m.gameObject.SetActive(true);
            }

            nextIndexPerId[hazardId] = (idx + 1) % objectsInScene.Count;
            return m.gameObject;
        }

        Debug.LogWarning($"HazardPoolManager: No available object with ID '{hazardId}'. Add more copies.", this);
        return null;
    }

    public void Return(HazardPoolMember member)
    {
        if (member != null) member.ReturnToPool();
    }
}
