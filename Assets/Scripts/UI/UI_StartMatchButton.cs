using Unity.Netcode;
using UnityEngine;

public class UI_StartMatchButton : MonoBehaviour
{
    public void EnterMatch()
    {

        SceneLoader.Instance.LoadScene("MatchmakingScene");
        NetworkManager.Singleton.StartClient();
    }
}
