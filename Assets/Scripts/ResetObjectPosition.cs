using UnityEngine;

public class ResetObjectPosition : MonoBehaviour
{
    public Transform startMarker;

    public void PerformReset()
    {
        if (startMarker == null)
        {
            Debug.LogWarning("Start Marker is not assigned on " + gameObject.name);
            return;
        }

        transform.position = startMarker.position;
        transform.rotation = startMarker.rotation;
    }
}