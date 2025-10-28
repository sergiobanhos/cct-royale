using UnityEngine;

public class UI_BottomBarButton : MonoBehaviour
{
    [SerializeField] private GameObject text;
    [SerializeField] private GameObject image;

    public void Show()
    {
        text.SetActive(true);
        image.transform.localScale = Vector3.one * 1.3f;
    }

    public void Hide()
    {
        text.SetActive(false);
        image.transform.localScale = Vector3.one;

    }
}
