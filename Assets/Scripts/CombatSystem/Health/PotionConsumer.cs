using UnityEngine;

[RequireComponent(typeof(Health))]
public class PotionConsumer : MonoBehaviour
{
    private Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    private void OnEnable()
    {
        CoreBus.Subscribe<PotionConsumedEvent>(OnPotion);
    }

    private void OnDisable()
    {
        CoreBus.Unsubscribe<PotionConsumedEvent>(OnPotion);
    }

    private void OnPotion(PotionConsumedEvent e)
    {
        if (health == null) return;
        if (health.IsDead) return;
        if (!CompareTag("Player")) return;

        health.Heal(e.healAmount);

        if (AnimationHelper.Instance != null)
        {
            AnimationHelper.Instance.Trigger("Drink");
        }
    }
}
