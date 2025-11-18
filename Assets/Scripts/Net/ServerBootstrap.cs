using UnityEngine;
using Unity.Netcode;
using System;
using System.Linq;
using UnityEngine.SceneManagement;

public class ServerBootstrap : MonoBehaviour
{
    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);

        if (!Application.isBatchMode)
        {
            Debug.Log("Client build detected. Skipping server start.");
            SceneManager.LoadScene("MenuScene");
            return;
        }


        Debug.Log("Starting ServerBootstrap...");

        string[] args = Environment.GetCommandLineArgs();
        string matchId = GetArgValue(args, "-matchId") ?? "dev";
        string portArg = GetArgValue(args, "-port") ?? "7777";

        ushort port = ushort.Parse(portArg);

        var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
        transport.ConnectionData.Port = port;

        Debug.Log($"[ServerBootstrap] Match {matchId} | Port {port}");

        NetworkManager.Singleton.StartServer();
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private int playerCount = 0;

    private void OnClientConnected(ulong clientId)
    {
        playerCount++;
        Debug.Log($"Client connected: {clientId} (total: {playerCount})");

        if (playerCount == 2)
        {
            Debug.Log("Both players connected, starting match...");
            NetworkManager.Singleton.SceneManager.LoadScene("BattleScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        playerCount--;
        Debug.Log($"Client disconnected: {clientId} (total: {playerCount})");
    }

    private static string GetArgValue(string[] args, string name)
    {
        int index = Array.IndexOf(args, name);
        if (index >= 0 && index < args.Length - 1)
            return args[index + 1];
        return null;
    }
}
