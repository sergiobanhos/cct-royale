using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControllerUI : NetworkBehaviour
{
    [SerializeField] private PlayerController playerController;

    [Header("Elixir UI")]
    [SerializeField] private Slider elixirSlider;
    [SerializeField] private TMPro.TextMeshProUGUI elixirText;


    private void Start()
    {
        if (!IsOwner)
        {
            this.gameObject.SetActive(false);
            return;
        }

        this.playerController.Elixir.OnValueChanged += OnElixirChanged;
        elixirSlider.maxValue = playerController.maxElixir;
        
    } 

    private void OnElixirChanged(float oldValue, float newValue)
    {
        elixirSlider.value = newValue;
        elixirText.text = Mathf.FloorToInt(newValue).ToString();
    }
}
