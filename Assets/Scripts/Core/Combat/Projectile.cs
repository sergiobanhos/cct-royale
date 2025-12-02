using System;
using Unity.Netcode;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    private NetworkVariable<NetworkObjectReference> netTarget = new NetworkVariable<NetworkObjectReference>();
    private Transform targetTransform;
    private Action onHitTarget = null;
    
    public void SetTarget(Transform t)
    {
        if (IsServer)
        {
            if (t.TryGetComponent(out NetworkObject no))
            {
                netTarget.Value = no;
                targetTransform = t;
            }
            else
            {
                Debug.LogWarning("Projectile target does not have a NetworkObject!");
            }
        }
    }

    public void SetOnHitTarget(Action onHit)
    {
        onHitTarget = onHit;
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            ResolveTarget();
        }
    }

    private void ResolveTarget()
    {
        if (netTarget.Value.TryGet(out NetworkObject targetObj))
        {
            targetTransform = targetObj.transform;
        }
    }

    private void Update()
    {
        // Try to resolve target if we don't have it yet
        if (targetTransform == null)
        {
            ResolveTarget();
        }

        if (targetTransform == null)
        {
            // Only Server decides to destroy if target is missing/null
            if (IsServer)
            {
                GetComponent<NetworkObject>().Despawn();
            }
            return;
        }

        float step = 30f * Time.deltaTime;

        Vector3 to = targetTransform.position;
        to.y = transform.position.y;
        transform.position = Vector3.MoveTowards(transform.position, to, step);

        if (Vector3.Distance(transform.position, to) < 0.1f && IsServer)
        {
            onHitTarget?.Invoke();
            GetComponent<NetworkObject>().Despawn();
        }
    }
}
