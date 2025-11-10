using UnityEngine;

public interface ICombatTarget
{
    public Vector3 GetPosition();
    public void TakeDamage(int damage);

    public int GetTeam();
    public void SetTeam(int team);
}
