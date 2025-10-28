using UnityEngine;

public interface ICombatTarget
{
    public Vector3 GetPosition();
    public void TakeDamage(int damage);

    public int playerIndex { get; }
}
