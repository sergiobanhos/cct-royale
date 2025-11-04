using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace CctRoyale.GameServer
{
    [System.Serializable]
    public class GameServerResponse
    {
        public bool success;
        public string message;
    }

    [System.Serializable]
    public class StartMatchRequest
    {
        public string matchId;
    }

    [System.Serializable]
    public class FinishMatchRequest
    {
        public string matchId;
    }

    public class GameServerApiClient : MonoBehaviour
    {
        [Header("API Configuration")]
        public string baseUrl = "http://localhost:3000/api/game-server";
        public string gameServerToken = "game-server-secret-token-change-in-production";
        
        [Header("Debug")]
        public bool debugMode = true;

        public static GameServerApiClient Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            // Load game server token from environment or command line args
            LoadGameServerToken();
        }

        private void LoadGameServerToken()
        {
            // Try to get token from command line arguments
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length - 1; i++)
            {
                if (args[i] == "-gameServerToken")
                {
                    gameServerToken = args[i + 1];
                    if (debugMode)
                        Debug.Log("Game server token loaded from command line");
                    break;
                }
            }

            // You can also load from environment variables if needed
            string envToken = System.Environment.GetEnvironmentVariable("GAME_SERVER_TOKEN");
            if (!string.IsNullOrEmpty(envToken))
            {
                gameServerToken = envToken;
                if (debugMode)
                    Debug.Log("Game server token loaded from environment");
            }
        }

        public void StartMatch(string matchId, System.Action<bool, string> callback = null)
        {
            if (string.IsNullOrEmpty(matchId))
            {
                callback?.Invoke(false, "Match ID is required");
                return;
            }

            StartCoroutine(StartMatchCoroutine(matchId, callback));
        }

        public void FinishMatch(string matchId, System.Action<bool, string> callback = null)
        {
            if (string.IsNullOrEmpty(matchId))
            {
                callback?.Invoke(false, "Match ID is required");
                return;
            }

            StartCoroutine(FinishMatchCoroutine(matchId, callback));
        }

        private IEnumerator StartMatchCoroutine(string matchId, System.Action<bool, string> callback)
        {
            if (debugMode)
                Debug.Log($"Starting match: {matchId}");

            var request = new StartMatchRequest { matchId = matchId };
            
            yield return StartCoroutine(PostGameServerRequest("/match/start", request, (response) =>
            {
                if (response.success)
                {
                    if (debugMode)
                        Debug.Log($"Match started successfully: {matchId}");
                    
                    callback?.Invoke(true, response.message);
                }
                else
                {
                    if (debugMode)
                        Debug.LogError($"Failed to start match {matchId}: {response.message}");
                    
                    callback?.Invoke(false, response.message);
                }
            }));
        }

        private IEnumerator FinishMatchCoroutine(string matchId, System.Action<bool, string> callback)
        {
            if (debugMode)
                Debug.Log($"Finishing match: {matchId}");

            var request = new FinishMatchRequest { matchId = matchId };
            
            yield return StartCoroutine(PostGameServerRequest("/match/finish", request, (response) =>
            {
                if (response.success)
                {
                    if (debugMode)
                        Debug.Log($"Match finished successfully: {matchId}");
                    
                    callback?.Invoke(true, response.message);
                }
                else
                {
                    if (debugMode)
                        Debug.LogError($"Failed to finish match {matchId}: {response.message}");
                    
                    callback?.Invoke(false, response.message);
                }
            }));
        }

        private IEnumerator PostGameServerRequest<T>(string endpoint, T data, System.Action<GameServerResponse> callback)
        {
            string json = JsonUtility.ToJson(data);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

            using (UnityWebRequest request = new UnityWebRequest(baseUrl + endpoint, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("x-game-server-token", gameServerToken);

                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        string responseText = request.downloadHandler.text;
                        var response = JsonUtility.FromJson<GameServerResponse>(responseText);
                        callback?.Invoke(response);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("Failed to parse game server response: " + e.Message);
                        callback?.Invoke(new GameServerResponse { success = false, message = "Failed to parse response" });
                    }
                }
                else
                {
                    Debug.LogError("Game server request failed: " + request.error);
                    callback?.Invoke(new GameServerResponse { success = false, message = request.error });
                }
            }
        }
    }
}
