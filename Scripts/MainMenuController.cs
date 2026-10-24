using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField]
    private string gameSceneName = "InfiniteRunner";

    private bool isLoading;

    public void Play()
    {
        if (isLoading)
            return;

        isLoading = true;

        float soundDuration = 0f;

        if (GameAudio.Instance != null)
            soundDuration = GameAudio.Instance.PlayUrn();

        if (GameTransition.Instance != null)
        {
            bool accepted = GameTransition.Instance.LoadScene(
                gameSceneName,
                GameTransition.Style.Fade,
                soundDuration
            );

            if (!accepted)
                isLoading = false;
        }
        else
        {
            Debug.LogWarning(
                "GameTransition ausente. Abrindo sem transição."
            );

            SceneManager.LoadSceneAsync(gameSceneName);
        }
    }
}

