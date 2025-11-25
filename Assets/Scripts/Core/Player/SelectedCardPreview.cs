using UnityEngine;

public class SelectedCardPreview : MonoBehaviour
{
    [SerializeField] private PlayerController playerController = null;
    private GameObject previewObject;


    private void Start()
    {
        playerController.HandleOnSelectedCardChanged += HandleOnSelectedCardChanged;
    }

    private void Update()
    {
        if (!previewObject) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector2 spawnPoint = new Vector2(hit.point.x, hit.point.z);
            previewObject.transform.position = new Vector3(spawnPoint.x, 0, spawnPoint.y);
        }
    }

    private void HandleOnSelectedCardChanged(string cardId)
    {
        CardData card = GameInstance.Instance.cardsContainer.GetCardById(cardId);
        UpdatePreview(card);
    }

    private void UpdatePreview(CardData card)
    {
        if (previewObject)
        {
            Destroy(previewObject);
        }

        if (card != null && card.previewPrefab != null)
        {
            previewObject = Instantiate(card.previewPrefab, Vector3.zero, Quaternion.identity);
        }
    }

}