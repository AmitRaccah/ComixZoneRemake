using UnityEngine;

public class PickupObjective : TutorialObjective
{
    [Header("Balloon")]
    [SerializeField] private Sprite promptSprite; // אייקון "Pickup" / מקש הרמה

    [SerializeField] private int requiredCount = 2;

    private int baselineCount;

    protected override void OnBegin()
    {
        if (promptSprite) Manager.ShowBalloon(promptSprite);
        CacheBaseline();
        CoreBus.Subscribe<InventoryChangedEvent>(OnInventoryChanged);
        CheckCompletion();
    }

    protected override void OnEnd()
    {
        CoreBus.Unsubscribe<InventoryChangedEvent>(OnInventoryChanged);
        Manager.HideBalloon();
    }

    protected override void OnReset()
    {
        CacheBaseline();
    }

    private void CacheBaseline()
    {
        baselineCount = CountMatches();
    }

    private void OnInventoryChanged(InventoryChangedEvent _)
    {
        if (!IsActive) return;
        CheckCompletion();
    }

    private void CheckCompletion()
    {
        if (CountMatches() >= baselineCount + requiredCount)
            CompleteObjective();
    }

    private int CountMatches()
    {
        if (InventoryManager.Instance == null) return 0;
        int count = 0;
        var inv = InventoryManager.Instance.inventory;
        for (int i = 0; i < inv.Count; i++) if (inv[i] != null) count++;
        return count;
    }

    protected override void OnComplete()
    {
        Manager.HideBalloon();
    }
}
