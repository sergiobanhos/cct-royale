// Scripts/Systems/CardSpawnSystem.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using Core;

public class CardSpawnSystem : MonoBehaviour
{

    private readonly Dictionary<string, GameObject> _spawned = new();
    private IDisposable _cardPlacedSub;

    void OnEnable()
    {
        _cardPlacedSub = EventBus.Subscribe<CardPlacedEvent>(OnCardPlaced);
    }
    void OnDisable() { _cardPlacedSub?.Dispose(); }

    void OnCardPlaced(CardPlacedEvent evt)
    {
        GameInstance.Instance.charactersContainer.GetCharacterById(evt.CardId).Spawn(evt.World, evt.SenderId);
    }
}
