using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(HealthComponent))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NetworkObject))]
public class TroopController : CardController<TroopCardData>
{
    [Header("Components")]
    [SerializeField] private HealthComponent healthComponent;
    [SerializeField] private NavMeshAgent navMeshAgent;

    [Header("Combat")]
    private HealthComponent currentTarget;
    private float attackTimer = 0.0f;
    private CharacterState currentState = CharacterState.Idle;

    [Header("Movement - Network Sync")]
    private NetworkVariable<float> networkSpeed = new NetworkVariable<float>(
        0f, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    );

    public event Action OnAttack;

    public override void SetTeam(int t) {
        base.SetTeam(t);
        healthComponent.SetTeam(t);
    }

    private void Awake()
    {
        if (healthComponent == null) healthComponent = GetComponent<HealthComponent>();
        if (navMeshAgent == null) navMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (IsServer)
        {
            Vector3 localVelocity = transform.InverseTransformDirection(navMeshAgent.desiredVelocity);
            networkSpeed.Value = localVelocity.z;

            if (MatchController.Instance.matchStatus.Value != MatchStatus.InProgress)
            {
                navMeshAgent.isStopped = true;
                navMeshAgent.speed = 0f;
                return;
            }

            switch (currentState)
            {
                case CharacterState.Idle:
                    Idle();
                    break;
                case CharacterState.Moving:
                    Moving();
                    break;
                case CharacterState.Attacking:
                    HandleAttackState();
                    break;
            }
        }
    }

    private void Idle()
    {
        currentTarget = GetNearestTarget();
        if (currentTarget != null) currentState = CharacterState.Moving;
    }

    private void Moving()
    {
        currentTarget = GetNearestTarget();

        if (currentTarget == null)
        {
            currentState = CharacterState.Idle;
            return;
        }

        navMeshAgent.isStopped = false;
        navMeshAgent.speed = cardStats.moveSpeed;
        navMeshAgent.SetDestination(currentTarget.GetPosition());

        if (Vector3.Distance(transform.position, currentTarget.GetPosition()) <= cardStats.attackRange)
        {
            currentState = CharacterState.Attacking;
        }
    }

    private void HandleAttackState()
    {
        if (currentTarget == null || Vector3.Distance(transform.position, currentTarget.GetPosition()) > cardStats.attackRange)
        {
            currentState = CharacterState.Moving;
            return;
        }

        LookToTarget();

        navMeshAgent.isStopped = true;
        navMeshAgent.speed = 0f;
        attackTimer += Time.deltaTime;

        if (attackTimer >= 1f / cardStats.attackRate)
        {
            attackTimer = 0f;
            // AttackRpc();
            if (this.cardData.isRanged)
            {   
                this.cardData.BaseAttack(this, currentTarget);
            }
            else
            {
                PlayAttackAnimation();
            }
        }
    }

    private void PlayAttackAnimation()
    {
        OnAttack?.Invoke();
    }

    public void HandleAttackTarget()
    {
        if (!IsServer) return;
        this.cardData.BaseAttack(this, currentTarget);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void AttackRpc()
    {
        OnAttack?.Invoke();
        Debug.Log("Attack RPC");
    }

    private HealthComponent GetNearestTarget()
    {
        HealthComponent nearest = null;
        float nearestDist = Mathf.Infinity;
        var allTargets = FindObjectsByType<HealthComponent>(FindObjectsSortMode.None);

        foreach (var target in allTargets)
        {
            if (target.team == team.Value) continue;
            if (target.gameObject == gameObject) continue;

            Vector3 targetPosition = target.GetPosition();
            Vector3 selfPosition = transform.position;

            targetPosition.y = selfPosition.y;

            float dist = (targetPosition - selfPosition).magnitude;
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = target;
            }
        }

        return nearest;
    }

    private void LookToTarget()
    {
        if (currentTarget == null) return;

        Vector3 targetPosition = currentTarget.GetPosition();
        targetPosition.y = transform.position.y;

        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction == Vector3.zero) return;

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = lookRotation;
    }

    // Retorna a velocidade sincronizada pela rede
    public float GetSpeed()
    {
        return networkSpeed.Value;
    }
}

enum CharacterState
{
    Idle,
    Moving,
    Attacking
}