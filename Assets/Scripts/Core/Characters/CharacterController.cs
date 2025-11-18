using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(HealthComponent))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(NetworkObject))]
public class CharacterController : NetworkBehaviour
{
    [Header("Components")]
    [SerializeField] public CharacterData characterData;
    private CharacterStats characterStats;
    [SerializeField] private HealthComponent healthComponent;
    [SerializeField] private NavMeshAgent navMeshAgent;

    [Header("Combat")]
    private HealthComponent currentTarget;
    private float attackTimer = 0.0f;
    private CharacterState currentState = CharacterState.Idle;

    [Header("Movement")]
    private Vector3 lastPosition;

    private ulong ownerId;
    private int team;

    public event Action OnAttack;

    public void SetOwnerId(ulong clientId) => ownerId = clientId;
    public void SetTeam(int t) => team = t;

    private void Awake()
    {
        if (healthComponent == null) healthComponent = GetComponent<HealthComponent>();
        if (navMeshAgent == null) navMeshAgent = GetComponent<NavMeshAgent>();
        if (characterData) characterStats = characterData.stats;
    }

    private void Update()
    {
        if (!IsServer) return; // movimentação e ataque só no servidor

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
        navMeshAgent.speed = characterStats.moveSpeed;
        navMeshAgent.SetDestination(currentTarget.GetPosition());

        if (Vector3.Distance(transform.position, currentTarget.GetPosition()) <= characterStats.attackRange)
        {
            currentState = CharacterState.Attacking;
        }
    }

    private void HandleAttackState()
    {
        if (currentTarget == null || Vector3.Distance(transform.position, currentTarget.GetPosition()) > characterStats.attackRange)
        {
            currentState = CharacterState.Moving;
            return;
        }

        LookToTarget();

        navMeshAgent.isStopped = true;
        navMeshAgent.speed = 0f;
        attackTimer += Time.deltaTime;

        if (attackTimer >= 1f / characterStats.attackRate)
        {
            // currentTarget.TakeDamage(Mathf.RoundToInt(characterStats.attackDamage));
            attackTimer = 0f;
            AttackRpc();
        }
    }

    public void HandleAttackTarget()
    {
        this.characterData.BaseAttack(this, currentTarget);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void AttackRpc()
    {
        OnAttack?.Invoke();
    }

    private HealthComponent GetNearestTarget()
    {
        HealthComponent nearest = null;
        float nearestDist = Mathf.Infinity;
        var allTargets = FindObjectsByType<HealthComponent>(FindObjectsSortMode.None);

        foreach (var target in allTargets)
        {
            if (target.team == team) continue; // ignora aliados
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

    public Vector3 GetVelocity()
    {
        Vector3 velocity = (transform.position - lastPosition) / Time.deltaTime;
        lastPosition = transform.position;

        return transform.InverseTransformDirection(velocity);
    }
}

enum CharacterState
{
    Idle,
    Moving,
    Attacking
}
