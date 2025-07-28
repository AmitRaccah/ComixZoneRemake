using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class TriggerDelayedActivator : MonoBehaviour
{
    [SerializeField] private List<GameObject> objectsToActivate = new();
    [SerializeField] private List<GameObject> objectsToDeactivate = new();
    [SerializeField] private float activateDelay = 4f;
    [SerializeField] private float deactivateDelay = 4f;
    bool started;
    void Awake()
    {
        foreach (var go in objectsToActivate)
            if (go) go.SetActive(false);
    }
    public void BeginTimers()
    {
        if (started) return;
        started = true;
        StartCoroutine(ActivateAfter());
        StartCoroutine(DeactivateAfter());
    }
    IEnumerator ActivateAfter()
    {
        yield return new WaitForSeconds(activateDelay);
        foreach (var go in objectsToActivate)
            if (go) go.SetActive(true);
    }
    IEnumerator DeactivateAfter()
    {
        yield return new WaitForSeconds(deactivateDelay);
        foreach (var go in objectsToDeactivate)
            if (go) go.SetActive(false);
        Destroy(this);
    }
}