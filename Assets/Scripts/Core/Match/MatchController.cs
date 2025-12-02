using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System;
using CctRoyale.Server;

public class MatchController : NetworkBehaviour
{

    public static MatchController Instance;

    [Header("Configuração da Partida")]
    public float matchDuration = 180f; // 3 minutos
    public float doubleElixirTime = 90f;

    [Header("Estado da Partida")]
    public NetworkVariable<float> timeLeft = new NetworkVariable<float>();
    public NetworkVariable<bool> isDoubleElixir = new NetworkVariable<bool>();
    public NetworkVariable<MatchStatus> matchStatus = new NetworkVariable<MatchStatus>(MatchStatus.Waiting);
    public Action<int> OnMatchFinished;

    [Header("Players e torres")]
    public PlayerController team1PlayerController;
    public PlayerController team2PlayerController;
    public HealthComponent team1KingTower;
    public HealthComponent team1QueenTower1;
    public HealthComponent team1QueenTower2;
    public HealthComponent team2KingTower;
    public HealthComponent team2QueenTower1;
    public HealthComponent team2QueenTower2;

     private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            var player = client.PlayerObject.GetComponent<PlayerController>();
            if (player != null)
            {
                if (player.Team.Value == 0) team1PlayerController = player;
                else team2PlayerController = player;
            }
        }

        matchStatus.Value = MatchStatus.InProgress;

        // Inicia a partida no servidor
        timeLeft.Value = matchDuration;
        isDoubleElixir.Value = false;
        StartCoroutine(MatchLoop());
        InitTowers();
    }

    private void InitTowers()
    {
        team1KingTower.SetHealth(1000f);
        team1QueenTower1.SetHealth(500f);
        team1QueenTower2.SetHealth(500f);
        team2KingTower.SetHealth(1000f);
        team2QueenTower1.SetHealth(500f);
        team2QueenTower2.SetHealth(500f);

        team1KingTower.SetTeam(1);
        team1QueenTower1.SetTeam(1);
        team1QueenTower2.SetTeam(1);
        team2KingTower.SetTeam(2);
        team2QueenTower1.SetTeam(2);
        team2QueenTower2.SetTeam(2);

        team1KingTower.OnDeath += () => EndMatch();
        team2KingTower.OnDeath += () => EndMatch();
    }
    
    private IEnumerator MatchLoop()
    {
        while (timeLeft.Value > 0 && matchStatus.Value == MatchStatus.InProgress)
        {
            yield return new WaitForSeconds(1f);
            timeLeft.Value -= 1f;

            if (timeLeft.Value <= doubleElixirTime)
            {
                isDoubleElixir.Value = true;
                NotifyDoubleElixirClientRpc();
            }

        }

        EndMatch();
    }
    
    private int CalculateWinningTeam()
    {
        if (team1KingTower.IsDead())
        {
            return 2; // Time 2 vence
        }
        else if (team2KingTower.IsDead())
        {
            return 1; // Time 1 vence
        }
        else
        {
            int team1TowersAlive = 0;
            if (!team1KingTower.IsDead()) team1TowersAlive++;
            if (!team1QueenTower1.IsDead()) team1TowersAlive++;
            if (!team1QueenTower2.IsDead()) team1TowersAlive++;

            int team2TowersAlive = 0;
            if (!team2KingTower.IsDead()) team2TowersAlive++;
            if (!team2QueenTower1.IsDead()) team2TowersAlive++;
            if (!team2QueenTower2.IsDead()) team2TowersAlive++;

            if (team1TowersAlive > team2TowersAlive)
                return 1; // Time 1 vence
            else if (team2TowersAlive > team1TowersAlive)
                return 2; // Time 2 vence
        }

        return -1; // Empate
    }
    
    private void EndMatch()
    {
        Debug.Log("Partida terminou!");
        matchStatus.Value = MatchStatus.Finished;
        EndMatchClientRpc(CalculateWinningTeam());
        GameServerManager.Instance?.FinishMatch();
    }

    [ClientRpc]
    private void NotifyDoubleElixirClientRpc()
    {
        Debug.Log("Double Elixir ativado!");
        // Aqui você pode acionar animações, sons ou UI
    }

    [ClientRpc]
    private void EndMatchClientRpc(int winningTeam)
    {
        Debug.Log($"Fim de partida. Vencedor: Time {winningTeam}");
        OnMatchFinished?.Invoke(winningTeam);

        // Aqui você pode atualizar a UI dos clientes
    }

    public static PlayerController GetLocalPlayer()
    {
        return NetworkManager.Singleton.ConnectedClientsList
            .FirstOrDefault(c => c.ClientId == NetworkManager.Singleton.LocalClientId)?
            .PlayerObject.GetComponent<PlayerController>();
    }

    public Vector3 GetSpellSpawnPositionForTeam(int team)
    {
        if (team == 1)
        {
            return team1KingTower.transform.position;
        }
        else if (team == 2)
        {
            return team2KingTower.transform.position;
        }

        return Vector3.zero;
    }
}

public enum MatchStatus 
{
    Waiting,
    InProgress,
    Finished
}
