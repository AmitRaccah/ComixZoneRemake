using UnityEngine;

public class PauseHoldOverlay : MonoBehaviour
{
    [SerializeField] private GameObject overlayRoot;
    [SerializeField] private KeyCode key = KeyCode.F1;
    [SerializeField] private GameObject[] hideWhileActive;

    float prevTimeScale = 1f;
    bool active;
    bool[] prevActiveStates;

    void Awake()
    {
        if (overlayRoot) overlayRoot.SetActive(false);

        if (hideWhileActive != null && hideWhileActive.Length > 0)
        {
            prevActiveStates = new bool[hideWhileActive.Length];
            for (int i = 0; i < hideWhileActive.Length; i++)
            {
                prevActiveStates[i] = hideWhileActive[i] && hideWhileActive[i].activeSelf;
            }
        }
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

        if (hideWhileActive != null)
        {
            for (int i = 0; i < hideWhileActive.Length; i++)
            {
                if (!hideWhileActive[i]) continue;
                prevActiveStates[i] = hideWhileActive[i].activeSelf;
                hideWhileActive[i].SetActive(false);
            }
        }
    }

    void Deactivate()
    {
        if (!active) return;
        active = false;

        Time.timeScale = prevTimeScale <= 0f ? 1f : prevTimeScale;

        if (overlayRoot) overlayRoot.SetActive(false);

        if (hideWhileActive != null)
        {
            for (int i = 0; i < hideWhileActive.Length; i++)
            {
                if (!hideWhileActive[i]) continue;
                hideWhileActive[i].SetActive(prevActiveStates[i]);
            }
        }
    }

    void OnDisable()
    {
        if (active) Deactivate();
    }
}
