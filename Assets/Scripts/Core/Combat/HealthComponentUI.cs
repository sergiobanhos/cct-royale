using UnityEngine;

public class HealthComponentUI : MonoBehaviour
{
    [SerializeField] private HealthComponent healthComponent;
    [SerializeField] private UnityEngine.UI.Image healthBarFill;

    private void Awake()
    {
        healthComponent.OnHealthChanged += UpdateHealthBar;
        healthComponent.OnTeamChanged += OnTeamChanged;

        healthComponent.team.OnValueChanged += (oldTeam, newTeam) => {
            OnTeamChanged(newTeam);
        };
    }

    private void Start()
    {
        if (healthComponent.GetTeam() == 0 || healthComponent.GetTeam() == 1)
        {
            healthBarFill.color = Color.blue; 
        }
        else if (healthComponent.GetTeam() == 2)
        {
            healthBarFill.color = Color.red;
        }
    }

    private void UpdateHealthBar(float healthPercent)
    {
        healthBarFill.fillAmount = healthPercent; 
    }

    private void OnTeamChanged(int team)
    {
        if (team == 0 || team == 1)
        {
            healthBarFill.color = Color.blue; 
        }
        else if (team == 2)
        {
            healthBarFill.color = Color.red;
        }
    }
}
