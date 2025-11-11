using UnityEngine;
using TMPro;
using CctRoyale.Auth;

public class LoginManager : MonoBehaviour
{
    [Header("Referências de UI")]
    [SerializeField] private TMP_InputField usernameInput;
    [SerializeField] private TMP_InputField passwordInput;

    // Função chamada ao clicar no botão "Entrar"
    public void OnLoginButtonClick()
    {
        string username = usernameInput.text.Trim();
        string password = passwordInput.text.Trim();

        AuthManager.Instance.Login(username, password);
    }
}