using UnityEngine;

[CreateAssetMenu(fileName = "TroopCardData", menuName = "Cards/TroopCardData")]
public class TroopCardData : CardData
{
    [Header("Base Attack")]
    public bool isRanged;
    public GameObject projectilePrefab;

    public override void ServerSpawn(Vector2 position, ulong ownerClientId, int team)
    {
        // Instantiate prefab Networked
        CardController characterInstance = Instantiate(this.prefab, new Vector3(position.x, 0, position.y), Quaternion.identity);

        var networkObj = characterInstance.GetComponent<Unity.Netcode.NetworkObject>();

        // Spawn
        networkObj.SpawnWithOwnership(ownerClientId);

        // Setup team and data
        characterInstance.SetTeam(team);
        characterInstance.SetData(this);

        characterInstance.Activate();
    }

    public override CardController Spawn(Vector2 world, string SenderId)
    {
        CardController instance = Instantiate(this.prefab, new Vector3(world.x, 0f, world.y), Quaternion.identity);
        return instance;
    }

    public void BaseAttack(TroopController attacker, HealthComponent target)
    {
        if (isRanged)
        {
            GameObject projectileInstance = Instantiate(projectilePrefab, attacker.transform.position + Vector3.up * 10f + attacker.transform.forward * 4f, Quaternion.identity);
            Projectile projectile = projectileInstance.GetComponent<Projectile>();
            projectile.NetworkObject.Spawn();
            projectile.SetTarget(target.transform);
            projectile.SetOnHitTarget(() =>
            {
                target.TakeDamage(attacker.cardData.stats.attackDamage);
                AudioManager.Play(this.onHitSound, target.transform.position);
            });
        }
        else
        {
            target.TakeDamage(attacker.cardData.stats.attackDamage);
            AudioManager.Play(this.onHitSound, target.transform.position);
        }
    }
}
