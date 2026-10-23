using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalScoreSceneUI : MonoBehaviour
{
    [Header("Textos dos totais")]
    [SerializeField] private TMP_Text leftProposalsText;
    [SerializeField] private TMP_Text leftMetersText;
    [SerializeField] private TMP_Text rightProposalsText;
    [SerializeField] private TMP_Text rightMetersText;

    [Header("Status")]
    [SerializeField] private TMP_Text statusText;

    [Header("Navegação")]
    [SerializeField] private string gameSceneName = "InfiniteRunner";
    [SerializeField] private string menuSceneName = "MainMenu";

    private bool loading;
    private bool changingScene;

    private static readonly CultureInfo NumberCulture =
        CultureInfo.GetCultureInfo("pt-BR");

    private void Start()
    {
        Refresh();
    }

    public async void Refresh()
    {
        if (loading || changingScene)
            return;

        loading = true;

        SetStatus("Carregando placar...");
        ClearNumbers();

        try
        {
            // Se o envio da corrida ainda está em andamento,
            // aguarda antes de consultar os totais.
            try
            {
                await GlobalScoreSubmission.FlushAsync();
            }
            catch (Exception exception)
            {
                // Ainda tenta consultar os totais existentes.
                Debug.LogWarning(
                    $"Placar: existe envio pendente.\n{exception.Message}"
                );
            }

            if (this == null)
                return;

            GlobalScoreData score =
                await GlobalScoreService.GetAsync();

            if (this == null)
                return;

            leftProposalsText.text = Format(score.leftProposals);
            leftMetersText.text = Format(score.leftMeters);
            rightProposalsText.text = Format(score.rightProposals);
            rightMetersText.text = Format(score.rightMeters);

            SetStatus(
                GlobalScoreSubmission.PendingCount > 0
                    ? "Totais carregados. Sua partida ainda aguarda envio."
                    : ""
            );
        }
        catch (Exception exception)
        {
            if (this == null)
                return;

            SetStatus(
                "Placar indisponível. Tente atualizar novamente."
            );

            Debug.LogWarning(
                $"Placar: falha na consulta.\n{exception.Message}"
            );
        }
        finally
        {
            if (this != null)
                loading = false;
        }
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
        if (changingScene)
            return;

        changingScene = true;
        SceneManager.LoadSceneAsync(sceneName);
    }

    private void ClearNumbers()
    {
        leftProposalsText.text = "—";
        leftMetersText.text = "—";
        rightProposalsText.text = "—";
        rightMetersText.text = "—";
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
            statusText.text = message;
    }

    private static string Format(long value)
    {
        return value.ToString("N0", NumberCulture);
    }
}

