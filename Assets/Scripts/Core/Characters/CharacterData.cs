using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "CharacterData", order = 0)]
public class CharacterData : ScriptableObject
{
    public string id;
    public string name;
    public Sprite sprite;
    public CharacterController prefab;

    [Header("Stats")]
    public CharacterStats stats;

    public CharacterController Spawn(Vec2 world, string SenderId)
    {
        CharacterController instance = Instantiate(prefab, new Vector3(world.x, 0f, world.y), Quaternion.identity);
        instance.HealthComponent.SetClientId(SenderId);
        return instance;
    }
}
