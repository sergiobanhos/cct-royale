using System;
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
    public Action<string> HandleOnSelectedCardChanged;

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
        UpdateElixir();

        if (!IsOwner) return;

        if (Input.GetMouseButtonDown(0))
        {
            MouseClick();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SelectCharacter(0);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SelectCharacter(1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SelectCharacter(2);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SelectCharacter(3);
        }

    }

    private void UpdateElixir()
    {
        if (!IsServer) return;

        float regenAmount = elixirRegenRate * Time.deltaTime;
        if (MatchController.Instance.isDoubleElixir.Value)
        {
            regenAmount *= doubleElixirMultiplier;
        }

        Elixir.Value = Mathf.Min(Elixir.Value + regenAmount, maxElixir);
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

    public static Vector3 GetMousePosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 point = hit.point;
            point.y = 0;
            return point;
        }
        return Vector3.zero;
    }

    private bool TrySpendElixir(int cost)
    {
        if (Elixir.Value >= cost)
        {
            Elixir.Value -= cost;
            return true;
        }
        return false;
    }

    [ServerRpc]
    private void SpawnCardServerRpc(int index, Vector2 world, ServerRpcParams rpcParams = default)
    {
        string characterId = characters[index];
        CardData character = GameInstance.Instance.cardsContainer.GetCardById(characterId);

        bool canSpendElixir = TrySpendElixir(character.cost);
        if (!canSpendElixir)
        {
            return;
        }

        // Instancia prefab Networked
        CardController characterInstance = Instantiate(character.prefab, new Vector3(world.x, 0, world.y), Quaternion.identity);

        var networkObj = characterInstance.GetComponent<NetworkObject>();

        // Configura time e owner
        characterInstance.SetTeam(Team.Value);
        characterInstance.SetData(character);

        // Faz spawn
        networkObj.SpawnWithOwnership(rpcParams.Receive.SenderClientId);


        // var healthComp = characterInstance.GetComponent<HealthComponent>();
        // healthComp.SetHealth(character.stats.health);
        // healthComp.SetTeam(Team.Value);
    }

    [ServerRpc]
    public void SetTeamServerRpc(int team)
    {
        Team.Value = team;
    }

    public void SelectCharacter(int index)
    {
        selectedCharacterIndex = index;
        HandleOnSelectedCardChanged?.Invoke(characters[index]);
    }
}
