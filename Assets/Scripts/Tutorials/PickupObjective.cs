using UnityEngine;

public class PickupObjective : TutorialObjective
{
    [SerializeField, Min(1)]
    private int requiredAmountToCollect = 2;

    private int baselineCount;

    protected override void OnBegin()
    {
        baselineCount = GetTotalItemCount();
        CoreBus.Subscribe<InventoryChangedEvent>(OnInventoryChanged);
        TryComplete();
    }

    protected override void OnEnd()
    {
        CoreBus.Unsubscribe<InventoryChangedEvent>(OnInventoryChanged);
    }

    protected override void OnReset()
    {
        baselineCount = GetTotalItemCount();
    }

    private void OnInventoryChanged(InventoryChangedEvent _)
    {
        if (!IsActive) return;
        TryComplete();
    }

    private void TryComplete()
    {
        int now = GetTotalItemCount();
        if (now - baselineCount >= requiredAmountToCollect)
            CompleteObjective();
    }

    private int GetTotalItemCount()
    {
        if (InventoryManager.Instance == null) return 0;

        int count = 0;
        var inv = InventoryManager.Instance.inventory;
        for (int i = 0; i < inv.Count; i++)
            if (inv[i] != null)
                count++; 
        return count;
    }
}
