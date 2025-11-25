using UnityEngine;

public class CardsContainerUI : MonoBehaviour
{
    [SerializeField] private CardsContainerItemUI cardItemPrefab;
    [SerializeField] private PlayerController playerController;

    private void Start()
    {
        Initialize(playerController.GetDeck());
    }


    public void Initialize(CardData[] cardsData)
    {
        cardItemPrefab.gameObject.SetActive(true);

        foreach (CardData cardData in cardsData)
        {
            CardsContainerItemUI cardItem = Instantiate(cardItemPrefab, this.transform);
            cardItem.gameObject.SetActive(true);
            cardItem.Initialize(cardData, (data) =>
            {
                playerController.SelectCard(data.id);
            });
        }

        cardItemPrefab.gameObject.SetActive(false);
    }
}
