using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TabsController : MonoBehaviour
{
    [System.Serializable]
    public class Tab
    {
        public string name;
        public Button button;
        public GameObject contentPanel;
        public Image background;
        public TextMeshProUGUI label;
    }

    [Header("Tabs")]
    public Tab[] tabs;

    [Header("Visual")]
    public Sprite activeSprite;
    public Sprite inactiveSprite;
    public Color activeTextColor = Color.white;
    public Color inactiveTextColor = Color.gray;

    private int _currentIndex = 0;

    void Awake()
    {
        // registra os cliques
        for (int i = 0; i < tabs.Length; i++)
        {
            int index = i;
            tabs[i].button.onClick.AddListener(() => OnTabClicked(index));
        }
    }

    void Start()
    {
        // começa na primeira aba
        SetActiveTab(_currentIndex);
    }

    void OnTabClicked(int index)
    {
        SetActiveTab(index);
    }

    void SetActiveTab(int index)
    {
        _currentIndex = index;

        for (int i = 0; i < tabs.Length; i++)
        {
            bool isActive = (i == index);

            // mostra/esconde painel
            if (tabs[i].contentPanel != null)
                tabs[i].contentPanel.SetActive(isActive);

            // troca sprite de fundo
            if (tabs[i].background != null)
                tabs[i].background.sprite = isActive ? activeSprite : inactiveSprite;

            // troca cor do texto
            if (tabs[i].label != null)
                tabs[i].label.color = isActive ? activeTextColor : inactiveTextColor;
        }
    }
}
