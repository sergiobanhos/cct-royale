
using System;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private LayerMask enemyLayer; // IMPORTANTE: Defina isso no Inspector (Layer das Tropas/Torres)
    private HealthComponent currentTarget;
    private float attackTimer = 0.0f;
    private CharacterState currentState = CharacterState.Idle;

    [Header("Movement - Network Sync")]
    private NetworkVariable<float> networkSpeed = new NetworkVariable<float>(
        0f, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server
    );

    // Otimização: Não buscar alvo todo frame
    private float targetSearchTimer = 0f;
    private const float TARGET_SEARCH_INTERVAL = 0.25f; // Busca alvo 4x por segundo
    private const float PATH_UPDATE_INTERVAL = 0.2f;   // Atualiza rota 5x por segundo
    private float pathTimer = 0f;

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

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer)
        {
            // CLIENTE: Desliga o cérebro (Agent) e vira apenas visual
            navMeshAgent.enabled = false;
            // Se tiver Rigidbody, torne isKinematic = true aqui também
        }
        else
        {
            // SERVIDOR: Garante que o boneco comece no lugar certo do NavMesh
            // Se o spawn point for um pouco acima do chão, o Warp corrige
            if (navMeshAgent.isOnNavMesh)
            {
                navMeshAgent.Warp(transform.position);
            }
        }
    }

    // Método chamado pelo PlayerController após o Spawn para "ligar" a IA
    public override void Activate() 
    {
        if (IsServer && navMeshAgent != null)
        {
            this.healthComponent.SetHealth(this.cardData.stats.health);

            navMeshAgent.enabled = true;
            // Tenta snapar pro NavMesh mais próximo caso tenha nascido no ar
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 2.0f, NavMesh.AllAreas))
            {
                navMeshAgent.Warp(hit.position);
            }

            FindNewTarget();
        }
    }

    private void Update()
    {
        if (!IsServer) return;

        // Se o jogo acabou, para tudo
        if (MatchController.Instance != null && MatchController.Instance.matchStatus.Value != MatchStatus.InProgress)
        {
            if (navMeshAgent.isActiveAndEnabled) navMeshAgent.isStopped = true;
            return;
        }

        // Sincroniza animação
        if (navMeshAgent.isActiveAndEnabled)
        {
            // Usamos velocity do agent, não desiredVelocity, para evitar deslizes
            Vector3 localVelocity = transform.InverseTransformDirection(navMeshAgent.velocity);
            networkSpeed.Value = localVelocity.z;
        }

        // State Machine
        switch (currentState)
        {
            case CharacterState.Idle:
                IdleBehavior();
                break;
            case CharacterState.Moving:
                MovingBehavior();
                break;
            case CharacterState.Attacking:
                AttackingBehavior();
                break;
        }
    }

    private void IdleBehavior()
    {
        // Só busca alvo de tempos em tempos para economizar CPU
        targetSearchTimer += Time.deltaTime;
        if (targetSearchTimer >= TARGET_SEARCH_INTERVAL)
        {
            targetSearchTimer = 0f;
            FindNewTarget();
            if (currentTarget != null) {
                currentState = CharacterState.Moving;
                LookToMovement();
            }
        }
    }

    private void FindNewTarget()
    {
        HealthComponent newTarget = GetNearestTarget();

        if (newTarget == null)
        {
            currentState = CharacterState.Idle;
            currentTarget = null;
            navMeshAgent.isStopped = true;
            return;
        }

        if (newTarget != currentTarget)
        {
            currentTarget = newTarget;
            if (navMeshAgent.isActiveAndEnabled)
            {
                navMeshAgent.isStopped = false;
                navMeshAgent.speed = cardStats.moveSpeed;
                navMeshAgent.SetDestination(currentTarget.GetPosition());
            }

            currentState = CharacterState.Moving;
        }
    }

    private void MovingBehavior()
    {
        if (currentTarget == null)
        {
            currentState = CharacterState.Idle;
            navMeshAgent.isStopped = true;
            return;
        }

        IdleBehavior();
        LookToMovement();
        

        float distance = Vector3.Distance(transform.position, currentTarget.GetPosition());

        // Chegou no alcance de ataque?
        if (distance <= cardStats.attackRange)
        {
            currentState = CharacterState.Attacking;
            navMeshAgent.isStopped = true;
            return;
        }

        // // OTIMIZAÇÃO: Não chame SetDestination todo frame!
        // // O NavMeshAgent sabe seguir um alvo em movimento se você atualizar periodicamente
        // pathTimer += Time.deltaTime;
        // if (pathTimer >= PATH_UPDATE_INTERVAL)
        // {
        //     pathTimer = 0f;
        //     if (navMeshAgent.isActiveAndEnabled)
        //     {
        //         navMeshAgent.isStopped = false;
        //         navMeshAgent.speed = cardStats.moveSpeed;
        //         navMeshAgent.SetDestination(currentTarget.GetPosition());
        //     }
        // }
    }

    private void AttackingBehavior()
    {
        if (currentTarget == null)
        {
            currentState = CharacterState.Idle;
            return;
        }

        float distance = Vector3.Distance(transform.position, currentTarget.GetPosition());

        // Alvo fugiu? Volta a perseguir
        // Adicionamos um pequeno buffer (0.5f) para evitar ficar trocando entre Attack/Move freneticamente na borda do range
        if (distance > cardStats.attackRange + 0.5f)
        {
            currentState = CharacterState.Moving;
            return;
        }

        LookToTarget();

        attackTimer += Time.deltaTime;
        if (attackTimer >= 1f / cardStats.attackRate)
        {
            attackTimer = 0f;
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        if (this.cardData.isRanged)
        {   
            // Attack();
            PlayAttackAnimation();
        }
        else
        {
            PlayAttackAnimation();
             // Dano corpo a corpo precisa ser aplicado aqui ou via evento da animação
             // Sugestão: currentTarget.TakeDamage(...)
        }
    }

    public void Attack()
    {
        if (!IsServer)
        {
            return;
        }

        this.cardData.BaseAttack(this, currentTarget);
    }
    

    private void PlayAttackAnimation()
    {
        OnAttack?.Invoke();
        PlayAttackAnimationClientRpc();
    }

    [ClientRpc]
    private void PlayAttackAnimationClientRpc()
    {
        OnAttack?.Invoke();
    }
    
    // OTIMIZADO: Usa Physics.OverlapSphereNonAlloc para não gerar lixo de memória
    private Collider[] targetBuffer = new Collider[20]; // Reusa esse array sempre
    
    private HealthComponent GetNearestTarget()
    {
        HealthComponent nearest = null;
        float nearestDist = Mathf.Infinity;
        var allTargets = FindObjectsByType<HealthComponent>(FindObjectsSortMode.None);

        foreach (var target in allTargets)
        {
            if (target.team.Value == team.Value) continue;
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

        Vector3 direction = (currentTarget.GetPosition() - transform.position).normalized;
        direction.y = 0; // Garante que não olha pra cima/baixo

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            // Rotação suave em vez de instantânea
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
        }
    }

    private void LookToMovement()
    {
        Vector3 velocity = navMeshAgent.desiredVelocity;
        velocity.y = 0;

        if (velocity != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(velocity.normalized);
            // Rotação suave em vez de instantânea
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
        }
    }

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
