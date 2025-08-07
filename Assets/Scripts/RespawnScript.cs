using UnityEngine;
using StarterAssets;

public class RespawnScript : MonoBehaviour
{
    [Header("Respawn Settings")]
    public Transform respawnPoint;
    public bool useFixedRotation = false;
    public Vector3 fixedRotation = new Vector3(0, 90, 0);

    private CharacterController _charController;
    private ThirdPersonController _thirdPersonController;

    void Awake()
    {
        _charController = GetComponent<CharacterController>();
        _thirdPersonController = GetComponent<ThirdPersonController>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("[RESPAWN] Key '1' pressed.");
            RespawnPlayer();
        }
    }

    void RespawnPlayer()
    {
        if (respawnPoint == null)
        {
            Debug.LogWarning("[RESPAWN] Respawn point is NOT assigned!");
            return;
        }

        if (_thirdPersonController != null) _thirdPersonController.enabled = false;
        if (_charController != null) _charController.enabled = false;

        Vector3 prevPos = transform.position;
        transform.position = respawnPoint.position;

        if (useFixedRotation)
            transform.rotation = Quaternion.Euler(fixedRotation);

        Debug.Log($"[RESPAWN] Player moved from {prevPos} to {respawnPoint.position}");

        if (_charController != null) _charController.enabled = true;
        if (_thirdPersonController != null) _thirdPersonController.enabled = true;
    }
}
