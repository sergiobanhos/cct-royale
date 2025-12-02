using Unity.Netcode;
using UnityEngine;

public class TowerController : NetworkBehaviour
{
    [Header("Components")]
    [SerializeField] private HealthComponent healthComponent = null;
    [SerializeField] private Transform cannonTransform = null;
    [SerializeField] private Transform projectileSpawnPoint = null;

    [Header("Settings")]
    [SerializeField] private float attackRange = 5f;
    [SerializeField] private float attackRate = 1f; // Attacks per second
    [SerializeField] private float damage = 10f;

    [Header("Prefabs")]
    [SerializeField] private Projectile projectilePrefab = null;

    private NetworkVariable<NetworkObjectReference> netTarget = new NetworkVariable<NetworkObjectReference>();
    private HealthComponent currentTarget;
    private float attackTimer = 0f;

    private void Awake()
    {
        if (healthComponent == null) healthComponent = GetComponent<HealthComponent>();
    }

    public void SetTeam(int t)
    {
        if (healthComponent != null)
        {
            healthComponent.SetTeam(t);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            netTarget.OnValueChanged += OnTargetChanged;
            ResolveTarget();
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            netTarget.OnValueChanged -= OnTargetChanged;
        }
    }

    private void OnTargetChanged(NetworkObjectReference previousValue, NetworkObjectReference newValue)
    {
        ResolveTarget();
    }

    private void ResolveTarget()
    {
        if (netTarget.Value.TryGet(out NetworkObject targetObj))
        {
            currentTarget = targetObj.GetComponent<HealthComponent>();
        }
        else
        {
            currentTarget = null;
        }
    }

    private void Update()
    {
        // Server Logic: Find and Manage Target
        if (IsServer)
        {
            ServerUpdate();
        }

        // Client & Server Logic: Visuals (Rotation)
        if (currentTarget != null)
        {
            LookAtTarget();
        }
    }

    private void ServerUpdate()
    {
        // Check if current target is still valid
        if (currentTarget != null)
        {
            bool isDead = currentTarget.IsDead();
            bool isOutOfRange = Vector3.Distance(transform.position, currentTarget.GetPosition()) > attackRange;

            if (isDead || isOutOfRange)
            {
                currentTarget = null;
                netTarget.Value = new NetworkObjectReference(); // Clear network target
            }
        }

        // If no target, try to find one
        if (currentTarget == null)
        {
            FindNewTarget();
        }

        // If we have a target, engage
        if (currentTarget != null)
        {
            AttackLogic();
        }
    }

    private void FindNewTarget()
    {
        HealthComponent nearest = null;
        float nearestDist = Mathf.Infinity;
        int myTeam = healthComponent.GetTeam();
        
        // Find all HealthComponents in the scene (optimization: could be cached or managed by a GameController)
        var allTargets = FindObjectsByType<HealthComponent>(FindObjectsSortMode.None);

        foreach (var target in allTargets)
        {
            if (target.GetTeam() == myTeam) continue;
            if (target.IsDead()) continue;
            if (target.gameObject == gameObject) continue;

            float dist = Vector3.Distance(transform.position, target.GetPosition());
            
            // Only consider targets within range
            if (dist <= attackRange && dist < nearestDist)
            {
                nearestDist = dist;
                nearest = target;
            }
        }
        
        currentTarget = nearest;
        
        // Sync to clients
        if (currentTarget != null && currentTarget.TryGetComponent(out NetworkObject no))
        {
            netTarget.Value = no;
        }
    }

    private void LookAtTarget()
    {
        if (cannonTransform == null || currentTarget == null) return;
        
        Vector3 dir = currentTarget.GetPosition() - cannonTransform.position;
        dir.y = 0; // Keep rotation flat on Y axis
        
        if (dir != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(dir);
            // Smooth rotation
            cannonTransform.rotation = Quaternion.Slerp(cannonTransform.rotation, lookRot, Time.deltaTime * 10f);
        }
    }

    private void AttackLogic()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer >= 1f / attackRate)
        {
            attackTimer = 0f;
            SpawnProjectile();
        }
    }

    private void SpawnProjectile()
    {
        if (projectilePrefab == null || projectileSpawnPoint == null) return;

        // Spawn projectile
        Projectile p = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
        p.GetComponent<NetworkObject>().Spawn();
        
        // Setup projectile
        p.SetTarget(currentTarget.transform);
        p.SetOnHitTarget(() => {
            // Apply damage when projectile hits
            if (currentTarget != null && !currentTarget.IsDead())
            {
                currentTarget.TakeDamage(damage);
            }
        });
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
