using UnityEngine;

[CreateAssetMenu(fileName = "TroopCardData", menuName = "Cards/TroopCardData")]
public class TroopCardData : CardData
{
    [Header("Base Attack")]
    public bool isRanged;
    public GameObject projectilePrefab;

    public override CardController Spawn(Vector2 world, string SenderId)
    {
        CardController instance = Instantiate(this.prefab, new Vector3(world.x, 0f, world.y), Quaternion.identity);
        return instance;
    }

    public void BaseAttack(TroopController attacker, HealthComponent target)
    {
        if (isRanged)
        {
            GameObject projectileInstance = Instantiate(projectilePrefab, attacker.transform.position + Vector3.up * 5f, Quaternion.identity);
            Projectile projectile = projectileInstance.GetComponent<Projectile>();
            projectile.SetTarget(target.transform);
            projectile.SetOnHitTarget(() =>
            {
                target.TakeDamage(attacker.cardData.stats.attackDamage);
            });
        }
        else
        {
            target.TakeDamage(attacker.cardData.stats.attackDamage);
        }
    }
}
