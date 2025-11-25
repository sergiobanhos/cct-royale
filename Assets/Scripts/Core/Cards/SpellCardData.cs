using UnityEngine;

public enum SpellType
{
    Instant,      // Immediate damage in area
    OverTime,     // Damage/effect over time in area
    Projectile    // Travels to target location before effect
}

[CreateAssetMenu(fileName = "New Spell Card", menuName = "Cards/Spell Card")]
public class SpellCardData : CardData
{
    [Header("Spell Properties")]
    public SpellType spellType;
    public float radius = 2.5f;
    public float damage = 100f;
    public float duration = 3f;
    
    [Header("Visual Effects")]
    public GameObject effectPrefab;
    
    [Header("Projectile Properties")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 15f;
    public float projectileArc = 0.5f;  // Height of projectile arc (0 = straight line)
    
    [Header("Over Time Properties")]
    public float tickRate = 0.5f;       // How often damage/effect is applied
    public bool slowsTargets = false;   // For ice/freeze spells
    public float slowAmount = 0.5f;     // 50% slow
    
    public override CardController Spawn(Vector2 world, string SenderId)
    {
        CardController instance = Instantiate(this.prefab, new Vector3(world.x, 0f, world.y), Quaternion.identity);
        return instance;
    }
    
    public override CardType GetCardType()
    {
        return CardType.Spell;
    }
}