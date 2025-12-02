using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CardsContainerUI : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private CardsContainerItemUI cardItemPrefab;
    
    [Header("Containers")]
    [SerializeField] private Transform activeCardsContainer; // Container para as 3 cartas ativas
    [SerializeField] private Transform nextCardContainer;    // Container para a próxima carta
    
    [Header("References")]
    [SerializeField] private PlayerController playerController;

    [Header("Animation Settings")]
    [SerializeField] private float cardSwapDuration = 0.3f;
    [SerializeField] private Ease cardSwapEase = Ease.OutBack;

    private List<CardsContainerItemUI> activeCardItems = new List<CardsContainerItemUI>(3);
    private CardsContainerItemUI nextCardItem;

    private void OnEnable()
    {
        if (playerController != null)
        {
            // Inscreve nos eventos do PlayerController
            playerController.OnActiveCardsChanged += UpdateActiveCards;
            playerController.OnNextCardChanged += UpdateNextCard;
            playerController.OnSelectedCardChanged += UpdateSelectedCard;
        }
    }

    private void OnDisable()
    {
        if (playerController != null)
        {
            // Desinscreve dos eventos
            playerController.OnActiveCardsChanged -= UpdateActiveCards;
            playerController.OnNextCardChanged -= UpdateNextCard;
            playerController.OnSelectedCardChanged -= UpdateSelectedCard;
        }
    }

    private void Start()
    {
        InitializeUI();
    }

    /// <summary>
    /// Inicializa a UI criando os slots das cartas
    /// </summary>
    private void InitializeUI()
    {
        // Esconde o prefab
        cardItemPrefab.gameObject.SetActive(false);

        // Cria os 3 slots de cartas ativas
        for (int i = 0; i < 3; i++)
        {
            CardsContainerItemUI cardItem = Instantiate(cardItemPrefab, activeCardsContainer);
            cardItem.gameObject.SetActive(true);
            
            int index = i; // Captura o índice para o lambda
            cardItem.Initialize(null, (data) =>
            {
                playerController.SelectActiveCard(index);
            });
            
            activeCardItems.Add(cardItem);

            // Animação de entrada inicial
            cardItem.transform.localScale = Vector3.zero;
            cardItem.transform.DOScale(1f, 0.5f)
                .SetEase(Ease.OutBack)
                .SetDelay(i * 0.1f);
        }

        // Cria o slot da próxima carta
        nextCardItem = Instantiate(cardItemPrefab, nextCardContainer);
        nextCardItem.gameObject.SetActive(true);
        nextCardItem.Initialize(null, null); // Próxima carta não é clicável
        nextCardItem.SetAsNextCard(true); // Marca como próxima carta (para visual diferente)

        // Animação de entrada da próxima carta
        nextCardItem.transform.localScale = Vector3.zero;
        nextCardItem.transform.DOScale(0.85f, 0.5f)
            .SetEase(Ease.OutBack)
            .SetDelay(0.4f);

        // Carrega os dados iniciais
        LoadInitialCards();
    }

    /// <summary>
    /// Carrega as cartas iniciais do PlayerController
    /// </summary>
    private void LoadInitialCards()
    {
        if (playerController == null) return;

        // Carrega as cartas ativas
        List<string> activeCardIds = playerController.GetActiveCards();
        for (int i = 0; i < activeCardIds.Count && i < activeCardItems.Count; i++)
        {
            CardData cardData = GameInstance.Instance.cardsContainer.GetCardById(activeCardIds[i]);
            activeCardItems[i].UpdateCardData(cardData, false); // Sem animação na inicialização
        }

        // Carrega a próxima carta
        string nextCardId = playerController.GetNextCard();
        if (!string.IsNullOrEmpty(nextCardId))
        {
            CardData nextCardData = GameInstance.Instance.cardsContainer.GetCardById(nextCardId);
            nextCardItem.UpdateCardData(nextCardData, false);
        }

        // Atualiza a seleção inicial
        UpdateSelectedCard(playerController.GetSelectedCardIndex());
    }

    /// <summary>
    /// Atualiza as cartas ativas quando o ciclo muda
    /// </summary>
    private void UpdateActiveCards(List<string> activeCardIds)
    {
        for (int i = 0; i < activeCardIds.Count && i < activeCardItems.Count; i++)
        {
            CardData cardData = GameInstance.Instance.cardsContainer.GetCardById(activeCardIds[i]);
            activeCardItems[i].UpdateCardData(cardData, true); // COM animação
        }
    }

    /// <summary>
    /// Atualiza a próxima carta quando o ciclo muda
    /// </summary>
    private void UpdateNextCard(string nextCardId)
    {
        if (!string.IsNullOrEmpty(nextCardId))
        {
            CardData cardData = GameInstance.Instance.cardsContainer.GetCardById(nextCardId);
            nextCardItem.UpdateCardData(cardData, true); // COM animação
        }
    }

    /// <summary>
    /// Atualiza qual carta está selecionada visualmente
    /// </summary>
    private void UpdateSelectedCard(int selectedIndex)
    {
        for (int i = 0; i < activeCardItems.Count; i++)
        {
            activeCardItems[i].SetSelected(i == selectedIndex);
        }
    }

    /// <summary>
    /// Define o PlayerController (útil se for inicializado dinamicamente)
    /// </summary>
    public void SetPlayerController(PlayerController controller)
    {
        // Remove listeners antigos
        if (playerController != null)
        {
            playerController.OnActiveCardsChanged -= UpdateActiveCards;
            playerController.OnNextCardChanged -= UpdateNextCard;
            playerController.OnSelectedCardChanged -= UpdateSelectedCard;
        }

        playerController = controller;

        // Adiciona novos listeners
        if (playerController != null)
        {
            playerController.OnActiveCardsChanged += UpdateActiveCards;
            playerController.OnNextCardChanged += UpdateNextCard;
            playerController.OnSelectedCardChanged += UpdateSelectedCard;

            LoadInitialCards();
        }
    }
}
