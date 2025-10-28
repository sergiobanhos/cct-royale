using UnityEngine;
using UnityEngine.EventSystems;

public class UIPanelSlider : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Layout")]
    [SerializeField] private int panelCount = 3;       // quantidade total de telas
    [SerializeField] private float panelWidth = 1080f; // largura de cada painel (em px do Canvas)
    
    [Header("Motion")]
    [SerializeField] private float lerpSpeed = 10f;            // suavização (maior = mais rápido)
    [SerializeField] private float swipeVelocityThreshold = 1500f; // px/s para considerar “swipe”
    [SerializeField] private bool clampBounds = true;          // impedir passar do 1º/último painel

    private RectTransform rect;
    private float targetX;          // posição alvo do container (anchoredPosition.x)
    private int panelIndex = 0;     // painel atual
    private bool isDragging;

    // controle de drag
    private Vector2 dragStartPointer;
    private float dragStartTargetX;
    private float lastSampleX;
    private float lastSampleTime;
    private float releaseVelocity; // px/s

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        SetIndexImmediate(panelIndex); // posiciona sem animar
    }

    void Update()
    {
        // Lerp “crítico” (1 - e^(-k*dt)) para suavização consistente
        float t = 1f - Mathf.Exp(-lerpSpeed * Time.unscaledDeltaTime);
        float newX = Mathf.Lerp(rect.anchoredPosition.x, targetX, t);
        rect.anchoredPosition = new Vector2(newX, rect.anchoredPosition.y);
    }

    // ---------- Drag (touch/mouse) ----------
    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        dragStartPointer = eventData.position;
        dragStartTargetX = targetX;

        lastSampleX = eventData.position.x;
        lastSampleTime = Time.unscaledTime;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // mover o container seguindo o dedo (direita arrasta para direita)
        float deltaX = eventData.position.x - dragStartPointer.x;
        targetX = dragStartTargetX + deltaX;

        // amostrar velocidade
        float now = Time.unscaledTime;
        float dt = Mathf.Max(0.0001f, now - lastSampleTime);
        releaseVelocity = (eventData.position.x - lastSampleX) / dt; // px/s

        lastSampleX = eventData.position.x;
        lastSampleTime = now;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        int nearest = Mathf.RoundToInt(-targetX / panelWidth);

        // swipe rápido “empurra” um painel extra
        if (Mathf.Abs(releaseVelocity) > swipeVelocityThreshold)
        {
            nearest += (releaseVelocity < 0f) ? +1 : -1; // esquerda -> próximo painel à direita
        }

        GoTo(nearest);
    }

    // ---------- API pública ----------
    public void Next() => GoTo(panelIndex + 1);
    public void Prev() => GoTo(panelIndex - 1);

    public void GoTo(int index)
    {
        if (clampBounds)
            index = Mathf.Clamp(index, -1, panelCount - 2);

        panelIndex = index;
        targetX = -panelWidth * panelIndex; // mover container para a esquerda conforme índice
    }

    public void SetIndexImmediate(int index)
    {
        panelIndex = Mathf.Clamp(index, 0, panelCount - 1);
        targetX = -panelWidth * panelIndex;
        if (rect != null)
            rect.anchoredPosition = new Vector2(targetX, rect.anchoredPosition.y);
    }

    // ---------- Utilidades ----------
    // opcional: definir panelWidth automaticamente pela largura do viewport (ex: ScrollView)
    public void AutoWidthFrom(RectTransform reference)
    {
        panelWidth = reference.rect.width;
        SetIndexImmediate(panelIndex);
    }

    // opcional: atualizar quantidade de painéis dinamicamente
    public void SetPanelCount(int count, bool clampToRange = true)
    {
        panelCount = Mathf.Max(1, count);
        if (clampToRange) GoTo(panelIndex);
    }
}
