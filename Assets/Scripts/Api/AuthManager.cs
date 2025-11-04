using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using CctRoyale.Api;

namespace CctRoyale.Auth
{
    public class AuthManager : MonoBehaviour
    {
        [Header("Events")]
        public UnityEvent<string> OnLoginSuccess;
        public UnityEvent<string> OnLoginFailed;
        public UnityEvent<string> OnRegisterSuccess;
        public UnityEvent<string> OnRegisterFailed;
        public UnityEvent OnLogout;

        [Header("Debug")]
        public bool debugMode = true;

        public static AuthManager Instance { get; private set; }

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

        public void Register(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                OnRegisterFailed?.Invoke("Username and password are required");
                return;
            }

            StartCoroutine(RegisterCoroutine(username, password));
        }

        public void Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                OnLoginFailed?.Invoke("Username and password are required");
                return;
            }

            StartCoroutine(LoginCoroutine(username, password));
        }

        public void Logout()
        {
            ApiClient.Instance.SetAuthToken(null);
            OnLogout?.Invoke();
            
            if (debugMode)
                Debug.Log("User logged out");
        }

        public bool IsLoggedIn()
        {
            return ApiClient.Instance != null && ApiClient.Instance.IsAuthenticated();
        }

        private IEnumerator RegisterCoroutine(string username, string password)
        {
            if (debugMode)
                Debug.Log($"Attempting to register user: {username}");

            yield return StartCoroutine(ApiClient.Instance.Register(username, password, (response) =>
            {
                if (response.success)
                {
                    if (debugMode)
                        Debug.Log($"Registration successful for user: {username}");
                    
                    OnRegisterSuccess?.Invoke($"Registration successful! Welcome {username}");
                }
                else
                {
                    if (debugMode)
                        Debug.LogError($"Registration failed: {response.message}");
                    
                    OnRegisterFailed?.Invoke(response.message ?? "Registration failed");
                }
            }));
        }

        private IEnumerator LoginCoroutine(string username, string password)
        {
            if (debugMode)
                Debug.Log($"Attempting to login user: {username}");

            yield return StartCoroutine(ApiClient.Instance.Login(username, password, (response) =>
            {
                if (response.success && response.data != null)
                {
                    if (debugMode)
                        Debug.Log($"Login successful for user: {response.data.user.username}");
                    
                    OnLoginSuccess?.Invoke($"Welcome back, {response.data.user.username}!");
                }
                else
                {
                    if (debugMode)
                        Debug.LogError($"Login failed: {response.message}");
                    
                    OnLoginFailed?.Invoke(response.message ?? "Login failed");
                }
            }));
        }
    }
}
