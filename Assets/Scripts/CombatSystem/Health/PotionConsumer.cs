using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Health))]
public class PotionConsumer : MonoBehaviour
{
    private Health health;

    [SerializeField] private GameObject rightHandProp;
    [SerializeField] private GameObject leftHandProp;
    [SerializeField] private float hideAfterSeconds = 0.8f;

    private Coroutine hideRoutine;

    private void Awake()
    {
        health = GetComponent<Health>();
        HideBoth();
    }

    private void OnEnable()
    {
        CoreBus.Subscribe<PotionConsumedEvent>(OnPotion);
        HideBoth();
    }

    private void OnDisable()
    {
        CoreBus.Unsubscribe<PotionConsumedEvent>(OnPotion);
        if (hideRoutine != null) { StopCoroutine(hideRoutine); hideRoutine = null; }
        HideBoth();
    }

    private void OnPotion(PotionConsumedEvent e)
    {
        if (health == null || health.IsDead) return;
        if (!CompareTag("Player")) return;

        health.Heal(e.healAmount);
        if (AnimationHelper.Instance != null) AnimationHelper.Instance.Trigger("Drink");

        HideBoth();
        bool facingRight = Vector3.Dot(transform.forward, Vector3.right) >= 0f;
        if (facingRight && rightHandProp) rightHandProp.SetActive(true);
        if (!facingRight && leftHandProp) leftHandProp.SetActive(true);

        if (hideRoutine != null) StopCoroutine(hideRoutine);
        hideRoutine = StartCoroutine(AutoHide(hideAfterSeconds));
    }

    private IEnumerator AutoHide(float t)
    {
        yield return new WaitForSeconds(t);
        HideBoth();
        hideRoutine = null;
    }

    private void HideBoth()
    {
        if (rightHandProp) rightHandProp.SetActive(false);
        if (leftHandProp) leftHandProp.SetActive(false);
    }
}
