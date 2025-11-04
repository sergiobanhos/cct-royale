using Unity.Netcode;
using UnityEngine;

public class UI_StartMatchButton : MonoBehaviour
{
    public void EnterMatch()
    {
        NetworkManager.Singleton.StartClient();
    }
}
