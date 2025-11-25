using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MatchControllerUI : MonoBehaviour
{
    public TMPro.TextMeshProUGUI timerText;
    public GameObject doubleElixirIndicator;
    public TMPro.TextMeshProUGUI matchStatusText;

    [Header("Finished Match Panel")]
    [SerializeField] private GameObject finishedMatchPanel;
    [SerializeField] private TMPro.TextMeshProUGUI finishedMatchText;
    [SerializeField] private Button finishedMatchButton;

    private void Start()
    {
        if (MatchController.Instance != null)
        {
            // Atualiza sempre que o tempo mudar
            MatchController.Instance.timeLeft.OnValueChanged += UpdateTimer;
            MatchController.Instance.isDoubleElixir.OnValueChanged += UpdateDoubleElixir;
            MatchController.Instance.matchStatus.OnValueChanged += UpdateMatchStatus;
            MatchController.Instance.OnMatchFinished += OnMatchFinished;

            // Atualiza UI inicial
            UpdateTimer(0, MatchController.Instance.timeLeft.Value);
            UpdateDoubleElixir(false, MatchController.Instance.isDoubleElixir.Value);
            UpdateMatchStatus(MatchStatus.Waiting, MatchController.Instance.matchStatus.Value);
        }
    }

    private void UpdateTimer(float oldValue, float newValue)
    {
        int minutes = Mathf.FloorToInt(newValue / 60f);
        int seconds = Mathf.FloorToInt(newValue % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    private void UpdateDoubleElixir(bool oldValue, bool newValue)
    {
        doubleElixirIndicator.SetActive(newValue);
    }

    private void UpdateMatchStatus(MatchStatus oldValue, MatchStatus newValue)
    {
        switch (newValue)
        {
            case MatchStatus.Waiting:
                matchStatusText.text = "Aguardando jogadores";
                break;
            case MatchStatus.InProgress:
                matchStatusText.text = "Partida em andamento";
                break;
            case MatchStatus.Finished:
                matchStatusText.text = "Partida finalizada";
                break;
        }
    }

    private void OnMatchFinished(int winningTeam)
    {
        


       StartCoroutine(OnMatchFinishedCoroutine(winningTeam));
    }

    private IEnumerator OnMatchFinishedCoroutine(int winningTeam)
    {
        this.timerText.gameObject.SetActive(false);
        this.doubleElixirIndicator.SetActive(false);
        this.matchStatusText.gameObject.SetActive(false);


        yield return new WaitForSeconds(2.5f); // Pequeno delay para garantir que a UI seja atualizada corretamente

        finishedMatchPanel.SetActive(true);
        finishedMatchButton.gameObject.SetActive(true);
        finishedMatchButton.onClick.AddListener(() =>
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("MenuScene");
        });
        if (winningTeam == MatchController.GetLocalPlayer().Team.Value)
        {
            finishedMatchText.text = "Voce venceu!";
        }
        else
        {
            finishedMatchText.text = "Voce perdeu!";
        }
    }

    private void OnDestroy()
    {
        if (MatchController.Instance != null)
        {
            MatchController.Instance.timeLeft.OnValueChanged -= UpdateTimer;
            MatchController.Instance.isDoubleElixir.OnValueChanged -= UpdateDoubleElixir;
            MatchController.Instance.matchStatus.OnValueChanged -= UpdateMatchStatus;
        }
    }
}
