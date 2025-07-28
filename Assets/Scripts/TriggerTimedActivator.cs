

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TriggerDelayedActivator : MonoBehaviour
{
    [Tooltip("Objects that will turn ON after the delay.")]
    [SerializeField] private List<GameObject> objectsToActivate = new();

    [Tooltip("Seconds to wait AFTER entry before turning them ON.")]
    [SerializeField] private float delaySeconds = 4f;

    bool started;

    /* ───── Ensure objects start OFF ───── */
    void Awake()
    {
        foreach (var go in objectsToActivate)
            if (go) go.SetActive(false);
    }

    /* ───── Trigger Enter ───── */
    void OnTriggerEnter(Collider other)
    {
        if (started || !other.CompareTag("Player")) return;

        started = true;
        StartCoroutine(ActivateAfterDelay());
    }

    /* ───── Coroutine ───── */
    IEnumerator ActivateAfterDelay()
    {
        yield return new WaitForSeconds(delaySeconds);

        foreach (var go in objectsToActivate)
            if (go) go.SetActive(true);

        Destroy(this);         // one‑shot
    }
}
