using UnityEngine;

public class HealthComponentUI : MonoBehaviour
{
    [SerializeField] private HealthComponent healthComponent;
    [SerializeField] private UnityEngine.UI.Image healthBarFill;

    private void Start()
    {
        healthComponent.OnHealthChanged += UpdateHealthBar;
    }

    private void UpdateHealthBar(float healthPercent)
    {
        healthBarFill.fillAmount = healthPercent; 
    }
}
