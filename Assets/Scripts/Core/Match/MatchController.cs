using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class MatchController : NetworkBehaviour
{

    public static MatchController Instance;

    [Header("Configuração da Partida")]
    public float matchDuration = 180f; // 3 minutos
    public float doubleElixirTime = 90f;

    [Header("Estado da Partida")]
    public NetworkVariable<float> timeLeft = new NetworkVariable<float>();
    public NetworkVariable<bool> isDoubleElixir = new NetworkVariable<bool>();
    public NetworkVariable<bool> matchStarted = new NetworkVariable<bool>(false);


    [Header("Players e torres")]
    public PlayerController team1PlayerController;
    public PlayerController team2PlayerController;

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

        matchStarted.Value = true;

        // Inicia a partida no servidor
        timeLeft.Value = matchDuration;
        isDoubleElixir.Value = false;
        StartCoroutine(MatchLoop());
    }

    private IEnumerator MatchLoop()
    {
        while (timeLeft.Value > 0)
        {
            yield return new WaitForSeconds(1f);
            timeLeft.Value -= 1f;

            if (timeLeft.Value <= doubleElixirTime)
            {
                isDoubleElixir.Value = true;
                NotifyDoubleElixirClientRpc();
            }

            // Aqui você pode atualizar UI, gerar recursos, etc
        }

        EndMatch();
    }
    
    private void EndMatch()
    {
        Debug.Log("Partida terminou!");
        EndMatchClientRpc(1);
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
        // Aqui você pode atualizar a UI dos clientes
    }

    public static PlayerController GetLocalPlayer()
    {
        return NetworkManager.Singleton.ConnectedClientsList
            .FirstOrDefault(c => c.ClientId == NetworkManager.Singleton.LocalClientId)?
            .PlayerObject.GetComponent<PlayerController>();
    }
}
