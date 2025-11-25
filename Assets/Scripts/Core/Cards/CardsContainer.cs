using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardsContainer", menuName = "CardsContainer")]
public class CardsContainer : ScriptableObject
{
    [SerializeField] private List<CardData> cards = new List<CardData>();

    public CardData GetCardById(string cardId)
    {
        return this.cards.Find(c => c.id == cardId);
    }
}
