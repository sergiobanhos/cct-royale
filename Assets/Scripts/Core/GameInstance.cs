using UnityEngine;
using Utils;

public class GameInstance : MonoSingleton<GameInstance>
{
    [SerializeField] private GameNetworkClient networkClient;
    public GameNetworkClient NetworkClient => networkClient;
    public CharactersContainer charactersContainer;

    [Header("Tower Settings")]
    [SerializeField] private HealthComponent[] playerTowers;
    [SerializeField] private HealthComponent[] enemyTowers;

    protected override void Awake()
    {
        base.Awake();

        if (networkClient == null)
        {
            networkClient = GetComponent<GameNetworkClient>();
        }

        if (charactersContainer == null)
        {
            charactersContainer = GetComponent<CharactersContainer>();
        }


    }

    private void InitializeTowers()
    {
        for (int i = 0; i < playerTowers.Length; i++)
        {
            playerTowers[i].SetClientId(networkClient.ClientId);
        }
    }
}
