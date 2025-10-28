using UnityEngine;

public class HealthComponent : MonoBehaviour, ICombatTarget
{
    public string clientId;
    public bool isEnemy = false;
    public int playerIndex;
    int ICombatTarget.playerIndex => playerIndex;

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public void TakeDamage(int damage)
    {
        // Implement damage logic here

    }
    
    public void SetClientId(string clientId)
    {
        this.clientId = clientId;
    }
    
}
