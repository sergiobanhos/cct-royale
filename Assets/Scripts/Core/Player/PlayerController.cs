using System;
using System.Collections.Generic;
using CctRoyale.Server;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI; // Required for NavMesh checking
using UnityEngine.EventSystems;
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
    [SerializeField] private Transform spawnRoot;

    [Header("Configuração do Elixir")]
    public float maxElixir = 10f;
    public float elixirRegenRate = 1f;
    public float doubleElixirMultiplier = 2f;

    [Header("Grid & Placement Settings")]
    [SerializeField] private static float gridSize = 5.0f;
    [SerializeField] private static float navMeshCheckRadius = 0.5f; 

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

        // Visualization (Optional: Just to see where the mouse is checking)
        var mouseData = GetMousePosition();
        if (mouseData.isValid)
        {
            // Green line for valid position
            Debug.DrawLine(mouseData.position, mouseData.position + Vector3.up * 2, Color.green);
        }
        else
        {
            // Red line for invalid position
            Debug.DrawLine(mouseData.position, mouseData.position + Vector3.up * 2, Color.red);
        }

        if (Input.GetMouseButtonDown(0))
        {
            MouseClick();
        }

        // Character Selection Input...
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectCard(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectCard(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectCard(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SelectCard(3);
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
        var mouseData = GetMousePosition();

        if (mouseData.isValid)
        {
            Vector2 spawnPoint = new Vector2(mouseData.position.x, mouseData.position.z);
            SpawnCardServerRpc(selectedCharacterIndex, spawnPoint);
        }
        else
        {
            Debug.Log("Cannot place card here: Invalid Position or No NavMesh.");
        }
    }

    /// <summary>
    /// Returns a Tuple: (bool isValid, Vector3 position)
    /// </summary>
    public static (bool isValid, Vector3 position) GetMousePosition()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return (false, Vector3.zero);
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        // LayerMask is optional but recommended to ignore UI or Characters
        if (Physics.Raycast(ray, out RaycastHit hit)) 
        {
            Vector3 rawPoint = hit.point;

            // --- 1. Grid Snapping Logic ---
            // We round the X and Z based on the gridSize
            float snappedX = Mathf.Round(rawPoint.x / gridSize) * gridSize;
            float snappedZ = Mathf.Round(rawPoint.z / gridSize) * gridSize;

            Vector3 finalPos = new Vector3(snappedX, 0, snappedZ);

            // --- 2. NavMesh Validation Logic ---
            // SamplePosition checks if 'finalPos' is close enough to the NavMesh
            NavMeshHit navHit;
            bool hasNavMesh = NavMesh.SamplePosition(finalPos, out navHit, navMeshCheckRadius, NavMesh.AllAreas);

            // Only valid if we found a NavMesh point nearby
            return (hasNavMesh, finalPos);
        }

        return (false, Vector3.zero);
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

        // Security Check: Ideally, you should also check NavMesh validity on the Server here 
        // to prevent cheaters from bypassing the client check.

        bool canSpendElixir = TrySpendElixir(character.cost);
        if (!canSpendElixir)
        {
            return;
        }

        // Instantiate prefab Networked
        CardController characterInstance = Instantiate(character.prefab, new Vector3(world.x, 0, world.y), Quaternion.identity);

        var networkObj = characterInstance.GetComponent<NetworkObject>();

        // Spawn
        networkObj.SpawnWithOwnership(rpcParams.Receive.SenderClientId);

        // Setup team and data
        characterInstance.SetTeam(Team.Value);
        characterInstance.SetData(character);

        characterInstance.Activate();
    }

    [ServerRpc]
    public void SetTeamServerRpc(int team)
    {
        Team.Value = team;
    }

    public void SelectCard(int index)
    {
        selectedCharacterIndex = index;
        HandleOnSelectedCardChanged?.Invoke(characters[index]);
    }

    public void SelectCard(string cardId)
    {
        int index = characters.IndexOf(cardId);
        if (index != -1)
        {
            selectedCharacterIndex = index;
            HandleOnSelectedCardChanged?.Invoke(cardId);
        }
    }

    public CardData[] GetDeck()
    {
        CardData[] deck = new CardData[characters.Count];
        for (int i = 0; i < characters.Count; i++)
        {
            deck[i] = GameInstance.Instance.cardsContainer.GetCardById(characters[i]);
        }
        return deck;
    }
}