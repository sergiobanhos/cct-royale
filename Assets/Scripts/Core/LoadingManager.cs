using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class LoadingManager : MonoBehaviour
{
    public Slider progressBar;
    public TextMeshProUGUI loadingText;
    public string nextScene = "LoginScene";

    private float textAlpha = 1f;
    private bool fadingOut = true;

    void Start()
    {
        StartCoroutine(LoadAsyncOperation());
    }



    IEnumerator LoadAsyncOperation()
    {
        AsyncOperation gameLevel = SceneManager.LoadSceneAsync(nextScene);
        gameLevel.allowSceneActivation = false;

        string[] tips = {
            "Despertando os servos do rei...",
            "Afiando as espadas...",
            "Preparando o campo de batalha..."
        };
        int i = 0;

        while (!gameLevel.isDone)
        {
            float progress = Mathf.Clamp01(gameLevel.progress / 0.9f);
            progressBar.value = progress;

            // alterna texto de loading
            loadingText.text = tips[i % tips.Length];
            if (progress >= 0.33f * (i + 1)) i++;

            if (progress >= 1f)
            {
                yield return new WaitForSeconds(1f);
                gameLevel.allowSceneActivation = true;
            }
            yield return null;
        }
    }

    void Update()
    {
        if (loadingText != null)
        {
            // Piscar suave
            float speed = 2f; // quanto maior, mais rápido o efeito
            if (fadingOut)
                textAlpha -= Time.deltaTime * speed;
            else
                textAlpha += Time.deltaTime * speed;

            textAlpha = Mathf.Clamp01(textAlpha);

            if (textAlpha <= 0.2f) fadingOut = false;
            else if (textAlpha >= 1f) fadingOut = true;

            loadingText.alpha = textAlpha;
            loadingText.rectTransform.anchoredPosition = new Vector2(0, Mathf.Sin(Time.time * 2f) * 10f);
        }
    }
}
