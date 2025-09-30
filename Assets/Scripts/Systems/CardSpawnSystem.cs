// Scripts/Systems/CardSpawnSystem.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using Core;

public class CardSpawnSystem : MonoBehaviour
{
    [Header("Prefabs por kind")]
    public GameObject giantPrefab;
    public GameObject magePrefab;
    public GameObject minionPrefab;

    private readonly Dictionary<string, GameObject> _spawned = new();
    private IDisposable _cardPlacedSub;

    void OnEnable()
    {
        _cardPlacedSub = EventBus.Subscribe<CardPlacedEvent>(OnCardPlaced);
    }
    void OnDisable() { _cardPlacedSub?.Dispose(); }

    void OnCardPlaced(CardPlacedEvent evt)
    {
        GameInstance.Instance.charactersContainer.GetCharacterById(evt.CardId).Spawn(evt.World);
    }

    GameObject InstantiatePrefabFor(string kind)
    {
        GameObject prefab = kind switch
        {
            "giant"  => giantPrefab,
            "mage"   => magePrefab,
            "minion" => minionPrefab,
            _        => minionPrefab
        };
        return Instantiate(prefab);
    }
}
