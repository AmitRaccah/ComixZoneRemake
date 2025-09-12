using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyHealthBarWorld : MonoBehaviour
{
    [SerializeField] private Health target;
    [SerializeField] private Slider slider;

    private void OnEnable()
    {
        if (!target || !slider) { enabled = false; return; }
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
    //    Apply(target.CurrentHp, target.MaxHp);
    //}

    private void OnHealthChanged(HealthChangedEvent e)
    {
        if (!target || e.entityId != target.EntityId) return;
        Apply(e.current, e.max);
    }

    private void Apply(int current, int max)
    {
        slider.minValue = 0;
        slider.maxValue = max;
        slider.value = current;
    }
}