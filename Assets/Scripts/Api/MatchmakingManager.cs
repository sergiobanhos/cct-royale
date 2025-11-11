using System.Collections;
using CctRoyale.Api;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MatchmakingManager : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(MatchmakingCoroutine());
    }

    private IEnumerator MatchmakingCoroutine()
    {
        yield return ApiClient.Instance.JoinMatchmaking((_) => Debug.Log("Joined matchmaking queue"));

        MatchmakingStatus matchmakingStatus = null;

        while (matchmakingStatus == null || matchmakingStatus.status == "in_queue")
        {
            yield return ApiClient.Instance.GetMatchmakingStatus((response) =>
            {
                if (response.success && response.data != null)
                {
                    Debug.Log($"Matchmaking status: {response.data.status}, Position: {response.data.position}/{response.data.queueSize}");
                    matchmakingStatus = response.data;
                }
            });
        }

        yield return new WaitForSeconds(8f); // Small delay to ensure transport is set up

        NetworkManager.Singleton.StartClient();
    }
    
    public void LeaveMatchmaking()
    {
        StopAllCoroutines();
        StartCoroutine(ApiClient.Instance.LeaveMatchmaking((_) => Debug.Log("Left matchmaking queue")));
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene("MenuScene");
    }
}
