using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharactersContainer", menuName = "CharactersContainer")]
public class CharactersContainer : ScriptableObject
{
    [SerializeField] private List<CharacterData> characters = new List<CharacterData>();

    public CharacterData GetCharacterById(string characterId)
    {
        return this.characters.Find(c => c.id == characterId);
    }
}
