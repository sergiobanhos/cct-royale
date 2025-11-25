using UnityEngine;

[CreateAssetMenu(fileName = "CardData", menuName = "CardData", order = 0)]
public class CardData : ScriptableObject
{
    [Header("General")]
    public string id;
    public int cost;
    public float deployTime = 0.15f;
    public string name;
    public Sprite sprite;
    public CardController prefab = null;
    public GameObject previewPrefab = null;

    [Header("Stats")]
    public CardStats stats;


    public virtual CardController Spawn(Vector2 world, string SenderId)
    {
        throw new System.NotImplementedException();
    }

    public virtual CardType GetCardType()
    {
        throw new System.NotImplementedException();
    }
   
}

public enum CardType
{
    Troop,
    Spell,
    Building
}