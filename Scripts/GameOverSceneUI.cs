using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverSceneUI : MonoBehaviour
{
    [Header("Texto de estatísticas")]
    [SerializeField] private TMP_Text statsText;

    [Header("Cenas")]
    [SerializeField] private string gameSceneName = "InfiniteRunner";
    [SerializeField] private string menuSceneName = "MainMenu";

    private bool isLoading;

    private void Start()
    {
        if (statsText == null)
        {
            Debug.LogError(
                "Preencha Stats Text no GameOverSceneUI."
            );

            return;
        }

        int proposals = LastRunData.HasResult
            ? LastRunData.ProposalPoints
            : 0;

        int distance = LastRunData.HasResult
            ? Mathf.FloorToInt(LastRunData.DistanceMeters)
            : 0;

        statsText.text =
            $"Propostas {proposals}\n" +
            $"Distância {distance}m";
    }

    public void PlayAgain()
    {
        LoadScene(gameSceneName);
    }

    public void MainMenu()
    {
        LoadScene(menuSceneName);
    }

    private void LoadScene(string sceneName)
    {
        if (isLoading)
            return;

        isLoading = true;

        LastRunData.Clear();
        SceneManager.LoadSceneAsync(sceneName);
    }

    public void OpenGlobalScore()
    {
        if (isLoading)
            return;

        isLoading = true;
        SceneManager.LoadSceneAsync("GlobalScore");
    }
}


