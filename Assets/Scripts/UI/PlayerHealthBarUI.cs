using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerHealthBarUI : MonoBehaviour
{
    [SerializeField] private Health playerHealth;
    [SerializeField] private Slider slider;

    private void OnEnable()
    {
        CoreBus.Subscribe<HealthChangedEvent>(OnHealthChanged);
        //  StartCoroutine(InitialSync());
    }

    private void OnDisable()
    {
        CoreBus.Unsubscribe<HealthChangedEvent>(OnHealthChanged);
    }

    //private IEnumerator InitialSync()
    //{
    //    yield return null;
    //    Apply(playerHealth.CurrentHp, playerHealth.MaxHp);
    //}

    private void OnHealthChanged(HealthChangedEvent e)
    {
        if (e.entityId != playerHealth.EntityId) return;
        Apply(e.current, e.max);
    }

    private void Apply(int current, int max)
    {
        if (!slider) return;
        slider.minValue = 0;
        slider.maxValue = max;
        slider.value = current;
    }

    public void SetTarget(Health h)
    {
        playerHealth = h;
        if (playerHealth != null)
        {
            Apply(playerHealth.CurrentHp, playerHealth.MaxHp);
        }
    }

}