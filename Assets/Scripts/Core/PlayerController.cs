using System.Collections.Generic;
using UnityEngine;
using Utils;

public class PlayerController : MonoSingleton<PlayerController>
{
    [SerializeField] private List<string> characters = new List<string>();
    public int selectedCharacterIndex = 0;


    void Start()
    {

    }

    public void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            MouseClick();
        }
    }

    private void MouseClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector2 spawnPoint = new Vector2(hit.point.x, hit.point.z);
            this.SpawnCard(selectedCharacterIndex, spawnPoint);
        }
    }

    public void SpawnCard(int index, Vector2 world)
    {
        string characterId = characters[index];
        CharacterData character = GameInstance.Instance.charactersContainer.GetCharacterById(characterId);
        GameNetworkClient.Instance.SendPlaceCard(character.id, world);
    }


    public void SelectCharacter(int index)
    {
        selectedCharacterIndex = index;
    }
}
