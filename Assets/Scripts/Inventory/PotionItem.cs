using UnityEngine;

[CreateAssetMenu(fileName = "New Potion", menuName = "Inventory/Potion")]
public class PotionItem : Item
{
    public int healAmount = 10;
    public ParticleEffectData consumeEffect;
    public AudioClip consumeClip;

    public override bool Use()
    {
        Transform t = null;
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player) t = player.transform;

        if (consumeEffect != null && !string.IsNullOrEmpty(consumeEffect.vfxId) && VfxPoolManager.Instance != null && t != null)
        {
            Vector3 pos = ParticleEffectUtility.CalculateSpawnPosition(t.position, t, consumeEffect.localOffset);
            VfxPoolManager.Instance.Spawn(consumeEffect.vfxId, pos, t.rotation);
        }

        if (consumeClip != null && t != null)
            AudioSource.PlayClipAtPoint(consumeClip, t.position);

        CoreBus.Publish(new PotionConsumedEvent(healAmount));
        return true;
    }
}
