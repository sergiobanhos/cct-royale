using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    [Header("Configurações")]
    [Tooltip("Câmera alvo. Se deixar vazio, usará a câmera principal")]
    public Camera targetCamera;
    
    [Tooltip("Inverter direção para corrigir texto espelhado")]
    public bool reverseDirection = true;

    private void Start()
    {
        // Se não definir uma câmera, usa a câmera principal
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    private void LateUpdate()
    {
        if (targetCamera == null) return;

        if (reverseDirection)
        {
            Vector3 directionToCamera = transform.position - targetCamera.transform.position;
            transform.rotation = Quaternion.LookRotation(directionToCamera);
        }
        else
        {
            transform.LookAt(targetCamera.transform);
        }
    }
}