using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpellController : CardController<SpellCardData>
{
    [Header("Components")]
    [SerializeField] private ParticleSystem effectParticles;
    
    private bool hasBeenCast = false;
    private Vector3 targetPosition;
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
      
    }

    public override void Activate()
    {
        if (IsServer)
        {
            // Store the target position
            targetPosition = transform.position;
            
            // Handle different spell types
            switch (cardData.spellType)
            {
                case SpellType.Instant:
                    CastInstantSpell();
                    break;
                    
                case SpellType.OverTime:
                    StartCoroutine(CastOverTimeSpell());
                    break;
                    
                case SpellType.Projectile:
                    LaunchProjectile();
                    break;
            }
        }
    }
    
    private void CastInstantSpell()
    {
        if (hasBeenCast) return;
        hasBeenCast = true;
        
        // Apply instant damage to all targets in radius
        ApplySpellEffect(targetPosition, cardData.damage);
        
        // Visual effects
        SpawnEffectClientRpc();
        
        // Destroy after a short delay
        StartCoroutine(DelayedDestroy(1.5f));
    }
    
    private IEnumerator CastOverTimeSpell()
    {
        if (hasBeenCast) yield return null;
        hasBeenCast = true;
        
        // Visual effects
        SpawnEffectClientRpc();
        
        // Apply damage/effect over time
        float elapsedTime = 0f;
        while (elapsedTime < cardData.duration)
        {
            // Apply damage tick
            ApplySpellEffect(targetPosition, cardData.damage * cardData.tickRate);
            
            yield return new WaitForSeconds(cardData.tickRate);
            elapsedTime += cardData.tickRate;
        }
        
        // Destroy after effect duration
        StartCoroutine(DelayedDestroy(1.0f));
    }
    
    private void LaunchProjectile()
    {
        if (hasBeenCast) return;
        hasBeenCast = true;
        
        // Create projectile
        GameObject projectileObj = Instantiate(cardData.projectilePrefab, 
            transform.position, 
            Quaternion.identity);

            
        SpellProjectile projectile = projectileObj.GetComponent<SpellProjectile>();
        

        if (projectile != null)
        {
            projectile.Initialize(
                GetStartPosition(),
                targetPosition,
                cardData.projectileSpeed,
                cardData.projectileArc,
                GetTeam()
            );
            
            // Set callback for when projectile hits target
            projectile.OnImpact += (impactPos) => {
                // Apply damage at impact location
                ApplySpellEffect(impactPos, cardData.damage);
                
                // Visual effects at impact location
                SpawnImpactEffectClientRpc(impactPos);
                
                // Destroy this controller
                StartCoroutine(DelayedDestroy(0.5f));
            };

            projectile.NetworkObject.Spawn();
        }
    }
    
    private void ApplySpellEffect(Vector3 position, float damageAmount)
    {
        List<HealthComponent> targets = CombatUtility.FindHealthComponentsInRadius(position, cardData.radius, GetTeam(), true);
        Debug.Log($"Meu time: {GetTeam()}");
        Debug.Log(targets.Count);

        foreach (var health in targets.ToArray())
        {
            Debug.Log(health.transform.name);
            // Apply damage
            health.TakeDamage(damageAmount);
            
            // // Apply slow effect if this is a slowing spell
            // if (cardData.slowsTargets)
            // {
            //     TroopController troop = health.GetComponent<TroopController>();
            //     if (troop != null)
            //     {
            //         troop.ApplySlowEffect(cardData.slowAmount, cardData.slowDuration);
            //     }
            // }
        }
    }
    
    private IEnumerator DelayedDestroy(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (IsServer)
        {
            NetworkObject.Despawn();
        }
    }

    private Vector3 GetStartPosition()
    {
        MatchController matchController = MatchController.Instance;

        if (matchController)
        {
            return matchController.GetSpellSpawnPositionForTeam(GetTeam());
        }
        
        return Vector3.zero;
    }
    
    [ClientRpc]
    private void SpawnEffectClientRpc()
    {
        if (effectParticles != null)
        {
            effectParticles.Play();
        }
        
        if (cardData.effectPrefab != null)
        {
            Instantiate(cardData.effectPrefab, transform.position, Quaternion.identity);
        }
    }
    
    [ClientRpc]
    private void SpawnImpactEffectClientRpc(Vector3 position)
    {
        if (cardData.effectPrefab != null)
        {
            Instantiate(cardData.effectPrefab, position, Quaternion.identity);
        }
    }
}