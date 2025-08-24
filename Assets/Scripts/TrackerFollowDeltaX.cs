using UnityEngine;

public class TrackerFollowDeltaX : MonoBehaviour
{
    [System.Serializable]
    public struct RoomSpeed
    {
        public string roomId;
        public float speed;
    }

    public Transform player;
    public float speed = 1f;
    [SerializeField] private RoomSpeed[] roomSpeeds;
    public string CurrentRoomId { get; private set; }

    float lastX;
    bool haveLast;

    void OnEnable()
    {
        haveLast = player != null;
        if (haveLast) lastX = player.position.x;
    }

    public void ResetSync()
    {
        if (!player) return;
        lastX = player.position.x;
        haveLast = true;
    }

    public void ApplyRoom(string roomId)
    {
        if (string.IsNullOrEmpty(roomId)) return;
        for (int i = 0; i < roomSpeeds.Length; i++)
        {
            if (roomSpeeds[i].roomId == roomId)
            {
                speed = roomSpeeds[i].speed;
                CurrentRoomId = roomId;
                return;
            }
        }
        CurrentRoomId = roomId;
    }

    public void SetSpeed(float s)
    {
        speed = s;
    }

    void LateUpdate()
    {
        if (!player) return;

        if (!haveLast) { lastX = player.position.x; haveLast = true; return; }

        float dx = player.position.x - lastX;
        lastX = player.position.x;

        if (dx == 0f) return;

        Vector3 p = transform.position;
        p.x += dx * speed;
        transform.position = p;
    }
}
