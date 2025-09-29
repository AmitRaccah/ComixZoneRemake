using UnityEngine;
using System.Collections.Generic;

public class RoomsDirectorManual : MonoBehaviour
{
    [System.Serializable]
    public class Room
    {
        public Health[] enemies;
        public GameObject[] gates;
        public GameObject[] arrows;
        [Tooltip("If greater than zero, this number of enemy defeats is required to unlock the room. Overrides the enemies array check when set.")]
        public int requiredKills;

        [HideInInspector] public int killsSoFar;
        [HideInInspector] public bool isUnlocked;
    }

    public static RoomsDirectorManual Instance;

    public Room[] Rooms;

    private readonly Dictionary<int, int> enemyToRoom = new Dictionary<int, int>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        BuildIndex();
        LockAll();
    }

    void OnEnable()
    {
        CoreBus.Subscribe<HealthDepletedEvent>(OnDead);
    }

    void OnDisable()
    {
        CoreBus.Unsubscribe<HealthDepletedEvent>(OnDead);
    }

    void BuildIndex()
    {
        enemyToRoom.Clear();
        if (Rooms == null) return;

        for (int i = 0; i < Rooms.Length; i++)
        {
            Room r = Rooms[i];
            if (r == null || r.enemies == null) continue;

            for (int j = 0; j < r.enemies.Length; j++)
            {
                Health h = r.enemies[j];
                if (h == null) continue;
                enemyToRoom[h.gameObject.GetInstanceID()] = i;
            }
        }
    }

    void LockAll()
    {
        if (Rooms == null) return;
        for (int i = 0; i < Rooms.Length; i++) LockRoom(i);
    }

    void LockRoom(int index)
    {
        Room r = GetRoom(index);
        if (r == null) return;

        r.killsSoFar = 0;
        r.isUnlocked = false;

        if (r.gates != null)
            for (int i = 0; i < r.gates.Length; i++)
                if (r.gates[i] != null) r.gates[i].SetActive(false);

        if (r.arrows != null)
            for (int i = 0; i < r.arrows.Length; i++)
                if (r.arrows[i] != null) r.arrows[i].SetActive(false);
    }

    Room GetRoom(int index)
    {
        if (Rooms == null) return null;
        if (index < 0 || index >= Rooms.Length) return null;
        return Rooms[index];
    }

    void OnDead(HealthDepletedEvent e)
    {
        int idx;
        if (!enemyToRoom.TryGetValue(e.entityId, out idx)) return;
        TryUnlock(idx);
    }

    void TryUnlock(int index)
    {
        Room r = GetRoom(index);
        if (r == null) return;
        if (!ShouldUnlock(r)) return;

        UnlockRoom(r);
    }

    bool ShouldUnlock(Room r)
    {
        if (r.requiredKills > 0)
            return r.killsSoFar >= r.requiredKills;

        return AllEnemiesDead(r);
    }

    void UnlockRoom(Room r)
    {
        r.isUnlocked = true;
        if (r.gates != null)
            for (int i = 0; i < r.gates.Length; i++)
                if (r.gates[i] != null) r.gates[i].SetActive(true);

        if (r.arrows != null)
            for (int i = 0; i < r.arrows.Length; i++)
                if (r.arrows[i] != null) r.arrows[i].SetActive(true);
    }

    bool AllEnemiesDead(Room r)
    {
        if (r.enemies == null || r.enemies.Length == 0) return true;
        for (int i = 0; i < r.enemies.Length; i++)
        {
            Health h = r.enemies[i];
            if (h != null && !h.IsDead) return false;
        }
        return true;
    }
    public void NotifyEnemyKilled(int roomIndex)
    {
        Room r = GetRoom(roomIndex);
        if (r == null) return;

        r.killsSoFar++;
        TryUnlock(roomIndex);
    }

    public bool IsRoomUnlocked(int roomIndex)
    {
        Room r = GetRoom(roomIndex);
        if (r == null) return false;
        return r.isUnlocked;
    }
}
