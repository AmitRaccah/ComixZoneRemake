using UnityEngine;

public class PauseHoldOverlay : MonoBehaviour
{
    [SerializeField] private GameObject overlayRoot;
    [SerializeField] private KeyCode key = KeyCode.F1;

    float prevTimeScale = 1f;
    bool active;

    void Awake()
    {
        if (overlayRoot) overlayRoot.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(key)) Activate();
        if (Input.GetKeyUp(key)) Deactivate();
    }

    void Activate()
    {
        if (active) return;
        active = true;
        prevTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        if (overlayRoot) overlayRoot.SetActive(true);
    }

    void Deactivate()
    {
        if (!active) return;
        active = false;
        Time.timeScale = prevTimeScale <= 0f ? 1f : prevTimeScale;
        if (overlayRoot) overlayRoot.SetActive(false);
    }

    void OnDisable()
    {
        if (active) Deactivate();
    }
}
