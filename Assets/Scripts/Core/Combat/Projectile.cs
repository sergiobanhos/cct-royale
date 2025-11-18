using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Transform target = null;
    private Action onHitTarget = null;
    
    public void SetTarget(Transform t)
    {
        target = t;
    }

    public void SetOnHitTarget(Action onHit)
    {
        onHitTarget = onHit;
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        float step = 30f * Time.deltaTime;

        Vector3 to = target.position;
        to.y = transform.position.y;
        transform.position = Vector3.MoveTowards(transform.position, to, step);

        if (Vector3.Distance(transform.position, to) < 0.1f)
        {
            onHitTarget?.Invoke();
            Destroy(gameObject);
        }
    }
}
