using System;
using Unity.Netcode;
using UnityEngine;

public class HealthComponent : NetworkBehaviour, ICombatTarget
{
    public NetworkVariable<float> currentHealth = new NetworkVariable<float>();
    private float maxHealth = 100f;

    public bool isEnemy = false;
    public int team = 0;
    public Action<float> OnHealthChanged;

    public void SetHealth(float health)
    {
        maxHealth = health;
        currentHealth.Value = health;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public void TakeDamage(float damage)
    {
        if (!IsServer) return;

        currentHealth.Value -= damage;
        if (currentHealth.Value < 0) currentHealth.Value = 0;

        if (currentHealth.Value == 0)
        {
            NetworkObject.Despawn(); 
        }

        NotifyTakeDamageClientRpc(currentHealth.Value / maxHealth);
    }

    [ClientRpc]
    private void NotifyTakeDamageClientRpc(float healthPercent)
    {
        OnHealthChanged?.Invoke(healthPercent);
    }

    

    public void SetTeam(int team)
    {
        this.team = team;
    }

    public int GetTeam()
    {
        return team;
    }
}
