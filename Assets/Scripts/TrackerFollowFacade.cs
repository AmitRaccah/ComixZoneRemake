using UnityEngine;

public class TrackerFollowFacade : MonoBehaviour
{
    public TrackerFollowDeltaX follow;

    public void ApplyRoom(string roomId)
    {
        if (follow == null) return;
        follow.ApplyRoom(roomId);
        follow.ResetSync();
    }

    public void SetSpeed(float s)
    {
        if (follow == null) return;
        follow.SetSpeed(s);
        follow.ResetSync();
    }

    public void EnableFollow()
    {
        if (follow == null) return;
        follow.enabled = true;
        follow.ResetSync();
    }

    public void DisableFollow()
    {
        if (follow == null) return;
        follow.enabled = false;
    }
}
