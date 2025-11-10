using UnityEngine;
using UnityEngine.UI;

public class MatchControllerUI : MonoBehaviour
{
    public TMPro.TextMeshProUGUI timerText;
    public GameObject doubleElixirIndicator;
    public TMPro.TextMeshProUGUI matchStatusText;

    private void Start()
    {
        if (MatchController.Instance != null)
        {
            // Atualiza sempre que o tempo mudar
            MatchController.Instance.timeLeft.OnValueChanged += UpdateTimer;
            MatchController.Instance.isDoubleElixir.OnValueChanged += UpdateDoubleElixir;
            MatchController.Instance.matchStarted.OnValueChanged += UpdateMatchStatus;

            // Atualiza UI inicial
            UpdateTimer(0, MatchController.Instance.timeLeft.Value);
            UpdateDoubleElixir(false, MatchController.Instance.isDoubleElixir.Value);
            UpdateMatchStatus(false, MatchController.Instance.matchStarted.Value);
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

    private void UpdateMatchStatus(bool oldValue, bool newValue)
    {
        matchStatusText.text = newValue ? "Partida em andamento" : "Aguardando jogadores";
    }

    private void OnDestroy()
    {
        if (MatchController.Instance != null)
        {
            MatchController.Instance.timeLeft.OnValueChanged -= UpdateTimer;
            MatchController.Instance.isDoubleElixir.OnValueChanged -= UpdateDoubleElixir;
            MatchController.Instance.matchStarted.OnValueChanged -= UpdateMatchStatus;
        }
    }
}
