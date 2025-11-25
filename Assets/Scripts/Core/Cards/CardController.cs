using Unity.Netcode;
using UnityEngine;

public abstract class CardController : NetworkBehaviour
{
    protected NetworkVariable<int> team = new NetworkVariable<int>();

    public abstract CardData GetData();
    public abstract void SetData(CardData data);

    public virtual void SetTeam(int t) {
        team.Value = t;
    }

    public int GetTeam() {
        return team.Value;
    }

    public virtual void Activate() { }
}

public class CardController<TCardData> : CardController where TCardData : CardData
{

    [Header("Components")]
    [SerializeField] public TCardData cardData;
    protected CardStats cardStats;


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }


    public override void SetData(CardData data)
    {
        this.cardData = data as TCardData;
        this.cardStats = data.stats;
    }

    public override CardData GetData()
    {
        return cardData;
    }
}
