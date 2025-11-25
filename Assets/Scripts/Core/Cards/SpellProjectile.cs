using System;
using Unity.Netcode;
using UnityEngine;

public class SpellProjectile : NetworkBehaviour
{
    [SerializeField] private TrailRenderer trail;

    // Variaveis de Rede para sincronizar dados essenciais
    private readonly NetworkVariable<Vector3> netStartPosition = new NetworkVariable<Vector3>();
    private readonly NetworkVariable<Vector3> netTargetPosition = new NetworkVariable<Vector3>();
    private readonly NetworkVariable<float> netSpeed = new NetworkVariable<float>();
    private readonly NetworkVariable<float> netArcHeight = new NetworkVariable<float>();
    private readonly NetworkVariable<int> netTeam = new NetworkVariable<int>();

    // Variáveis locais para lógica (não precisam de rede pois são calculadas localmente)
    private float journeyLength;
    private float startTime;
    private bool hasImpacted = false;

    public event Action<Vector3> OnImpact;

    public void Initialize(Vector3 start, Vector3 target, float projectileSpeed, float arc, int teamId)
    {
        // Como Initialize roda ANTES do Spawn() no servidor, 
        // podemos settar os valores das NetworkVariables aqui.
        // Eles serão enviados automaticamente no pacote de Spawn para os clientes.
        netStartPosition.Value = start;
        netTargetPosition.Value = target;
        netSpeed.Value = projectileSpeed;
        netArcHeight.Value = arc;
        netTeam.Value = teamId;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Assim que o objeto nasce na rede (tanto server quanto client),
        // configuramos os dados locais baseados nas variáveis de rede.
        transform.position = netStartPosition.Value;
        
        journeyLength = Vector3.Distance(netStartPosition.Value, netTargetPosition.Value);
        
        // Se distance for muito pequena (erro de segurança), evitamos divisão por zero
        if (journeyLength < 0.01f) journeyLength = 0.01f;

        startTime = Time.time;

        // Calcula rotação inicial
        Vector3 direction = (netTargetPosition.Value - netStartPosition.Value).normalized;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    private void Update()
    {
        // Importante: Se não spawnou ainda, não roda lógica
        if (!IsSpawned || hasImpacted) return;

        // Calculate journey completion percentage
        float distCovered = (Time.time - startTime) * netSpeed.Value;
        
        // Prevenção extra contra divisão por zero
        float fraction = (journeyLength > 0) ? distCovered / journeyLength : 1f;
        
        if (fraction >= 1.0f)
        {
            Impact();
            return;
        }

        // Move along parabolic arc usando os valores da rede
        Vector3 currentPos = Vector3.Lerp(netStartPosition.Value, netTargetPosition.Value, fraction);

        // Add height based on sin curve
        currentPos.y = netStartPosition.Value.y + netArcHeight.Value * Mathf.Sin(fraction * Mathf.PI);

        transform.position = currentPos;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Só o servidor deve processar colisão de lógica (dano) para evitar cheaters/desync,
        // ou se for puramente visual, o cliente pode rodar, mas cuidado com duplicação.
        // Geralmente, mantemos autoridade no Server ou Dono.
        if (!IsServer) return;

        HealthComponent health = other.GetComponent<HealthComponent>();
        // Note o uso de netTeam.Value
        if (health != null && health.GetTeam() != netTeam.Value)
        {
            Impact();
        }
    }

    private void Impact()
    {
        if (hasImpacted) return;
        hasImpacted = true;

        // Dispara evento (Isso vai rodar no Server se chamado do Update no Server, ou Client se chamado no Client)
        // Idealmente, seu SpellController deve escutar isso apenas no Server.
        OnImpact?.Invoke(transform.position);

        if (trail != null)
        {
            trail.transform.SetParent(null);
            trail.autodestruct = true;
        }

        // Apenas o servidor pode destruir o NetworkObject
        if (IsServer)
        {
            Destroy(gameObject); 
        }
    }

    private void OnDrawGizmos()
    {
        // Visualização precisa usar os values ou fallbacks se for nulo (editor mode)
        Vector3 s = (netStartPosition != null) ? netStartPosition.Value : Vector3.zero;
        Vector3 t = (netTargetPosition != null) ? netTargetPosition.Value : Vector3.zero;
        float a = (netArcHeight != null) ? netArcHeight.Value : 0;

        if (Application.isPlaying && s != Vector3.zero && t != Vector3.zero)
        {
            Gizmos.color = Color.red;
            Vector3 prev = s;

            for (float step = 0; step <= 1.0f; step += 0.05f)
            {
                Vector3 pos = Vector3.Lerp(s, t, step);
                pos.y = s.y + a * Mathf.Sin(step * Mathf.PI);

                Gizmos.DrawLine(prev, pos);
                prev = pos;
            }
        }
    }
}