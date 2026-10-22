using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "InfiniteRunner";

    private bool isLoading;

    public void Play()
    {
        if (isLoading)
            return;

        isLoading = true;
        SceneManager.LoadSceneAsync(gameSceneName);
    }
}


