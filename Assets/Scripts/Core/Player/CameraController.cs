using Unity.Netcode;
using UnityEngine;

public class CameraController : NetworkBehaviour
{
    public GameObject cameraTeam1;
    public GameObject cameraTeam2;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Debug.Log("CameraController: Rodando no servidor, não ativa câmeras.");
            cameraTeam1.SetActive(false);
            cameraTeam2.SetActive(false);
            return; // O servidor não precisa ativar câmeras
        }

        PlayerController localPlayer = MatchController.GetLocalPlayer();
        if (localPlayer == null)
        {
            Debug.LogError("Local Player não encontrado!");
            return;
        }

        Debug.Log($"CameraController: Time do jogador local é {localPlayer.Team.Value}");

        // Ativa a câmera correta
        if (localPlayer.Team.Value == 1)
        {
            cameraTeam1.SetActive(true);
            cameraTeam2.SetActive(false);
        }
        else
        {
            cameraTeam1.SetActive(false);
            cameraTeam2.SetActive(true);
        }
    }
}
