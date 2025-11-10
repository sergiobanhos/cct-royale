using UnityEngine;
using TMPro;

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

        // Apenas mostra no console (por enquanto)
        Debug.Log($"Usuário: {username}");
        Debug.Log($"Senha: {password}");

        // Cria um objeto para agrupar os dados (pode ser serializado depois)
        LoginData loginData = new LoginData(username, password);

        // Aqui futuramente você chamará o backend, exemplo:
        // StartCoroutine(ApiManager.Login(loginData));

        // Por enquanto só imprime
        Debug.Log(JsonUtility.ToJson(loginData));
    }
}

// Classe auxiliar para organizar os dados de login
[System.Serializable]
public class LoginData
{
    public string username;
    public string password;

    public LoginData(string username, string password)
    {
        this.username = username;
        this.password = password;
    }
}

//
