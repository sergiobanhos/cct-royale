using System.Collections.Generic;
using CctRoyale.Server;
using Unity.Netcode;
using UnityEngine;
using Utils;

public class PlayerController : NetworkBehaviour
{

    public NetworkVariable<int> Team = new NetworkVariable<int>();
    public NetworkVariable<float> Elixir = new NetworkVariable<float>(0f);


    [Header("Player Info")]
    [SerializeField] private List<string> characters = new List<string>();
    public int selectedCharacterIndex = 0;
    [Header("References")]
    [SerializeField] private Transform spawnRoot; // ponto de referência para spawn de tropas


    [Header("Configuração do Elixir")]
    public float maxElixir = 10f;
    public float elixirRegenRate = 1f; // unidades por segundo
    public float doubleElixirMultiplier = 2f;

    private void OnEnable()
    {
        Team.OnValueChanged += OnTeamChanged;
    }

    private void OnDisable()
    {
        Team.OnValueChanged -= OnTeamChanged;
    }

    private void OnTeamChanged(int oldValue, int newValue)
    {
        Debug.Log($"Meu time mudou de {oldValue} para {newValue}");
    }

    public override void OnNetworkSpawn()
    {
        // Só servidor define o time
        if (IsServer)
        {
            Team.Value = GameServerManager.Instance.AssignTeam(this);
        }
    }

    void Start()
    {
        if (IsOwner)
        {
            Debug.Log($"PlayerController iniciado para o client {OwnerClientId}, Team: {Team.Value}");
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
        characterCtrl.SetTeam(Team.Value);
        characterCtrl.SetOwnerId(rpcParams.Receive.SenderClientId);

        var healthComp = characterInstance.GetComponent<HealthComponent>();
        healthComp.SetHealth(character.health);
    }

    [ServerRpc]
    public void SetTeamServerRpc(int team)
    {
        Team.Value = team;
    }

    public void SelectCharacter(int index)
    {
        selectedCharacterIndex = index;
    }
}
