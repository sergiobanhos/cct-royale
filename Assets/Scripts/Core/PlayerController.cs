using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Utils;

public class PlayerController : NetworkBehaviour
{
    [Header("Player Info")]
    [SerializeField] private List<string> characters = new List<string>();
    public int selectedCharacterIndex = 0;
    public int team; // 0 = esquerda, 1 = direita

    [Header("References")]
    [SerializeField] private Transform spawnRoot; // ponto de referência para spawn de tropas

    void Start()
    {
        if (IsOwner)
        {
            Debug.Log($"PlayerController iniciado para o client {OwnerClientId}, Team: {team}");
        }
    }

    void Update()
    {
        if (!IsOwner) return;

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
            SpawnCardServerRpc(selectedCharacterIndex, spawnPoint);
        }
    }

    // Este método é chamado pelo cliente, mas executa no servidor
    [ServerRpc]
    private void SpawnCardServerRpc(int index, Vector2 world, ServerRpcParams rpcParams = default)
    {
        string characterId = characters[index];
        CharacterData character = GameInstance.Instance.charactersContainer.GetCharacterById(characterId);

        // Instancia prefab Networked
        CharacterController prefab = character.prefab; // prefab com NetworkObject e CharacterController
        CharacterController characterInstance = Instantiate(prefab, new Vector3(world.x, 0, world.y), Quaternion.identity);
        
        // Configura time e owner
        var networkObj = characterInstance.GetComponent<NetworkObject>();
        networkObj.SpawnWithOwnership(rpcParams.Receive.SenderClientId);

        var characterCtrl = characterInstance.GetComponent<CharacterController>();
        characterCtrl.SetTeam(team);
        characterCtrl.SetOwnerId(rpcParams.Receive.SenderClientId);
    }

    public void SelectCharacter(int index)
    {
        selectedCharacterIndex = index;
    }
}
