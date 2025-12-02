using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CardsContainerItemUI : MonoBehaviour
{
    [Header("Visual Components")]
    [SerializeField] private Image cardImage;
    [SerializeField] private Button cardButton;
    [SerializeField] private Image selectionOutline; // Borda quando selecionada
    [SerializeField] private TextMeshProUGUI costText; // Custo de elixir
    [SerializeField] private GameObject nextCardOverlay; // Overlay para indicar "próxima carta"
    [SerializeField] private CanvasGroup canvasGroup; // Para fade in/out

    [Header("Colors")]
    [SerializeField] private Color selectedColor = Color.yellow;
    [SerializeField] private Color normalColor = Color.white;

    [Header("Animation Settings")]
    [SerializeField] private float selectionScaleMultiplier = 1.15f;
    [SerializeField] private float selectionDuration = 0.2f;
    [SerializeField] private float cardSwapDuration = 0.3f;
    [SerializeField] private Ease selectionEase = Ease.OutBack;
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float hoverDuration = 0.15f;

    private CardData currentCardData;
    private Action<CardData> onClickCallback;
    private bool isSelected = false;
    private bool isNextCard = false;
    private Sequence currentAnimation;

    private void Awake()
    {
        // Inicializa o CanvasGroup se não existir
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
    }

    /// <summary>
    /// Inicializa o item de carta
    /// </summary>
    public void Initialize(CardData cardData, Action<CardData> onClickAction = null)
    {
        onClickCallback = onClickAction;

        if (cardButton != null)
        {
            cardButton.onClick.RemoveAllListeners();
            cardButton.onClick.AddListener(OnCardClicked);

            // Adiciona eventos de hover
            var trigger = cardButton.gameObject.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (trigger == null)
            {
                trigger = cardButton.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            }

            // Hover Enter
            var entryEvent = new UnityEngine.EventSystems.EventTrigger.Entry();
            entryEvent.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
            entryEvent.callback.AddListener((data) => { OnHoverEnter(); });
            trigger.triggers.Add(entryEvent);

            // Hover Exit
            var exitEvent = new UnityEngine.EventSystems.EventTrigger.Entry();
            exitEvent.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
            exitEvent.callback.AddListener((data) => { OnHoverExit(); });
            trigger.triggers.Add(exitEvent);
        }

        if (cardData != null)
        {
            UpdateCardData(cardData, false);
        }

        SetSelected(false);
        SetAsNextCard(false);
    }

    /// <summary>
    /// Atualiza os dados da carta exibida
    /// </summary>
    public void UpdateCardData(CardData cardData, bool animated = true)
    {
        currentCardData = cardData;

        if (cardData == null) return;

        if (animated)
        {
            PlayCardSwapAnimation(cardData);
        }
        else
        {
            UpdateCardVisuals(cardData);
        }
    }

    /// <summary>
    /// Atualiza os visuais da carta sem animação
    /// </summary>
    private void UpdateCardVisuals(CardData cardData)
    {
        // Atualiza a imagem
        if (cardImage != null)
        {
            cardImage.sprite = cardData.sprite;
            cardImage.preserveAspect = true;
        }

        // Atualiza o custo
        if (costText != null)
        {
            costText.text = cardData.cost.ToString();
        }
    }

    /// <summary>
    /// Animação de troca de carta
    /// </summary>
    private void PlayCardSwapAnimation(CardData cardData)
    {
        // Mata animação anterior se existir
        currentAnimation?.Kill();

        currentAnimation = DOTween.Sequence();

        // Fase 1: Fade out e reduz escala
        currentAnimation.Append(canvasGroup.DOFade(0f, cardSwapDuration * 0.4f));
        currentAnimation.Join(transform.DOScale(Vector3.one * 0.8f, cardSwapDuration * 0.4f));

        // Fase 2: Troca a carta
        currentAnimation.AppendCallback(() => UpdateCardVisuals(cardData));

        // Fase 3: Fade in e volta escala
        float targetScale = isSelected ? selectionScaleMultiplier : (isNextCard ? 0.85f : 1f);
        currentAnimation.Append(canvasGroup.DOFade(1f, cardSwapDuration * 0.6f));
        currentAnimation.Join(transform.DOScale(Vector3.one * targetScale, cardSwapDuration * 0.6f)
            .SetEase(Ease.OutBack));
    }

    /// <summary>
    /// Define se esta carta está selecionada
    /// </summary>
    public void SetSelected(bool selected)
    {
        if (isSelected == selected || isNextCard) return;

        isSelected = selected;

        // Mata animação anterior
        currentAnimation?.Kill();

        // Anima a borda de seleção
        if (selectionOutline != null)
        {
            selectionOutline.gameObject.SetActive(true);
            selectionOutline.color = selected ? selectedColor : normalColor;
            
            if (selected)
            {
                selectionOutline.DOFade(1f, selectionDuration);
                // Pulsa a borda
                selectionOutline.transform.DOScale(1.1f, 0.5f)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }
            else
            {
                selectionOutline.DOFade(0f, selectionDuration)
                    .OnComplete(() => selectionOutline.gameObject.SetActive(false));
                selectionOutline.transform.DOKill();
                selectionOutline.transform.localScale = Vector3.one;
            }
        }

        // Anima a escala da carta
        float targetScale = selected ? selectionScaleMultiplier : 1f;
        transform.DOScale(targetScale, selectionDuration)
            .SetEase(selectionEase);

        // Efeito de "punch" quando seleciona
        if (selected)
        {
            transform.DOPunchRotation(new Vector3(0, 0, 5f), selectionDuration * 0.5f, 10, 1f);
        }
    }

    /// <summary>
    /// Define se esta é a próxima carta (visual diferenciado)
    /// </summary>
    public void SetAsNextCard(bool isNext)
    {
        isNextCard = isNext;

        if (nextCardOverlay != null)
        {
            nextCardOverlay.SetActive(isNext);
        }

        // Próxima carta não deve ser clicável
        if (cardButton != null)
        {
            cardButton.interactable = !isNext;
        }

        // Ajusta a escala
        float targetScale = isNext ? 0.85f : 1f;
        transform.DOScale(targetScale, 0.3f).SetEase(Ease.OutBack);
    }

    /// <summary>
    /// Callback quando a carta é clicada
    /// </summary>
    private void OnCardClicked()
    {
        if (currentCardData != null && !isNextCard)
        {
            // Animação de clique
            transform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 5, 0.5f);
            
            onClickCallback?.Invoke(currentCardData);
        }
    }

    /// <summary>
    /// Quando o mouse entra na carta
    /// </summary>
    private void OnHoverEnter()
    {
        if (isNextCard || isSelected) return;

        // Pequena animação de hover
        transform.DOScale(hoverScale, hoverDuration).SetEase(Ease.OutQuad);
        
        // Leve rotação
        transform.DORotate(new Vector3(0, 0, 2f), hoverDuration);
    }

    /// <summary>
    /// Quando o mouse sai da carta
    /// </summary>
    private void OnHoverExit()
    {
        if (isNextCard || isSelected) return;

        // Volta ao normal
        transform.DOScale(1f, hoverDuration).SetEase(Ease.OutQuad);
        transform.DORotate(Vector3.zero, hoverDuration);
    }

    /// <summary>
    /// Retorna a carta atual
    /// </summary>
    public CardData GetCardData()
    {
        return currentCardData;
    }

    private void OnDestroy()
    {
        // Limpa todas as animações ao destruir
        currentAnimation?.Kill();
        transform.DOKill();
        if (selectionOutline != null)
        {
            selectionOutline.transform.DOKill();
        }
    }
}