using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [Header("Fade opcional")]
    [SerializeField] private CanvasGroup fadeCanvas; // arraste o CanvasGroup do fade aqui

    private static SceneLoader instance;

    private void Awake()
    {
        // Garante que só existe um loader persistente
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadAsync(sceneName));
    }

    private IEnumerator LoadAsync(string sceneName)
    {
        // Inicia fade out
        if (fadeCanvas != null)
            yield return StartCoroutine(Fade(1f, 0.5f));

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        // Espera carregar até 90%
        while (operation.progress < 0.9f)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.3f); // breve pausa opcional

        // Ativa a cena
        operation.allowSceneActivation = true;

        // Espera a nova cena carregar completamente
        while (!operation.isDone)
        {
            yield return null;
        }

        // Fade in
        if (fadeCanvas != null)
            yield return StartCoroutine(Fade(0f, 0.5f));
    }

    private IEnumerator Fade(float target, float duration)
    {
        float start = fadeCanvas.alpha;
        float t = 0f;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            fadeCanvas.alpha = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }

        fadeCanvas.alpha = target;
    }
}
