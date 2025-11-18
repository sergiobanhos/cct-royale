using UnityEngine;

public interface ICombatTarget
{
    public Vector3 GetPosition();
    public void TakeDamage(float damage);

    public int GetTeam();
    public void SetTeam(int team);
}
