// Scripts/Systems/EmoteSystem.cs
using UnityEngine;
using UnityEngine.UI;
using Core;
using System;

public class EmoteSystem : MonoBehaviour
{
    [Header("Referências de UI")]
    public Transform emoteLayer;        // canvas/layer onde os emotes aparecem
    public GameObject emotePrefab;      // prefab com Image/Text
    public float lifeTime = 1.5f;

    private IDisposable _sub;

    void OnEnable()
    {
        _sub = EventBus.Subscribe<EmoteReceivedEvent>(OnEmote);
    }
    
    void OnDisable()
    {
        _sub?.Dispose();
    }

    void OnEmote(EmoteReceivedEvent e)
    {
        // instanciar um emote na UI (poderia mapear posição por jogador, etc.)
        var go = Instantiate(emotePrefab, emoteLayer);
        var txt = go.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (txt) txt.text = e.Emoji;

        // opcional: marca o dono no nome do GO
        go.name = $"Emote_{e.SenderId}_{Guid.NewGuid().ToString("N").Substring(0,4)}";

        // animação/tempo de vida
        Destroy(go, lifeTime);
    }
}
