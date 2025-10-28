using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(HealthComponent))]
[RequireComponent(typeof(NavMeshAgent))]
public class CharacterController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private CharacterData characterData;
    [SerializeField] private HealthComponent healthComponent;
    [SerializeField] private NavMeshAgent navMeshAgent;
    public HealthComponent HealthComponent => healthComponent;

    [Header("Combat")]
    private HealthComponent currentTarget;
    public HealthComponent CurrentTarget => currentTarget;
    private float attackTimer = 0.0f;

    private CharacterState currentState = CharacterState.Idle;

    private void Awake()
    {
        if (healthComponent == null)
        {
            healthComponent = GetComponent<HealthComponent>();
        }
    }

    private void Update()
    {
        switch (currentState)
        {
            case CharacterState.Idle:
                Idle();
                break;
            case CharacterState.Moving:
                Moving();
                break;
            case CharacterState.Attacking:
                Attack();
                break;
        }
       
    }

    private void Idle()
    {
        HealthComponent newTarget = GetNearestTarget();

        if (newTarget != currentTarget)
        {
            this.currentState = CharacterState.Moving;
        }
    }

    private void Moving()
    {
        HealthComponent newTarget = GetNearestTarget();

        if (newTarget != currentTarget)
        {
            currentTarget = newTarget;
            this.navMeshAgent.SetDestination(currentTarget != null ? currentTarget.GetPosition() : transform.position);
            this.navMeshAgent.speed = characterData.speed;
        }

        float distanceToTarget = GetDistanceToTarget();
        if (distanceToTarget <= navMeshAgent.stoppingDistance && currentTarget != null)
        {
            this.currentState = CharacterState.Attacking;
        }
    }
    
    private void Attack()
    {
        if (currentTarget != null)
        {
            // Perform attack on the current target
        }
    }


    private HealthComponent GetNearestTarget()
    {
        HealthComponent nearestTarget = null;
        float nearestDistanceSqr = Mathf.Infinity;
        Vector3 currentPosition = transform.position;
        HealthComponent[] allTargets = FindObjectsByType<HealthComponent>(FindObjectsSortMode.None);

        foreach (HealthComponent target in allTargets)
        {
            if (!target.isEnemy)
                continue;

            if (this.healthComponent.clientId == target.clientId)
                continue;

            if (target.gameObject == this.gameObject)
                continue;

            float distanceSqr = (target.GetPosition() - currentPosition).sqrMagnitude;
            if (distanceSqr < nearestDistanceSqr)
            {
                nearestDistanceSqr = distanceSqr;
                nearestTarget = target;
            }
        }

        return nearestTarget;
    }

    private float GetDistanceToTarget()
    {
        if (currentTarget != null)
        {
            float distance = Vector3.Distance(transform.position, currentTarget.GetPosition());
            return distance;
        }
        return 0;
    }   
}

enum CharacterState
{
    Idle,
    Moving,
    Attacking
}