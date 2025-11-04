using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

namespace CctRoyale.Api
{
    [System.Serializable]
    public class ApiResponse<T>
    {
        public bool success;
        public T data;
        public string message;
    }

    [System.Serializable]
    public class RegisterRequest
    {
        public string username;
        public string password;
    }

    [System.Serializable]
    public class LoginRequest
    {
        public string username;
        public string password;
    }

    [System.Serializable]
    public class LoginResponse
    {
        public string token;
        public UserData user;
    }

    [System.Serializable]
    public class UserData
    {
        public string id;
        public string username;
    }

    [System.Serializable]
    public class MatchmakingStatus
    {
        public string status; // "not_in_queue", "in_queue", "match_found"
        public int position;
        public int queueSize;
        public MatchData match;
    }

    [System.Serializable]
    public class MatchData
    {
        public string id;
        public string ip;
        public int port;
        public string status;
        public string playerOneId;
        public string playerTwoId;
    }

    [System.Serializable]
    public class JoinMatchmakingResponse
    {
        public string message;
        public int position;
    }

    public class ApiClient : MonoBehaviour
    {
        [Header("API Configuration")]
        public string baseUrl = "http://localhost:3000/api";
        
        private string authToken;
        
        public static ApiClient Instance { get; private set; }

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

        public void SetAuthToken(string token)
        {
            authToken = token;
        }

        public string GetAuthToken()
        {
            return authToken;
        }

        public bool IsAuthenticated()
        {
            return !string.IsNullOrEmpty(authToken);
        }

        // Auth endpoints
        public IEnumerator Register(string username, string password, System.Action<ApiResponse<UserData>> callback)
        {
            var request = new RegisterRequest { username = username, password = password };
            yield return StartCoroutine(PostRequest("/auth/register", request, callback));
        }

        public IEnumerator Login(string username, string password, System.Action<ApiResponse<LoginResponse>> callback)
        {
            var request = new LoginRequest { username = username, password = password };
            yield return StartCoroutine(PostRequest("/auth/login", request, (ApiResponse<LoginResponse> response) =>
            {
                if (response.success && response.data != null)
                {
                    SetAuthToken(response.data.token);
                }
                callback?.Invoke(response);
            }));
        }

        // Matchmaking endpoints
        public IEnumerator JoinMatchmaking(System.Action<ApiResponse<JoinMatchmakingResponse>> callback)
        {
            yield return StartCoroutine(PostRequestAuth<Null, JoinMatchmakingResponse>("/matchmaking/join", null, callback));
        }

        public IEnumerator LeaveMatchmaking(System.Action<ApiResponse<object>> callback)
        {
            yield return StartCoroutine(PostRequestAuth<Null, object>("/matchmaking/leave", null, callback));
        }

        public IEnumerator GetMatchmakingStatus(System.Action<ApiResponse<MatchmakingStatus>> callback)
        {
            yield return StartCoroutine(GetRequestAuth("/matchmaking/status", callback));
        }

        // Generic request methods
        private IEnumerator PostRequest<T, R>(string endpoint, T data, System.Action<ApiResponse<R>> callback)
        {
            string json = data != null ? JsonUtility.ToJson(data) : "{}";
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

            using (UnityWebRequest request = new UnityWebRequest(baseUrl + endpoint, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");

                yield return request.SendWebRequest();

                HandleResponse(request, callback);
            }
        }

        private IEnumerator PostRequestAuth<T, R>(string endpoint, T data, System.Action<ApiResponse<R>> callback)
        {
            if (!IsAuthenticated())
            {
                callback?.Invoke(new ApiResponse<R> { success = false, message = "Not authenticated" });
                yield break;
            }

            string json = data != null ? JsonUtility.ToJson(data) : "{}";
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

            using (UnityWebRequest request = new UnityWebRequest(baseUrl + endpoint, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", "Bearer " + authToken);

                yield return request.SendWebRequest();

                HandleResponse(request, callback);
            }
        }

        private IEnumerator GetRequestAuth<R>(string endpoint, System.Action<ApiResponse<R>> callback)
        {
            if (!IsAuthenticated())
            {
                callback?.Invoke(new ApiResponse<R> { success = false, message = "Not authenticated" });
                yield break;
            }

            using (UnityWebRequest request = UnityWebRequest.Get(baseUrl + endpoint))
            {
                request.SetRequestHeader("Authorization", "Bearer " + authToken);

                yield return request.SendWebRequest();

                HandleResponse(request, callback);
            }
        }

        private void HandleResponse<R>(UnityWebRequest request, System.Action<ApiResponse<R>> callback)
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    string responseText = request.downloadHandler.text;
                    var response = JsonUtility.FromJson<ApiResponse<R>>(responseText);
                    callback?.Invoke(response);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Failed to parse response: " + e.Message);
                    callback?.Invoke(new ApiResponse<R> { success = false, message = "Failed to parse response" });
                }
            }
            else
            {
                Debug.LogError("Request failed: " + request.error);
                callback?.Invoke(new ApiResponse<R> { success = false, message = request.error });
            }
        }
    }
}
