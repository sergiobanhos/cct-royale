using System.Collections;
using CctRoyale.Api;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchmakingManager : MonoBehaviour
{
    [SerializeField] private GameObject enterMatchButton;
    MatchmakingStatus currentMatchStatus;
    
    void Start()
    {
        StartCoroutine(MatchmakingCoroutine());
    }

    private IEnumerator MatchmakingCoroutine()
    {
        enterMatchButton.SetActive(false);

        yield return ApiClient.Instance.JoinMatchmaking((_) => Debug.Log("Joined matchmaking queue"));

        currentMatchStatus = null;

        while (currentMatchStatus == null || currentMatchStatus.status != "match_found")
        {
            yield return ApiClient.Instance.GetMatchmakingStatus((response) =>
            {
                if (response.success && response.data != null)
                {
                    Debug.Log($"Matchmaking status: {response.data.status}, Position: {response.data.position}/{response.data.queueSize}");
                    currentMatchStatus = response.data;
                }
            });
        }

        yield return new WaitForSeconds(8f); // Small delay to ensure transport is set up

        enterMatchButton.SetActive(true);

        // EnterMatch();
    }

    public void LeaveMatchmaking()
    {
        StopAllCoroutines();
        StartCoroutine(ApiClient.Instance.LeaveMatchmaking((_) => Debug.Log("Left matchmaking queue")));
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene("MenuScene");
    }

    public void EnterMatch()
    {
        if (currentMatchStatus == null || currentMatchStatus.status != "match_found")
        {
            Debug.LogError("Cannot enter match - no valid match found!");
            return;
        }
        
        Debug.Log($"[CLIENT] Attempting to connect to {currentMatchStatus.match.ip}:{currentMatchStatus.match.port}");

        // Verify transport configuration
        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        // transport.ConnectionData.Address = currentMatchStatus.match.ip;
        transport.ConnectionData.Port = (ushort)currentMatchStatus.match.port;
        Debug.Log($"[CLIENT] Transport Address: {transport.ConnectionData.Address}");
        Debug.Log($"[CLIENT] Transport Port: {transport.ConnectionData.Port}");
        Debug.Log($"[CLIENT] NetworkManager IsListening: {NetworkManager.Singleton.IsListening}");
        Debug.Log($"[CLIENT] NetworkManager IsServer: {NetworkManager.Singleton.IsServer}");
        Debug.Log($"[CLIENT] NetworkManager IsClient: {NetworkManager.Singleton.IsClient}");
        
        // Setup connection callbacks BEFORE starting client
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        NetworkManager.Singleton.OnTransportFailure += OnTransportFailure;
        
        bool started = NetworkManager.Singleton.StartClient();
        Debug.Log($"[CLIENT] StartClient returned: {started}");
        
        if (!started)
        {
            Debug.LogError("[CLIENT] Failed to start client!");
        }
    }
    
    private void OnTransportFailure()
    {
        Debug.LogError("[CLIENT] Transport failure detected!");
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"[CLIENT] ✓✓✓ Successfully connected! Client ID: {clientId}");
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.LogWarning($"[CLIENT] ✗✗✗ Disconnected from server. Client ID: {clientId}");
        
        // Cleanup callbacks
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            NetworkManager.Singleton.OnTransportFailure -= OnTransportFailure;
        }
    }
}
