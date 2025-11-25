using System;
using UnityEngine;
using UnityEngine.UI;

public class CardsContainerItemUI : MonoBehaviour
{
    [SerializeField] private Image cardImage;
    [SerializeField] private Button cardButton;

    public void Initialize(CardData cardData, Action<CardData> onClickAction = null)
    {
        cardImage.sprite = cardData.sprite;

        cardButton.onClick.RemoveAllListeners();
        cardButton.onClick.AddListener(() =>
        {
            onClickAction?.Invoke(cardData);
        });
    }
}
