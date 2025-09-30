using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "CharacterData", order = 0)]
public class CharacterData : ScriptableObject
{
    public string id;
    public string name;
    public Sprite sprite;
    public GameObject prefab;

    public void Spawn(Vec2 world) => Instantiate(prefab, new Vector3(world.x, 0f, world.y), Quaternion.identity);
}
