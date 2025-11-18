using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "CharacterData", order = 0)]
public class CharacterData : ScriptableObject
{
    public string id;
    public string name;
    public float health;
    public Sprite sprite;
    public CharacterController prefab;


    [Header("Stats")]
    public CharacterStats stats;

    [Header("Base Attack")]
    public bool isRanged;
    public GameObject projectilePrefab;

    public CharacterController Spawn(Vec2 world, string SenderId)
    {
        CharacterController instance = Instantiate(prefab, new Vector3(world.x, 0f, world.y), Quaternion.identity);
        return instance;
    }

    public void BaseAttack(CharacterController attacker, HealthComponent target)
    {
        if (isRanged)
        {
            GameObject projectileInstance = Instantiate(projectilePrefab, attacker.transform.position + Vector3.up * 5f, Quaternion.identity);
            Projectile projectile = projectileInstance.GetComponent<Projectile>();
            projectile.SetTarget(target.transform);
            projectile.SetOnHitTarget(() =>
            {
                target.TakeDamage(attacker.characterData.stats.attackDamage);
            });
        }
        else
        {
            target.TakeDamage(attacker.characterData.stats.attackDamage);
        }
    }
}
