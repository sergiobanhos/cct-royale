using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using CctRoyale.GameServer;

namespace CctRoyale.Server
{
    public class GameServerManager : MonoBehaviour
    {
        [Header("Match Configuration")]
        public string matchId;
        public ushort port = 7777;
        
        [Header("Scene Management")]
        public string menuSceneName = "MenuScene";
        public string battleSceneName = "BattleScene";
        
        [Header("Debug")]
        public bool debugMode = true;

        private bool matchStarted = false;
        private bool matchFinished = false;
        private int playerCount = 0;

        private void Start()
        {
            DontDestroyOnLoad(gameObject);

            // Check if this is a client build
            if (!Application.isBatchMode)
            {
                if (debugMode)
                    Debug.Log("Client build detected. Skipping server start.");
                
                SceneManager.LoadScene(menuSceneName);
                return;
            }

            if (debugMode)
                Debug.Log("Starting GameServerManager...");

            // Load configuration from command line arguments
            LoadServerConfiguration();
            
            if (!string.IsNullOrEmpty(matchId))
            {
                if (debugMode)
                    Debug.Log($"Game server started for match: {matchId} on port: {port}");
                
                // Setup and start the server
                StartServer();
            }
            else
            {
                Debug.LogError("No match ID provided! Server will not function properly.");
            }
        }

        private void LoadServerConfiguration()
        {
            string[] args = Environment.GetCommandLineArgs();
            
            // Get match ID
            matchId = GetArgValue(args, "-matchId") ?? "dev";
            
            // Get port
            string portArg = GetArgValue(args, "-port") ?? "7777";
            port = ushort.Parse(portArg);

            if (debugMode)
            {
                Debug.Log($"Match ID: {matchId}");
                Debug.Log($"Port: {port}");
            }
        }

        private void StartServer()
        {
            // Configure Unity Transport
            var transport = NetworkManager.Singleton.GetComponent<Unity.Netcode.Transports.UTP.UnityTransport>();
            transport.ConnectionData.Port = port;

            // Setup network callbacks
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

            // Start the server
            NetworkManager.Singleton.StartServer();

            if (debugMode)
                Debug.Log($"[GameServerManager] Server started - Match {matchId} | Port {port}");
        }

        private void OnClientConnected(ulong clientId)
        {
            playerCount++;
            
            if (debugMode)
                Debug.Log($"Client connected: {clientId} (total: {playerCount})");

            // When both players are connected, start the match
            if (playerCount == 2)
            {
                if (debugMode)
                    Debug.Log("Both players connected, starting match...");
                
                // Load battle scene
                NetworkManager.Singleton.SceneManager.LoadScene(battleSceneName, LoadSceneMode.Single);
                
                // Start the match
                StartMatch();
            }
        }

        private void OnClientDisconnected(ulong clientId)
        {
            playerCount--;
            
            if (debugMode)
                Debug.Log($"Client disconnected: {clientId} (total: {playerCount})");

            // If a player disconnects during the match, end it
            if (matchStarted && !matchFinished)
            {
                if (debugMode)
                    Debug.Log("Player disconnected during match, ending match...");
                
                EndMatch();
            }
        }

        public void StartMatch()
        {
            if (matchStarted)
            {
                if (debugMode)
                    Debug.LogWarning("Match already started");
                return;
            }

            if (string.IsNullOrEmpty(matchId))
            {
                Debug.LogError("Cannot start match: No match ID");
                return;
            }

            matchStarted = true;

            if (debugMode)
                Debug.Log($"Starting match: {matchId}");

            // Notify the API that the match has started
            if (GameServerApiClient.Instance != null)
            {
                GameServerApiClient.Instance.StartMatch(matchId, (success, message) =>
                {
                    if (success)
                    {
                        if (debugMode)
                            Debug.Log("Match start confirmed by API");
                        
                        OnMatchStarted();
                    }
                    else
                    {
                        Debug.LogError($"Failed to confirm match start with API: {message}");
                    }
                });
            }
            else
            {
                // If no API client, just start the match locally
                OnMatchStarted();
            }
        }

        public void FinishMatch()
        {
            if (matchFinished)
            {
                if (debugMode)
                    Debug.LogWarning("Match already finished");
                return;
            }

            if (string.IsNullOrEmpty(matchId))
            {
                Debug.LogError("Cannot finish match: No match ID");
                return;
            }

            matchFinished = true;

            if (debugMode)
                Debug.Log($"Finishing match: {matchId}");

            // Notify the API that the match has finished
            if (GameServerApiClient.Instance != null)
            {
                GameServerApiClient.Instance.FinishMatch(matchId, (success, message) =>
                {
                    if (success)
                    {
                        if (debugMode)
                            Debug.Log("Match finish confirmed by API");
                        
                        OnMatchFinished();
                    }
                    else
                    {
                        Debug.LogError($"Failed to confirm match finish with API: {message}");
                    }
                });
            }
            else
            {
                // If no API client, just finish the match locally
                OnMatchFinished();
            }
        }

        private void OnMatchStarted()
        {
            if (debugMode)
                Debug.Log("Match started! Game logic can begin.");
            
            // Add your match start logic here
            // For example:
            // - Enable player controls
            // - Start game timer
            // - Initialize game state
        }

        private void OnMatchFinished()
        {
            if (debugMode)
                Debug.Log("Match finished! Cleaning up...");
            
            // Stop the server
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                NetworkManager.Singleton.Shutdown();
            }
            
            // Add your match finish logic here
            // For example:
            // - Save match results
            // - Show end game screen
            // - Clean up resources
            
            // Quit the server after a delay
            StartCoroutine(QuitServerAfterDelay(10f));
        }

        private IEnumerator QuitServerAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (debugMode)
                Debug.Log("Shutting down game server...");
            
            Application.Quit();
        }

        // Call this when a player wins/loses or the match ends for any reason
        public void EndMatch()
        {
            if (!matchFinished)
            {
                FinishMatch();
            }
        }

        // Utility method to get command line argument values
        private static string GetArgValue(string[] args, string name)
        {
            int index = Array.IndexOf(args, name);
            if (index >= 0 && index < args.Length - 1)
                return args[index + 1];
            return null;
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && !matchFinished)
            {
                FinishMatch();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && !matchFinished)
            {
                FinishMatch();
            }
        }

        private void OnDestroy()
        {
            if (!Application.isBatchMode)
                {
                    Debug.Log("Client build detected. Skipping match end.");
                    return;
                }

            // Clean up network callbacks
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            }

            if (!matchFinished)
            {
                
                FinishMatch();
            }
        }
    }
}
