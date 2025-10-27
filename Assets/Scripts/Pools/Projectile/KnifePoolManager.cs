using System.Collections.Generic;
using UnityEngine;

public class KnifePoolManager : MonoBehaviour
{
    public static KnifePoolManager Instance { get; private set; }

    [SerializeField] private List<KnifePoolMember> knivesInScene;

    private readonly Dictionary<string, int> nextIndexPerId = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public KnifePoolMember Spawn(
        string knifeId,
        Vector3 position,
        Quaternion rotation,
        int attackerId,
        AttackData data,
        float speed,
        float distance,
        Vector3 spinPerSecond)
    {
        if (string.IsNullOrEmpty(knifeId) || data == null || knivesInScene == null || knivesInScene.Count == 0)
            return null;

        if (!nextIndexPerId.TryGetValue(knifeId, out int startIndex))
            startIndex = 0;

        int count = knivesInScene.Count;
        for (int i = 0; i < count; i++)
        {
            int currentIndex = (startIndex + i) % count;
            var member = knivesInScene[currentIndex];
            if (member && member.knifeId == knifeId && !member.IsActive)
            {
                member.PrepareForSpawn(position, rotation, attackerId, data, speed, distance, spinPerSecond);
                nextIndexPerId[knifeId] = (currentIndex + 1) % count;
                return member;
            }
        }

        Debug.LogWarning($"KnifePoolManager: No available knife with ID '{knifeId}' found. Add more copies to the scene.", this);
        return null;
    }

    public void Return(KnifePoolMember member)
    {
        if (member != null)
            member.ReturnToPool();
    }
}
