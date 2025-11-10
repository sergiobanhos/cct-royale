using UnityEngine;

public class HealthComponent : MonoBehaviour, ICombatTarget
{
    public bool isEnemy = false;
    public int team = 0;

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public void TakeDamage(int damage)
    {
        // Implement damage logic here
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
