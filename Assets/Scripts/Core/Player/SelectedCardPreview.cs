using UnityEngine;

public class SelectedCardPreview : MonoBehaviour
{
    [SerializeField] private PlayerController playerController = null;
    private GameObject previewObject;


    private void Start()
    {
        playerController.OnSelectedCardChanged += HandleOnSelectedCardChanged;
    }

    private void Update()
    {
        if (!previewObject) return;

        var mouseData = PlayerController.GetMousePosition();

        if (mouseData.isValid)
        {
            if (previewObject.activeSelf == false)
            {
                previewObject.SetActive(true);
            }

            previewObject.transform.position = mouseData.position;
        }
        else
        {
            if (previewObject.activeSelf == true)
            {
                previewObject.SetActive(false);
            }
        }
    }

    private void HandleOnSelectedCardChanged(int cardIndex)
    {
        string cardId = playerController.GetSelectedCardId();
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