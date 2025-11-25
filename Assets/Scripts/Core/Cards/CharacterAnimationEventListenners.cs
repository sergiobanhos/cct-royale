using System;
using UnityEngine;

public class CharacterAnimationEventListenners : MonoBehaviour
{

    public Action OnHit;
    public void Hit()
    {
        Debug.Log("Evento de animação 'Hit' chamado!");
        OnHit?.Invoke();
        // Aqui você pode aplicar dano, tocar som, emitir partículas, etc.
    }
}
