using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_StartMatchButton : MonoBehaviour
{
    public void EnterMatch()
    {

        SceneManager.LoadScene("MatchmakingScene");
        // NetworkManager.Singleton.StartClient();
    }
}
