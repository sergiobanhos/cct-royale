using System;
using System.Collections.Generic;
using System.Linq;
using CctRoyale.Server;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using Utils;

public class PlayerController : NetworkBehaviour
{
    public NetworkVariable<int> Team = new NetworkVariable<int>();
    public NetworkVariable<float> Elixir = new NetworkVariable<float>(0f);

    [Header("Player Info")]
    [SerializeField] private List<string> deckCards = new List<string>(); // Deck completo (8 cartas)
    
    // Server-side cycle
    private Queue<string> cardCycle = new Queue<string>(); 

    // Networked State
    private NetworkList<FixedString64Bytes> netActiveCards;
    private NetworkVariable<FixedString64Bytes> netNextCard = new NetworkVariable<FixedString64Bytes>();
    
    public int selectedCardIndex = 0;
    private bool cardCycleInitialized = false;

    // Actions para UI
    public Action<List<string>> OnActiveCardsChanged; // Notifica mudança nas 3 cartas ativas
    public Action<string> OnNextCardChanged; // Notifica mudança na próxima carta
    public Action<int> OnSelectedCardChanged; // Notifica qual carta está selecionada (0-2)

    [Header("Configuração do Elixir")]
    public float maxElixir = 10f;
    public float elixirRegenRate = 1f;
    public float doubleElixirMultiplier = 2f;

    [Header("Grid & Placement Settings")]
    [SerializeField] private static float gridSize = 5.0f;
    [SerializeField] private static float navMeshCheckRadius = 0.5f;

    private void Awake()
    {
        netActiveCards = new NetworkList<FixedString64Bytes>();
    }

    private void OnEnable()
    {
        Team.OnValueChanged += OnTeamChanged;
        netActiveCards.OnListChanged += OnNetActiveCardsChanged;
        netNextCard.OnValueChanged += OnNetNextCardChanged;
    }

    private void OnDisable()
    {
        Team.OnValueChanged -= OnTeamChanged;
        netActiveCards.OnListChanged -= OnNetActiveCardsChanged;
        netNextCard.OnValueChanged -= OnNetNextCardChanged;
    }

    private void OnTeamChanged(int oldValue, int newValue)
    {
        Debug.Log($"Meu time mudou de {oldValue} para {newValue}");
    }

    private void OnNetActiveCardsChanged(NetworkListEvent<FixedString64Bytes> changeEvent)
    {
        // Converte NetworkList para List<string> e notifica a UI
        List<string> activeCardsList = new List<string>();
        foreach (var card in netActiveCards)
        {
            activeCardsList.Add(card.ToString());
        }
        OnActiveCardsChanged?.Invoke(activeCardsList);
        
        // Se a carta selecionada não for mais válida (ex: índice fora do range, embora aqui seja fixo em 3), ajusta
        if (selectedCardIndex >= activeCardsList.Count)
        {
            selectedCardIndex = activeCardsList.Count - 1;
            OnSelectedCardChanged?.Invoke(selectedCardIndex);
        }
    }

    private void OnNetNextCardChanged(FixedString64Bytes previousValue, FixedString64Bytes newValue)
    {
        OnNextCardChanged?.Invoke(newValue.ToString());
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            if (GameServerManager.Instance != null)
            {
                Team.Value = GameServerManager.Instance.AssignTeam(this);
            }
            InitializeCardCycle();
        }
        
        // Force UI update on spawn
        if (IsClient && IsOwner)
        {
             // Trigger initial UI update if data is already there
            if (netActiveCards.Count > 0)
            {
                List<string> activeCardsList = new List<string>();
                foreach (var card in netActiveCards)
                {
                    activeCardsList.Add(card.ToString());
                }
                OnActiveCardsChanged?.Invoke(activeCardsList);
            }
            if (!netNextCard.Value.IsEmpty)
            {
                OnNextCardChanged?.Invoke(netNextCard.Value.ToString());
            }
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

        // Visualization
        var mouseData = GetMousePosition();
        if (mouseData.isValid)
        {
            Debug.DrawLine(mouseData.position, mouseData.position + Vector3.up * 2, Color.green);
        }
        else
        {
            Debug.DrawLine(mouseData.position, mouseData.position + Vector3.up * 2, Color.red);
        }

        if (Input.GetMouseButtonDown(0))
        {
            MouseClick();
        }

        // Character Selection Input (agora só 3 cartas)
        if (Input.GetKeyDown(KeyCode.Alpha1)) SelectActiveCard(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SelectActiveCard(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SelectActiveCard(2);
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

    /// <summary>
    /// Inicializa o ciclo de cartas embaralhando o deck (Server Only)
    /// </summary>
    private void InitializeCardCycle()
    {
        if (cardCycleInitialized) return;
        
        if (deckCards.Count < 4)
        {
            Debug.LogError($"Deck precisa ter pelo menos 4 cartas! Atual: {deckCards.Count}");
            return;
        }

        Debug.Log($"Inicializando ciclo de cartas com {deckCards.Count} cartas");

        // Embaralha o deck
        List<string> shuffledDeck = deckCards.OrderBy(x => UnityEngine.Random.value).ToList();
        
        // Preenche o ciclo
        cardCycle.Clear();
        foreach (string card in shuffledDeck)
        {
            cardCycle.Enqueue(card);
        }
        
        // Pega as 3 primeiras cartas ativas
        netActiveCards.Clear();
        for (int i = 0; i < 3; i++)
        {
            string card = cardCycle.Dequeue();
            netActiveCards.Add(new FixedString64Bytes(card));
            Debug.Log($"Carta ativa {i}: {card}");
        }
        
        // Próxima carta
        string next = cardCycle.Dequeue();
        netNextCard.Value = new FixedString64Bytes(next);
        Debug.Log($"Próxima carta: {next}");
        
        cardCycleInitialized = true;
    }

    private void MouseClick()
    {
        // Client-side check for valid selection
        if (selectedCardIndex < 0 || selectedCardIndex >= netActiveCards.Count) return;

        var mouseData = GetMousePosition();

        if (mouseData.isValid)
        {
            Vector2 spawnPoint = new Vector2(mouseData.position.x, mouseData.position.z);
            SpawnCardServerRpc(selectedCardIndex, spawnPoint);
        }
        else
        {
            Debug.Log("Cannot place card here: Invalid Position or No NavMesh.");
        }
    }

    public static (bool isValid, Vector3 position) GetMousePosition()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return (false, Vector3.zero);
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit)) 
        {
            Vector3 rawPoint = hit.point;

            float snappedX = Mathf.Round(rawPoint.x / gridSize) * gridSize;
            float snappedZ = Mathf.Round(rawPoint.z / gridSize) * gridSize;

            Vector3 finalPos = new Vector3(snappedX, 0, snappedZ);

            NavMeshHit navHit;
            bool hasNavMesh = NavMesh.SamplePosition(finalPos, out navHit, navMeshCheckRadius, NavMesh.AllAreas);

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
    private void SpawnCardServerRpc(int activeIndex, Vector2 world, ServerRpcParams rpcParams = default)
    {
        if (activeIndex < 0 || activeIndex >= netActiveCards.Count)
        {
            Debug.LogError($"Índice de carta ativa inválido: {activeIndex}");
            return;
        }

        string characterId = netActiveCards[activeIndex].ToString();
        CardData character = GameInstance.Instance.cardsContainer.GetCardById(characterId);

        bool canSpendElixir = TrySpendElixir(character.cost);
        if (!canSpendElixir)
        {
            Debug.Log($"Elixir insuficiente! Necessário: {character.cost}, Atual: {Elixir.Value}");
            return;
        }

        // Delegate spawn logic to the card data
        character.ServerSpawn(world, rpcParams.Receive.SenderClientId, Team.Value);

        // Atualiza o ciclo de cartas no servidor
        UpdateCardCycleServer(activeIndex);
    }

    private void UpdateCardCycleServer(int activeIndex)
    {
        // Remove a carta usada (na verdade, substitui pela próxima)
        string usedCard = netActiveCards[activeIndex].ToString();
        string next = netNextCard.Value.ToString();
        
        // Atualiza a lista de ativas
        netActiveCards[activeIndex] = new FixedString64Bytes(next);
        
        // Adiciona a carta usada de volta ao ciclo
        cardCycle.Enqueue(usedCard);
        
        // Pega a nova próxima carta
        string newNext = cardCycle.Dequeue();
        netNextCard.Value = new FixedString64Bytes(newNext);
        
        Debug.Log($"Cycle Updated. Used: {usedCard}, New Active: {next}, New Next: {newNext}");
    }

    [ServerRpc]
    public void SetTeamServerRpc(int team)
    {
        Team.Value = team;
    }

    /// <summary>
    /// Seleciona uma carta ativa (0-2)
    /// </summary>
    public void SelectActiveCard(int index)
    {
        if (index < 0 || index >= netActiveCards.Count)
        {
            Debug.LogWarning($"Tentando selecionar carta inválida: {index}");
            return;
        }

        selectedCardIndex = index;
        OnSelectedCardChanged?.Invoke(selectedCardIndex);
    }

    // ========== MÉTODOS PÚBLICOS PARA UI ==========

    /// <summary>
    /// Retorna as 3 cartas ativas atuais
    /// </summary>
    public List<string> GetActiveCards()
    {
        List<string> list = new List<string>();
        foreach(var c in netActiveCards) list.Add(c.ToString());
        return list;
    }

    /// <summary>
    /// Retorna a próxima carta
    /// </summary>
    public string GetNextCard()
    {
        return netNextCard.Value.ToString();
    }

    /// <summary>
    /// Retorna o índice da carta selecionada (0-2)
    /// </summary>
    public int GetSelectedCardIndex()
    {
        return selectedCardIndex;
    }

    /// <summary>
    /// Retorna o ID da carta selecionada
    /// </summary>
    public string GetSelectedCardId()
    {
        if (selectedCardIndex >= 0 && selectedCardIndex < netActiveCards.Count)
        {
            return netActiveCards[selectedCardIndex].ToString();
        }
        return "";
    }

    /// <summary>
    /// Retorna os CardData das cartas ativas
    /// </summary>
    public CardData[] GetActiveCardsData()
    {
        CardData[] cards = new CardData[netActiveCards.Count];
        for (int i = 0; i < netActiveCards.Count; i++)
        {
            cards[i] = GameInstance.Instance.cardsContainer.GetCardById(netActiveCards[i].ToString());
        }
        return cards;
    }

    /// <summary>
    /// Retorna o CardData da próxima carta
    /// </summary>
    public CardData GetNextCardData()
    {
        return GameInstance.Instance.cardsContainer.GetCardById(netNextCard.Value.ToString());
    }

    /// <summary>
    /// Retorna o deck completo
    /// </summary>
    public CardData[] GetDeck()
    {
        CardData[] deck = new CardData[deckCards.Count];
        for (int i = 0; i < deckCards.Count; i++)
        {
            deck[i] = GameInstance.Instance.cardsContainer.GetCardById(deckCards[i]);
        }
        return deck;
    }
}