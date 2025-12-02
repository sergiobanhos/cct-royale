using UnityEngine;

[CreateAssetMenu(fileName = "GroupTroopCardData", menuName = "Cards/GroupTroopCardData")]
public class GroupTroopCardData : TroopCardData
{
    [Header("Group Settings")]
    public int amount = 3;
    public float radius = 1.5f;

    public override void ServerSpawn(Vector2 position, ulong ownerClientId, int team)
    {
        for (int i = 0; i < amount; i++)
        {
            Vector2 randomOffset = Random.insideUnitCircle * radius;
            Vector2 spawnPos = position + randomOffset;
            
            base.ServerSpawn(spawnPos, ownerClientId, team);
        }
    }
}
